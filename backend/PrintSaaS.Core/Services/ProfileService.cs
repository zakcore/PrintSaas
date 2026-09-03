using PrintSaaS.Data.Repositories;
using PrintSaaS.Models;

namespace PrintSaaS.Core.Services;

public interface IProfileService
{
    Task<List<QueueProfile>> GetQueueProfilesAsync(int? printerId = null);
    Task<QueueProfile?> GetQueueProfileByIdAsync(int id);
    Task<QueueProfile> CreateQueueProfileAsync(QueueProfile profile);
    Task<bool> UpdateQueueProfileAsync(int id, QueueProfile updated);
    Task<bool> DeactivateQueueProfileAsync(int id);
    Task<QueueProfile?> SuggestProfileAsync(int printerId, bool isDuplex, string colorMode);
    Task<List<TrayProfile>> GetTrayProfilesAsync(int? printerId = null);
    Task<TrayProfile?> GetTrayProfileByIdAsync(int id);
    Task<TrayProfile> CreateTrayProfileAsync(TrayProfile profile);
    Task<bool> UpdateTrayProfileAsync(int id, TrayProfile updated);
    Task<bool> DeactivateTrayProfileAsync(int id);
    Task<List<MachineQueueName>> GetMachineQueueNamesAsync(int printerId);
}

public class ProfileService : IProfileService
{
    private readonly IPrinterRepository _repo;

    public ProfileService(IPrinterRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<QueueProfile>> GetQueueProfilesAsync(int? printerId = null)
    {
        if (printerId.HasValue)
            return await _repo.GetQueueProfilesAsync(printerId.Value);

        var printers = await _repo.GetAllAsync();
        var profiles = new List<QueueProfile>();
        foreach (var printer in printers)
            profiles.AddRange(await _repo.GetQueueProfilesAsync(printer.Id));
        return profiles;
    }

    public async Task<QueueProfile?> GetQueueProfileByIdAsync(int id)
    {
        return await _repo.GetQueueProfileByIdAsync(id);
    }

    public async Task<QueueProfile> CreateQueueProfileAsync(QueueProfile profile)
    {
        profile.IsActive = true;
        return await _repo.CreateQueueProfileAsync(profile);
    }

    public async Task<bool> UpdateQueueProfileAsync(int id, QueueProfile updated)
    {
        var profile = await _repo.GetQueueProfileByIdAsync(id);
        if (profile is null) return false;

        profile.DisplayName = updated.DisplayName;
        profile.MachineQueueName = updated.MachineQueueName;
        profile.IsDuplex = updated.IsDuplex;
        profile.ColorMode = updated.ColorMode;
        profile.Priority = updated.Priority;
        profile.HoldOnArrival = updated.HoldOnArrival;
        profile.TrayProfileId = updated.TrayProfileId;

        await _repo.UpdateQueueProfileAsync(profile);
        return true;
    }

    public async Task<bool> DeactivateQueueProfileAsync(int id)
    {
        var profile = await _repo.GetQueueProfileByIdAsync(id);
        if (profile is null) return false;

        profile.IsActive = false;
        await _repo.UpdateQueueProfileAsync(profile);
        return true;
    }

    public async Task<QueueProfile?> SuggestProfileAsync(int printerId, bool isDuplex, string colorMode)
    {
        var profiles = await _repo.GetQueueProfilesAsync(printerId, activeOnly: true);
        return profiles.FirstOrDefault(p => p.IsDuplex == isDuplex && p.ColorMode == colorMode)
            ?? profiles.FirstOrDefault(p => p.ColorMode == colorMode);
    }

    public async Task<List<TrayProfile>> GetTrayProfilesAsync(int? printerId = null)
    {
        if (printerId.HasValue)
            return await _repo.GetTrayProfilesAsync(printerId.Value);

        var printers = await _repo.GetAllAsync();
        var profiles = new List<TrayProfile>();
        foreach (var printer in printers)
            profiles.AddRange(await _repo.GetTrayProfilesAsync(printer.Id));
        return profiles;
    }

    public async Task<TrayProfile?> GetTrayProfileByIdAsync(int id)
    {
        return await _repo.GetTrayProfileByIdAsync(id);
    }

    public async Task<TrayProfile> CreateTrayProfileAsync(TrayProfile profile)
    {
        profile.IsActive = true;
        return await _repo.CreateTrayProfileAsync(profile);
    }

    public async Task<bool> UpdateTrayProfileAsync(int id, TrayProfile updated)
    {
        var profile = await _repo.GetTrayProfileByIdAsync(id);
        if (profile is null) return false;

        profile.TrayNumber = updated.TrayNumber;
        profile.PaperType = updated.PaperType;
        profile.PaperSize = updated.PaperSize;
        profile.PaperWeight = updated.PaperWeight;

        await _repo.UpdateTrayProfileAsync(profile);
        return true;
    }

    public async Task<bool> DeactivateTrayProfileAsync(int id)
    {
        var profile = await _repo.GetTrayProfileByIdAsync(id);
        if (profile is null) return false;

        profile.IsActive = false;
        await _repo.UpdateTrayProfileAsync(profile);
        return true;
    }

    public async Task<List<MachineQueueName>> GetMachineQueueNamesAsync(int printerId)
    {
        return await _repo.GetMachineQueueNamesAsync(printerId);
    }
}
