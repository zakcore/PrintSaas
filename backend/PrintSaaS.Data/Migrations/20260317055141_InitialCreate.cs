using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PrintSaaS.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Printers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Protocol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultQueueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ColorCapability = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastSeenOnline = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Printers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineQueueNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrinterId = table.Column<int>(type: "int", nullable: false),
                    QueueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiscoveredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineQueueNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineQueueNames_Printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "Printers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrayProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrinterId = table.Column<int>(type: "int", nullable: false),
                    TrayNumber = table.Column<int>(type: "int", nullable: false),
                    PaperType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaperSize = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaperWeight = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrayProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrayProfiles_Printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "Printers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueueProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MachineQueueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrinterId = table.Column<int>(type: "int", nullable: false),
                    IsDuplex = table.Column<bool>(type: "bit", nullable: false),
                    ColorMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    HoldOnArrival = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TrayProfileId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueProfiles_Printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "Printers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueueProfiles_TrayProfiles_TrayProfileId",
                        column: x => x.TrayProfileId,
                        principalTable: "TrayProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileLocation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrinterId = table.Column<int>(type: "int", nullable: true),
                    QueueProfileId = table.Column<int>(type: "int", nullable: true),
                    OperatorId = table.Column<int>(type: "int", nullable: true),
                    PrinterJobId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "Printers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobs_QueueProfiles_QueueProfileId",
                        column: x => x.QueueProfileId,
                        principalTable: "QueueProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobs_Users_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComplianceNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEntries_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditEntries_Users_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperatorId = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobHistories_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobHistories_Users_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    IsDuplex = table.Column<bool>(type: "bit", nullable: false),
                    ColorMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Copies = table.Column<int>(type: "int", nullable: false),
                    TotalPageCount = table.Column<int>(type: "int", nullable: false),
                    IsPayrollJob = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeCount = table.Column<int>(type: "int", nullable: true),
                    PagesPerEmployee = table.Column<int>(type: "int", nullable: true),
                    ContainsCheques = table.Column<bool>(type: "bit", nullable: false),
                    ContainsStubs = table.Column<bool>(type: "bit", nullable: false),
                    PaperType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobParameters_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Printers",
                columns: new[] { "Id", "ColorCapability", "DefaultQueueName", "IpAddress", "IsActive", "IsOnline", "LastSeenOnline", "Model", "Name", "Port", "Protocol", "Status" },
                values: new object[,]
                {
                    { 1, "BW", "Nuvera", "192.168.1.101", true, false, null, "Xerox Nuvera 144MX", "Nuvera 144MX", 443, "IPPS", null },
                    { 2, "Color", "Brenva", "192.168.1.102", true, false, null, "Xerox Brenva HD", "Brenva HD", 443, "IPPS", null },
                    { 3, "Both", "Iridesse", "192.168.1.103", true, false, null, "Xerox Iridesse", "Iridesse", 443, "IPPS", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "DisplayName", "IsActive", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrateur", true, "$2a$11$placeholder_will_be_set_at_runtime", "Admin", "admin" });

            migrationBuilder.InsertData(
                table: "QueueProfiles",
                columns: new[] { "Id", "ColorMode", "DisplayName", "HoldOnArrival", "IsActive", "IsDuplex", "MachineQueueName", "PrinterId", "Priority", "TrayProfileId" },
                values: new object[] { 4, "Color", "Color Duplex", false, true, true, "Color-Duplex-Letter", 2, 1, null });

            migrationBuilder.InsertData(
                table: "TrayProfiles",
                columns: new[] { "Id", "IsActive", "PaperSize", "PaperType", "PaperWeight", "PrinterId", "TrayNumber" },
                values: new object[,]
                {
                    { 1, true, "Letter", "Plain", "75gsm", 1, 1 },
                    { 2, true, "Letter", "Bond", "90gsm", 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "QueueProfiles",
                columns: new[] { "Id", "ColorMode", "DisplayName", "HoldOnArrival", "IsActive", "IsDuplex", "MachineQueueName", "PrinterId", "Priority", "TrayProfileId" },
                values: new object[,]
                {
                    { 1, "BW", "PDF Duplex B&W", false, true, true, "Duplex-BW-Letter", 1, 1, 1 },
                    { 2, "BW", "PDF Simplex B&W", false, true, false, "Simplex-BW-Letter", 1, 2, 1 },
                    { 3, "BW", "Cheque Run", false, true, true, "Cheque-Duplex", 1, 1, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_JobId",
                table: "AuditEntries",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_OperatorId",
                table: "AuditEntries",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_Timestamp",
                table: "AuditEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_JobHistories_JobId",
                table: "JobHistories",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobHistories_OperatorId",
                table: "JobHistories",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_JobParameters_JobId",
                table: "JobParameters",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CreatedAt",
                table: "Jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OperatorId",
                table: "Jobs",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PrinterId",
                table: "Jobs",
                column: "PrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_QueueProfileId",
                table: "Jobs",
                column: "QueueProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                table: "Jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MachineQueueNames_PrinterId",
                table: "MachineQueueNames",
                column: "PrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueProfiles_PrinterId",
                table: "QueueProfiles",
                column: "PrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueProfiles_TrayProfileId",
                table: "QueueProfiles",
                column: "TrayProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayProfiles_PrinterId",
                table: "TrayProfiles",
                column: "PrinterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "JobHistories");

            migrationBuilder.DropTable(
                name: "JobParameters");

            migrationBuilder.DropTable(
                name: "MachineQueueNames");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "QueueProfiles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TrayProfiles");

            migrationBuilder.DropTable(
                name: "Printers");
        }
    }
}
