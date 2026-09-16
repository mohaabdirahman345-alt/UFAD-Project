using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Data.Seed;

/// <summary>
/// Applies pending migrations and, on a database with no categories yet,
/// seeds roles, a demo administrator, service categories and skills, and a
/// handful of realistic demo service provider/client accounts so the directory
/// is browsable immediately after first run. Every call is idempotent —
/// safe to run on every startup.
/// </summary>
public static class DbInitializer
{
    // Used automatically only in Development. Outside Development, the
    // administrator and demo-account passwords must come from configuration
    // (e.g. Seed:AdminPassword / Seed:DemoAccountPassword as Azure App
    // Service settings) — this value is published in the README and thesis
    // appendix, so it must never be the live administrator's password.
    private const string DemoPassword = "Garowe@2026";

    public static async Task RunAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var configuration = services.GetRequiredService<IConfiguration>();
        var env = services.GetRequiredService<IHostEnvironment>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        await SeedAdministratorAsync(userManager, configuration, env);

        if (await context.Categories.AnyAsync())
        {
            // Already seeded on a prior run.
            return;
        }

        var categories = await SeedCategoriesAndSkillsAsync(context);
        await SeedDemoAccountsAsync(context, userManager, categories, configuration, env);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in new[] { "Administrator", "ServiceProvider", "Client" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedAdministratorAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration, IHostEnvironment env)
    {
        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@garoweartisans.so";
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrEmpty(adminPassword))
        {
            if (!env.IsDevelopment())
            {
                // Refuse to auto-create an administrator with a published,
                // guessable password outside local development. Set
                // Seed:AdminPassword (e.g. as an App Service setting) to a
                // strong, private password to enable this on a live deployment.
                return;
            }

            adminPassword = DemoPassword;
        }

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Garowe",
            LastName = "Administrator"
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Administrator");
        }
    }

    private static async Task<Dictionary<string, Category>> SeedCategoriesAndSkillsAsync(ApplicationDbContext context)
    {
        var definitions = new (string Name, string Description, string IconClass, string[] Skills)[]
        {
            ("Carpentry", "Furniture making, cabinetry, and wood structural work.", "icon-carpentry",
                new[] { "Cabinet Making", "Furniture Repair", "Door & Window Framing", "Wood Finishing" }),
            ("Electrical Work", "Household and small commercial wiring and repair.", "icon-electrical",
                new[] { "House Wiring", "Solar Panel Installation", "Generator Setup", "Fault Diagnosis" }),
            ("Plumbing", "Water supply, drainage, and fixture installation.", "icon-plumbing",
                new[] { "Pipe Installation", "Leak Repair", "Water Tank Setup", "Bathroom Fitting" }),
            ("Tailoring", "Custom garment making and alterations.", "icon-tailoring",
                new[] { "Traditional Dirac Making", "Men's Suit Tailoring", "Alterations", "Embroidery" }),
            ("Masonry & Construction", "Block work, plastering, and general building.", "icon-masonry",
                new[] { "Block Laying", "Plastering", "Tiling", "Foundation Work" }),
            ("Auto Mechanics", "Vehicle repair and maintenance.", "icon-mechanic",
                new[] { "Engine Repair", "Brake Service", "Electrical Diagnostics", "Tire Service" }),
            ("Painting & Decoration", "Interior and exterior painting.", "icon-painting",
                new[] { "Interior Painting", "Exterior Painting", "Wall Texturing", "Signage Painting" }),
            ("Photography", "Event and portrait photography services.", "icon-photography",
                new[] { "Wedding Photography", "Portrait Photography", "Event Videography", "Photo Editing" }),
        };

        var categories = new Dictionary<string, Category>();

        foreach (var (name, description, iconClass, skills) in definitions)
        {
            var category = new Category
            {
                Name = name,
                Description = description,
                IconClass = iconClass,
                IsActive = true
            };

            category.Skills = skills.Select(skillName => new Skill { Name = skillName }).ToList();

            context.Categories.Add(category);
            categories[name] = category;
        }

        await context.SaveChangesAsync();
        return categories;
    }

    private static async Task SeedDemoAccountsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        Dictionary<string, Category> categories,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        // Same reasoning as the administrator password: fall back to the
        // published demo password only in Development. On a live deployment,
        // set Seed:DemoAccountPassword to something not published anywhere.
        var demoPassword = configuration["Seed:DemoAccountPassword"];
        if (string.IsNullOrEmpty(demoPassword))
        {
            demoPassword = env.IsDevelopment() ? DemoPassword : Guid.NewGuid().ToString("N") + "Aa1!";
        }

        var serviceProviderSeeds = new (string First, string Last, string Category, string Headline, int Years, decimal Rate, string Address, string Phone)[]
        {
            ("Cabdirisaaq", "Warsame", "Carpentry", "Custom cabinetry and furniture repair, 12 years in Garowe", 12, 15, "Boocame", "090111222"),
            ("Xaliimo", "Nuur", "Tailoring", "Traditional dirac and modern alterations", 8, 10, "Garowe Town Center", "090222333"),
            ("Maxamed", "Cali", "Electrical Work", "Licensed electrician for homes and small shops", 10, 20, "Wadajir", "090333444"),
            ("Faadumo", "Ismaaciil", "Photography", "Wedding and family event photography", 6, 25, "Iftin", "090444555"),
            ("Cabdulaahi", "Xasan", "Auto Mechanics", "Toyota and Nissan engine specialist", 15, 18, "Israac", "090555666"),
            ("Sahra", "Maxamuud", "Masonry & Construction", "Block laying and plastering crew lead", 9, 14, "Garowe Town Center", "090666777"),
        };

        var clientSeeds = new (string First, string Last, string Neighborhood, string Phone)[]
        {
            ("Ikraan", "Cabdi", "Wadajir", "090777888"),
            ("Yusuf", "Farax", "Boocame", "090888999"),
            ("Hodan", "Cige", "Iftin", "090999000"),
        };

        var serviceProviderProfiles = new List<ServiceProviderProfile>();

        foreach (var seed in serviceProviderSeeds)
        {
            var email = $"{seed.First.ToLowerInvariant()}.{seed.Last.ToLowerInvariant()}@garoweartisans.so";
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = seed.First,
                LastName = seed.Last
            };

            var result = await userManager.CreateAsync(user, demoPassword);
            if (!result.Succeeded)
            {
                continue;
            }

            await userManager.AddToRoleAsync(user, "ServiceProvider");

            var category = categories[seed.Category];
            var profile = new ServiceProviderProfile
            {
                UserId = user.Id,
                CategoryId = category.Id,
                Headline = seed.Headline,
                Bio = $"{seed.First} has been serving clients across Garowe for {seed.Years} years, known for reliable, on-time work.",
                YearsOfExperience = seed.Years,
                HourlyRate = seed.Rate,
                Address = seed.Address,
                PhoneNumber = seed.Phone,
                IsVerified = true,
                ApprovalStatus = ApprovalStatus.Approved,
                AvailabilityStatus = AvailabilityStatus.Available,
                SubmittedForApprovalOn = DateTime.UtcNow.AddDays(-30),
                ApprovalDecisionOn = DateTime.UtcNow.AddDays(-29)
            };

            context.ServiceProviderProfiles.Add(profile);
            serviceProviderProfiles.Add(profile);
        }

        await context.SaveChangesAsync();

        // Attach a couple of skills per service provider from their own category.
        foreach (var profile in serviceProviderProfiles)
        {
            var categorySkills = await context.Skills
                .Where(s => s.CategoryId == profile.CategoryId)
                .Take(2)
                .ToListAsync();

            foreach (var skill in categorySkills)
            {
                context.ServiceProviderSkills.Add(new ServiceProviderSkill
                {
                    ServiceProviderProfileId = profile.Id,
                    SkillId = skill.Id,
                    ProficiencyLevel = 5
                });
            }

            context.PortfolioItems.Add(new PortfolioItem
            {
                ServiceProviderProfileId = profile.Id,
                Title = $"{profile.Headline.Split(',')[0]} — recent work",
                Description = "A recent job completed for a client in Garowe.",
                ImagePath = "/images/portfolio/placeholder.jpg",
                CompletedOn = DateTime.UtcNow.AddMonths(-2),
                DisplayOrder = 0
            });
        }

        await context.SaveChangesAsync();

        var clientProfiles = new List<ClientProfile>();

        foreach (var seed in clientSeeds)
        {
            var email = $"{seed.First.ToLowerInvariant()}.{seed.Last.ToLowerInvariant()}@example.so";
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = seed.First,
                LastName = seed.Last
            };

            var result = await userManager.CreateAsync(user, demoPassword);
            if (!result.Succeeded)
            {
                continue;
            }

            await userManager.AddToRoleAsync(user, "Client");

            var profile = new ClientProfile
            {
                UserId = user.Id,
                Neighborhood = seed.Neighborhood,
                PhoneNumber = seed.Phone
            };

            context.ClientProfiles.Add(profile);
            clientProfiles.Add(profile);
        }

        await context.SaveChangesAsync();

        // A few realistic reviews so ratings and sort order aren't empty on first run.
        var reviewComments = new[]
        {
            "Did excellent work and finished on the day he promised.",
            "Good quality but arrived a little later than planned.",
            "Very professional, would hire again for future work.",
            "Fair pricing and explained the work clearly beforehand."
        };

        var random = new Random(42);
        foreach (var profile in serviceProviderProfiles)
        {
            var reviewerCount = random.Next(1, clientProfiles.Count + 1);
            for (var i = 0; i < reviewerCount; i++)
            {
                var client = clientProfiles[i];
                context.Reviews.Add(new Review
                {
                    ServiceProviderProfileId = profile.Id,
                    ClientProfileId = client.Id,
                    Rating = random.Next(3, 6),
                    Comment = reviewComments[random.Next(reviewComments.Length)],
                    CreatedOn = DateTime.UtcNow.AddDays(-random.Next(5, 90))
                });
            }
        }

        await context.SaveChangesAsync();

        // Recalculate denormalized rating fields from the seeded reviews.
        foreach (var profile in serviceProviderProfiles)
        {
            var reviews = await context.Reviews.Where(r => r.ServiceProviderProfileId == profile.Id).ToListAsync();
            profile.ReviewCount = reviews.Count;
            profile.AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 2);
        }

        await context.SaveChangesAsync();

        await SeedApprovalHistoryAsync(context, serviceProviderProfiles);
        await SeedServiceRequestsAsync(context, serviceProviderProfiles, clientProfiles);
    }

    /// <summary>Records the initial approval decision for each demo provider so the admin history view isn't empty.</summary>
    private static async Task SeedApprovalHistoryAsync(ApplicationDbContext context, List<ServiceProviderProfile> serviceProviderProfiles)
    {
        foreach (var profile in serviceProviderProfiles)
        {
            context.ServiceProviderApprovalHistories.Add(new ServiceProviderApprovalHistory
            {
                ServiceProviderProfileId = profile.Id,
                FromStatus = ApprovalStatus.PendingApproval,
                ToStatus = ApprovalStatus.Approved,
                ChangedByUserId = null,
                ChangedOn = profile.ApprovalDecisionOn ?? DateTime.UtcNow,
                Notes = "Initial demo approval."
            });
        }

        await context.SaveChangesAsync();
    }

    /// <summary>A handful of demo service requests across different statuses so the request pages aren't empty on first run.</summary>
    private static async Task SeedServiceRequestsAsync(
        ApplicationDbContext context,
        List<ServiceProviderProfile> serviceProviderProfiles,
        List<ClientProfile> clientProfiles)
    {
        if (serviceProviderProfiles.Count < 3 || clientProfiles.Count < 2)
        {
            return;
        }

        var requestSeeds = new (int ProviderIndex, int ClientIndex, string Title, string Description, ServiceRequestStatus Status)[]
        {
            (0, 0, "Repair a wobbly dining table", "The dining table legs have loosened and need to be re-glued and reinforced.", ServiceRequestStatus.Completed),
            (1, 1, "Tailor a dirac for a wedding", "Need a dirac tailored for a wedding next month, will provide measurements.", ServiceRequestStatus.Accepted),
            (2, 0, "Rewire kitchen outlets", "Two kitchen outlets have stopped working and need to be inspected and rewired.", ServiceRequestStatus.Pending),
            (3, 2, "Family portrait session", "Looking for a one-hour outdoor family portrait session this weekend.", ServiceRequestStatus.Declined),
        };

        foreach (var seed in requestSeeds)
        {
            var provider = serviceProviderProfiles[seed.ProviderIndex];
            var client = clientProfiles[seed.ClientIndex];
            var createdOn = DateTime.UtcNow.AddDays(-20);

            var request = new ServiceRequest
            {
                ServiceProviderProfileId = provider.Id,
                ClientProfileId = client.Id,
                Title = seed.Title,
                Description = seed.Description,
                Status = seed.Status,
                CreatedOn = createdOn
            };

            var history = new List<ServiceRequestStatusHistory>
            {
                new()
                {
                    FromStatus = null,
                    ToStatus = ServiceRequestStatus.Pending,
                    ChangedByUserId = client.UserId,
                    ChangedOn = createdOn,
                    Notes = "Request submitted."
                }
            };

            switch (seed.Status)
            {
                case ServiceRequestStatus.Accepted:
                    request.RespondedOn = createdOn.AddDays(1);
                    history.Add(new ServiceRequestStatusHistory
                    {
                        FromStatus = ServiceRequestStatus.Pending,
                        ToStatus = ServiceRequestStatus.Accepted,
                        ChangedByUserId = provider.UserId,
                        ChangedOn = request.RespondedOn.Value
                    });
                    break;

                case ServiceRequestStatus.Declined:
                    request.RespondedOn = createdOn.AddDays(1);
                    request.DeclineReason = "Fully booked that weekend.";
                    history.Add(new ServiceRequestStatusHistory
                    {
                        FromStatus = ServiceRequestStatus.Pending,
                        ToStatus = ServiceRequestStatus.Declined,
                        ChangedByUserId = provider.UserId,
                        ChangedOn = request.RespondedOn.Value,
                        Notes = request.DeclineReason
                    });
                    break;

                case ServiceRequestStatus.Completed:
                    request.RespondedOn = createdOn.AddDays(1);
                    request.CompletedOn = createdOn.AddDays(5);
                    history.Add(new ServiceRequestStatusHistory
                    {
                        FromStatus = ServiceRequestStatus.Pending,
                        ToStatus = ServiceRequestStatus.Accepted,
                        ChangedByUserId = provider.UserId,
                        ChangedOn = request.RespondedOn.Value
                    });
                    history.Add(new ServiceRequestStatusHistory
                    {
                        FromStatus = ServiceRequestStatus.Accepted,
                        ToStatus = ServiceRequestStatus.Completed,
                        ChangedByUserId = provider.UserId,
                        ChangedOn = request.CompletedOn.Value
                    });
                    break;
            }

            request.StatusHistory = history;
            context.ServiceRequests.Add(request);
        }

        await context.SaveChangesAsync();
    }
}
