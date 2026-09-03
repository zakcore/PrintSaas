# PrintSaaS

A transactional print-production management system built for a Montreal print shop that
produces high-volume documents (bank statements, payroll cheques, insurance documents) for
banks and insurance companies. It replaces two legacy applications — an ASP.NET WebForms work
order tool and a WinForms print queue manager — with a modern web app that talks to Xerox
production printers directly over **IPP/IPPS** instead of the legacy **LPR** protocol.

## The Problem

The legacy print queue manager sent large payroll files (800+ pages) to the production printer
over LPR — a byte-by-byte handshaking protocol from 1988 — causing **30-minute delays** before
a job even started printing. Operators also worked from a paper printout ("bon de travail"),
manually matching print settings to a queue profile by hand for every job.

## The Solution

- **IPPS (IPP over TLS) instead of LPR** for all printer communication — eliminates the
  handshake delay and encrypts job data in transit, which matters when the payload is bank
  statements and salary cheques.
- **Dynamic printer fleet** — printers, their IPP queue names, and paper tray configurations
  all live in the database. An admin can add a brand-new IPP-capable printer through the UI
  with zero code changes and zero downtime.
- **Automatic profile suggestion** — a rules engine reads a job's parameters (duplex, color
  mode, paper type) and pre-selects the matching print queue profile instead of an operator
  reading settings off a paper printout.
- **Payroll-specific safety checks** — page-count validation against `employees × pages/employee`,
  and a job-sequencing guard that requires operator confirmation before a payroll/cheque run
  follows a non-payroll job on the same printer within 30 minutes (a real failure mode when
  cheques accidentally print on the wrong stock).
- **Real-time dashboard** — printer status, tray paper levels, and job state pushed to every
  connected client over SignalR, no polling.
- **Audit trail** — every job send, profile choice, and operator confirmation is written to an
  append-only compliance log.

## Architecture

```
backend/
  PrintSaaS.API/       ASP.NET Core 9 Web API — controllers, JWT auth, SignalR hub, Hangfire jobs
  PrintSaaS.Engine/     IPP/IPPS job submission, SNMP tray monitoring, AES-256 file encryption
  PrintSaaS.Core/       Application services (jobs, printers, profiles, auth, users)
  PrintSaaS.Data/       EF Core DbContext, migrations, repositories
  PrintSaaS.Models/     Shared domain models and enums
  PrintSaaS.Rules/      NRules-based business rules engine (payroll safety, color/duplex checks)

frontend/
  React 19 + TypeScript, Vite, TanStack Query, Zustand, Tailwind, SignalR client
```

## Tech Stack

**Backend** — .NET 9, ASP.NET Core Web API, Entity Framework Core, SQL Server, SignalR,
Hangfire, SharpIpp (IPP/IPPS), SnmpSharpNet, NRules, Serilog, JWT Bearer auth, BCrypt.

**Frontend** — React 19, TypeScript, Vite, TanStack Query, Zustand, React Router, Axios,
Tailwind CSS, react-i18next (French/English), Recharts.

## Key Design Decisions

- **Two-name queue system** — printers have their own pre-configured queue names on the Xerox
  DFE controller (e.g. `Duplex-BW-Letter`). The app discovers these via IPP and maps them to a
  friendly operator-facing name, so the raw machine queue name is what's actually used on the
  wire, never a value entered by hand.
- **No hardcoded printers or queue names** — everything is discovered or admin-configured and
  stored in the database, so onboarding a new printer is a UI operation, not a deployment.
- **Result-pattern services, no exceptions across layers** — application services return
  typed results rather than throwing, keeping error handling explicit at the API boundary.
- **Encryption at rest, decrypt in memory only** — uploaded PDFs are stored AES-256 encrypted
  on disk and are only decrypted in memory immediately before being streamed to the printer.
- **Bilingual UI** — French-first (Montreal-based operators), with English as the secondary
  language via `react-i18next`.

## Running Locally

### Backend
```bash
cd backend
dotnet restore
dotnet ef database update --project PrintSaaS.Data --startup-project PrintSaaS.API
dotnet run --project PrintSaaS.API
```

Set the following via environment variables (or user-secrets) before running — do **not**
put real values in `appsettings.json`:

```
ConnectionStrings__DefaultConnection
Jwt__Key
Encryption__Key
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

The dev server runs at `http://localhost:5173` and expects the API at the URL configured in
`frontend/src/services/api.ts`.

## Status

This is a portfolio/learning project modeling a real production print workflow. It is not
connected to a live print fleet or real client data — printer IPs, hostnames, and seed data in
this repo are placeholders.
