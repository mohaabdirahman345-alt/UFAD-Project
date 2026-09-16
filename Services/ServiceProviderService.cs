using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Repositories;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class ServiceProviderService : IServiceProviderService
{
    private const int MaxPageSize = 50;

    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public ServiceProviderService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<ServiceProviderSummaryViewModel>> SearchAsync(
        string? keyword, int? categoryId, string? address, double? minimumRating,
        AvailabilityStatus? availabilityStatus, int pageNumber, int pageSize)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize is < 1 or > MaxPageSize ? 12 : pageSize;

        var (results, totalCount) = await _unitOfWork.ServiceProviders.SearchAsync(
            keyword, categoryId, address, minimumRating, availabilityStatus, pageNumber, pageSize);

        var items = results.Select(MapToSummary).ToList();

        return new PagedResult<ServiceProviderSummaryViewModel>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ServiceProviderDetailViewModel?> GetDetailsAsync(int serviceProviderProfileId)
    {
        var profile = await _unitOfWork.ServiceProviders.GetProfileWithDetailsAsync(serviceProviderProfileId);
        return profile is null ? null : MapToDetail(profile);
    }

    public Task<ServiceProviderProfile?> GetByUserIdAsync(string userId) =>
        _unitOfWork.ServiceProviders.GetProfileByUserIdAsync(userId);

    public async Task<IReadOnlyList<ServiceProviderSummaryViewModel>> GetTopRatedAsync(int count) =>
        (await _unitOfWork.ServiceProviders.GetTopRatedAsync(count))
            .Select(MapToSummary)
            .ToList();

    public async Task<IReadOnlyList<ServiceProviderSummaryViewModel>> GetAllForAdminAsync() =>
        (await _unitOfWork.ServiceProviders.GetAllForAdminAsync())
            .Select(MapToSummary)
            .ToList();

    public async Task<ServiceProviderProfile> CreateProfileAsync(string userId, ServiceProviderProfileEditInput input)
    {
        var profile = new ServiceProviderProfile
        {
            UserId = userId,
            Headline = input.Headline.Trim(),
            Bio = input.Bio.Trim(),
            CategoryId = input.CategoryId,
            YearsOfExperience = input.YearsOfExperience,
            HourlyRate = input.HourlyRate,
            Address = input.Address.Trim(),
            PhoneNumber = input.PhoneNumber.Trim(),
            AvailabilityStatus = input.AvailabilityStatus,
            ApprovalStatus = ApprovalStatus.Draft
        };

        await _unitOfWork.ServiceProviders.AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();

        await SyncSkillsAsync(profile.Id, input.SkillIds);
        await _unitOfWork.SaveChangesAsync();

        return profile;
    }

    public async Task UpdateProfileAsync(int serviceProviderProfileId, ServiceProviderProfileEditInput input)
    {
        var profile = await _unitOfWork.ServiceProviders.GetByIdAsync(serviceProviderProfileId)
            ?? throw new InvalidOperationException($"Service provider profile {serviceProviderProfileId} was not found.");

        profile.Headline = input.Headline.Trim();
        profile.Bio = input.Bio.Trim();
        profile.CategoryId = input.CategoryId;
        profile.YearsOfExperience = input.YearsOfExperience;
        profile.HourlyRate = input.HourlyRate;
        profile.Address = input.Address.Trim();
        profile.PhoneNumber = input.PhoneNumber.Trim();
        profile.AvailabilityStatus = input.AvailabilityStatus;

        _unitOfWork.ServiceProviders.Update(profile);
        await SyncSkillsAsync(serviceProviderProfileId, input.SkillIds);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetVerifiedAsync(int serviceProviderProfileId, bool isVerified, string changedByUserId)
    {
        var profile = await GetOrThrowAsync(serviceProviderProfileId);
        profile.IsVerified = isVerified;

        // Revoking verification removes the provider from the public directory.
        if (!isVerified && profile.ApprovalStatus == ApprovalStatus.Approved)
        {
            var previousStatus = profile.ApprovalStatus;
            profile.ApprovalStatus = ApprovalStatus.Rejected;
            profile.ApprovalDecisionOn = DateTime.UtcNow;
            profile.ApprovalNotes = "Verification revoked — removed from public directory.";

            _unitOfWork.ServiceProviders.Update(profile);
            await AddApprovalHistoryAsync(
                serviceProviderProfileId,
                previousStatus,
                ApprovalStatus.Rejected,
                changedByUserId,
                profile.ApprovalNotes);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.NotifyUserAsync(
                profile.UserId,
                NotificationType.ProfileRejected,
                "Verification revoked",
                "Your verification was revoked and your profile was removed from the public directory.",
                "/ServiceProvider/Profile/Edit",
                "ServiceProviderProfile",
                serviceProviderProfileId);
            return;
        }

        _unitOfWork.ServiceProviders.Update(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SubmitForApprovalAsync(int serviceProviderProfileId, string submittedByUserId)
    {
        var profile = await GetOrThrowAsync(serviceProviderProfileId);

        if (profile.ApprovalStatus is ApprovalStatus.PendingApproval or ApprovalStatus.Approved)
        {
            return;
        }

        var previousStatus = profile.ApprovalStatus;
        profile.ApprovalStatus = ApprovalStatus.PendingApproval;
        profile.SubmittedForApprovalOn = DateTime.UtcNow;
        profile.ApprovalNotes = null;

        _unitOfWork.ServiceProviders.Update(profile);
        await AddApprovalHistoryAsync(serviceProviderProfileId, previousStatus, ApprovalStatus.PendingApproval, null, "Submitted for review.");
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.NotifyAdminsAsync(
            NotificationType.ProfilePendingApproval,
            "Profile pending approval",
            $"A service provider submitted a profile for review.",
            "/Admin/Approvals",
            "ServiceProviderProfile",
            serviceProviderProfileId);
    }

    public async Task ApproveAsync(int serviceProviderProfileId, string approvedByUserId, string? notes)
    {
        var profile = await GetOrThrowAsync(serviceProviderProfileId);

        var previousStatus = profile.ApprovalStatus;
        profile.ApprovalStatus = ApprovalStatus.Approved;
        profile.ApprovalDecisionOn = DateTime.UtcNow;
        profile.ApprovalNotes = notes;

        _unitOfWork.ServiceProviders.Update(profile);
        await AddApprovalHistoryAsync(serviceProviderProfileId, previousStatus, ApprovalStatus.Approved, approvedByUserId, notes);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.NotifyUserAsync(
            profile.UserId,
            NotificationType.ProfileApproved,
            "Profile approved",
            "Your profile was approved and is now eligible for the directory.",
            "/ServiceProvider/Profile/Edit",
            "ServiceProviderProfile",
            serviceProviderProfileId);
    }

    public async Task RejectAsync(int serviceProviderProfileId, string rejectedByUserId, string? notes)
    {
        var profile = await GetOrThrowAsync(serviceProviderProfileId);

        var previousStatus = profile.ApprovalStatus;
        profile.ApprovalStatus = ApprovalStatus.Rejected;
        profile.IsVerified = false;
        profile.ApprovalDecisionOn = DateTime.UtcNow;
        profile.ApprovalNotes = notes;

        _unitOfWork.ServiceProviders.Update(profile);
        await AddApprovalHistoryAsync(serviceProviderProfileId, previousStatus, ApprovalStatus.Rejected, rejectedByUserId, notes);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.NotifyUserAsync(
            profile.UserId,
            NotificationType.ProfileRejected,
            "Profile rejected",
            string.IsNullOrWhiteSpace(notes)
                ? "Your profile was rejected. Update it and resubmit for review."
                : $"Your profile was rejected: {notes.Trim()}",
            "/ServiceProvider/Profile/Edit",
            "ServiceProviderProfile",
            serviceProviderProfileId);
    }

    public async Task<IReadOnlyList<StatusHistoryEntryViewModel>> GetApprovalHistoryAsync(int serviceProviderProfileId) =>
        (await _unitOfWork.ServiceProviderApprovalHistories.Query()
            .Include(h => h.ChangedByUser)
            .Where(h => h.ServiceProviderProfileId == serviceProviderProfileId)
            .OrderBy(h => h.ChangedOn)
            .ToListAsync())
        .Select(h => new StatusHistoryEntryViewModel
        {
            FromStatus = h.FromStatus?.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedOn = h.ChangedOn,
            ChangedByName = h.ChangedByUser?.FullName ?? "Service provider (self-submitted)",
            Notes = h.Notes
        })
        .ToList();

    private async Task AddApprovalHistoryAsync(int serviceProviderProfileId, ApprovalStatus fromStatus, ApprovalStatus toStatus, string? changedByUserId, string? notes)
    {
        await _unitOfWork.ServiceProviderApprovalHistories.AddAsync(new ServiceProviderApprovalHistory
        {
            ServiceProviderProfileId = serviceProviderProfileId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedByUserId = changedByUserId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        });
    }

    public async Task IncrementProfileViewsAsync(int serviceProviderProfileId)
    {
        var profile = await _unitOfWork.ServiceProviders.GetByIdAsync(serviceProviderProfileId);
        if (profile is null)
        {
            return;
        }

        profile.ProfileViews++;
        _unitOfWork.ServiceProviders.Update(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkNotificationsViewedAsync(int serviceProviderProfileId)
    {
        var profile = await GetOrThrowAsync(serviceProviderProfileId);
        profile.NotificationsViewedOn = DateTime.UtcNow;
        _unitOfWork.ServiceProviders.Update(profile);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<ServiceProviderProfile> GetOrThrowAsync(int serviceProviderProfileId) =>
        await _unitOfWork.ServiceProviders.GetByIdAsync(serviceProviderProfileId)
            ?? throw new InvalidOperationException($"Service provider profile {serviceProviderProfileId} was not found.");

    /// <summary>
    /// Replaces the service provider's claimed skills with the given set. Doing
    /// this as a diff (rather than remove-all-then-reinsert) keeps existing
    /// ServiceProviderSkill rows — and any future per-skill metadata — intact
    /// for skills that remain selected.
    /// </summary>
    private async Task SyncSkillsAsync(int serviceProviderProfileId, IReadOnlyCollection<int> skillIds)
    {
        var existing = await _unitOfWork.ServiceProviderSkills.FindAsync(fs => fs.ServiceProviderProfileId == serviceProviderProfileId);
        var existingIds = existing.Select(fs => fs.SkillId).ToHashSet();
        var desiredIds = skillIds.ToHashSet();

        foreach (var toRemove in existing.Where(fs => !desiredIds.Contains(fs.SkillId)))
        {
            _unitOfWork.ServiceProviderSkills.Remove(toRemove);
        }

        foreach (var skillId in desiredIds.Where(id => !existingIds.Contains(id)))
        {
            await _unitOfWork.ServiceProviderSkills.AddAsync(new ServiceProviderSkill
            {
                ServiceProviderProfileId = serviceProviderProfileId,
                SkillId = skillId
            });
        }
    }

    /// <summary>
    /// A simple filled-fields-out-of-eight percentage shown on the provider's
    /// own dashboard to nudge them toward a complete profile before submitting
    /// for approval. Not stored — always computed fresh from current data.
    /// </summary>
    private static int CalculateProfileCompletion(ServiceProviderProfile profile)
    {
        var checks = new[]
        {
            !string.IsNullOrWhiteSpace(profile.User.ProfileImagePath),
            !string.IsNullOrWhiteSpace(profile.Bio),
            !string.IsNullOrWhiteSpace(profile.PhoneNumber),
            !string.IsNullOrWhiteSpace(profile.Address),
            profile.YearsOfExperience > 0,
            profile.HourlyRate.HasValue,
            profile.ServiceProviderSkills.Count > 0,
            profile.PortfolioItems.Count > 0
        };

        var completed = checks.Count(passed => passed);
        return (int)Math.Round(completed / (double)checks.Length * 100);
    }

    private static ServiceProviderSummaryViewModel MapToSummary(ServiceProviderProfile profile) => new()
    {
        Id = profile.Id,
        FullName = profile.User.FullName,
        ProfileImagePath = profile.User.ProfileImagePath,
        Headline = profile.Headline,
        CategoryName = profile.Category.Name,
        Address = profile.Address,
        HourlyRate = profile.HourlyRate,
        IsVerified = profile.IsVerified,
        ApprovalStatus = profile.ApprovalStatus,
        AvailabilityStatus = profile.AvailabilityStatus,
        AverageRating = profile.AverageRating,
        ReviewCount = profile.ReviewCount,
        TopSkills = profile.ServiceProviderSkills.Take(3).Select(fs => fs.Skill.Name).ToList()
    };

    private static ServiceProviderDetailViewModel MapToDetail(ServiceProviderProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        FullName = profile.User.FullName,
        ProfileImagePath = profile.User.ProfileImagePath,
        Headline = profile.Headline,
        Bio = profile.Bio,
        CategoryName = profile.Category.Name,
        Address = profile.Address,
        PhoneNumber = profile.PhoneNumber,
        Email = profile.User.Email ?? string.Empty,
        YearsOfExperience = profile.YearsOfExperience,
        HourlyRate = profile.HourlyRate,
        IsVerified = profile.IsVerified,
        ApprovalStatus = profile.ApprovalStatus,
        AvailabilityStatus = profile.AvailabilityStatus,
        ProfileViews = profile.ProfileViews,
        ProfileCompletionPercentage = CalculateProfileCompletion(profile),
        AverageRating = profile.AverageRating,
        ReviewCount = profile.ReviewCount,
        Skills = profile.ServiceProviderSkills.Select(fs => fs.Skill.Name).ToList(),
        PortfolioItems = profile.PortfolioItems.Select(p => new PortfolioItemViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            ImagePath = p.ImagePath,
            CompletedOn = p.CompletedOn
        }).ToList(),
        Reviews = profile.Reviews.Select(r => new ReviewDisplayViewModel
        {
            Id = r.Id,
            ClientName = r.ClientProfile.User.FullName,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedOn = r.CreatedOn,
            ServiceProviderReply = r.ServiceProviderReply,
            RepliedOn = r.RepliedOn
        }).ToList()
    };
}
