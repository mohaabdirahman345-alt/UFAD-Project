# Unified Freelance & Artisans Directory System

A localized directory and reputation platform connecting **Service
Providers** and clients in Garowe, Somalia. Built with ASP.NET Core 9 Razor
Pages, EF Core 9, and SQL Server, following Clean Architecture, Repository,
and Service Layer patterns.

## Domain model

A **Service Provider** is the single unified account type for every
professional the platform supports — electricians, carpenters, tailors,
mechanics, photographers, web developers, designers, translators, and more.
There is no separate "freelancer" vs. "artisan" account type; a provider's
profession is expressed entirely through their **Service Category**,
**Skills**, **Experience**, and **Portfolio** rather than by account type.

### Roles

Exactly three roles exist: **Administrator**, **Service Provider**, and
**Client**.

### Service Provider workflow

```
Register Account
  -> Select Service Category & Skills
  -> Complete Profile (bio, phone, address, experience, availability)
  -> Upload Portfolio
  -> Submit Profile for Approval
  -> Administrator Review
  -> Approved -> Public Listing -> appears in client search
     Rejected -> provider edits and resubmits
  -> Receive Reviews from clients they've worked with
```

A profile starts as **Draft** on registration, moves to **PendingApproval**
when the provider submits it, and only becomes visible in the public
directory once an administrator sets it to **Approved**. This approval
status is separate from the **Verified** badge — verification is an
additional trust signal an administrator can grant on top of approval; an
approved-but-unverified profile still appears in search.

## Prerequisites

- .NET SDK 9.0
- SQL Server (LocalDB is fine for development — it ships with Visual Studio /
  the "SQL Server Express LocalDB" component)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## First-time setup

```bash
cd UnifiedFreelanceArtisansDirectory
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

No migrations are checked in — the first `migrations add` command generates
them from the entity model in `Domain/Entities` and `Data/ApplicationDbContext.cs`.
`dotnet run` also applies pending migrations automatically on every start
(via `DbInitializer.RunAsync`, which calls `Database.MigrateAsync()`), and
seeds roles, a demo administrator, categories/skills, and a handful of demo
Service Provider/Client accounts the first time it finds an empty database.
Seeded Service Providers are created already **Approved**, so the directory
looks populated immediately.

The app listens on the port shown in the console output (typically
`https://localhost:5001` or similar — check the `Now listening on:` line).

## Demo accounts

All seeded accounts use the password **`Garowe@2026`**.

| Role | Email |
|---|---|
| Administrator | `admin@garoweartisans.so` |
| Service Provider | `cabdirisaaq.warsame@garoweartisans.so` (and 5 others — see `Data/Seed/DbInitializer.cs`) |
| Client | `ikraan.cabdi@example.so` (and 2 others) |

## Project structure

```
Domain/Entities/        Core entities (ApplicationUser, ServiceProviderProfile, Category, Review, ...)
Domain/Enums/           ApprovalStatus, AvailabilityStatus, ReportStatus, ReportReason
Data/                    ApplicationDbContext (Fluent API config) + DbInitializer (seed)
Repositories/            IGenericRepository/GenericRepository, IServiceProviderRepository (search),
                         IUnitOfWork/UnitOfWork
Services/                Business logic: CategoryService, SkillService, ServiceProviderService
                         (search, profile CRUD, approval workflow, profile views/completion),
                         ClientProfileService, PortfolioService, ReviewService, FavoriteService,
                         ReportService
Services/ViewModels/     DTOs used by pages (never expose entities directly to views)
Pages/                   Public site: Home, Browse (find service providers), ServiceProviderDetails,
                         Error
Areas/Identity/Pages/    Custom login, registration (role choice), logout, access denied —
                         hand-built, not the default scaffolded Identity UI
Areas/ServiceProvider/   Service Provider dashboard, profile editor (+ submit for approval),
                         portfolio manager, review replies
Areas/Client/Pages/      Client dashboard, saved service providers, my reviews
Areas/Admin/Pages/       Admin dashboard, category/skill management, service provider
                         approval + verification queue, user management, report moderation
wwwroot/css/site.css     The custom design system (see below)
```

## Design system

The visual identity is original to this project — no default Bootstrap look,
no blue/purple gradients, no glassmorphism. Palette: warm sand, deep navy,
olive green, and copper, built around `Fraunces` (headings) and `Work Sans`
(body) from Google Fonts. Every component (buttons, cards, badges, tables,
pagination, forms, alerts) is hand-styled in `wwwroot/css/site.css` on top of
Bootstrap 5's grid and JS behaviors (modals, dropdowns) — Bootstrap supplies
layout plumbing only, not visual styling. Internal CSS class names (e.g.
`artisan-card`, `btn-artisan-primary`) are a styling-convention leftover from
the project's working name and are not user-facing — they were left as-is
during the Service Provider terminology refactor since renaming them touches
dozens of files for zero visible or functional benefit.

## Key architectural decisions

- **Denormalized ratings**: `ServiceProviderProfile.AverageRating` /
  `ReviewCount` are recalculated by `ReviewService` on every review
  add/edit/delete, so the directory's search and sort never run a live
  aggregate query.
- **Approval gates search, verification doesn't**: `ServiceProviderRepository.SearchAsync`
  and `GetTopRatedAsync` filter on `ApprovalStatus.Approved` — this is the
  only gate on public visibility. `IsVerified` is a separate badge with no
  effect on whether a profile is searchable.
- **Cascade rules are sized per relationship**: deleting a service provider
  profile cascades their skills, portfolio, and reviews; deleting a `Skill`
  or `Category` is `Restrict`ed so it can't silently erase history elsewhere.
  `Report` only cascades on the reporter — the reported user and resolver
  references are `Restrict`ed (SQL Server also disallows multiple cascade
  paths into the same table).
- **Area-based authorization** is enforced once, at the folder level, in
  `Program.cs` (`AuthorizeAreaFolder`), so every page under `/Admin`,
  `/ServiceProvider`, `/Client` requires the matching role with no per-page
  attribute needed.
- **Skill sync as a diff**: updating a service provider's claimed skills
  removes only what was deselected and adds only what's new, rather than
  clearing and reinserting every row.
- **Profile completion is computed, not stored**: `ServiceProviderService`
  calculates the profile-completion percentage fresh from current data
  (photo, bio, phone, address, experience, rate, skills, portfolio) rather
  than persisting a value that could drift out of sync.
- **One Service Category per provider**: the current model keeps a single
  primary `CategoryId` on `ServiceProviderProfile` (as it did before this
  refactor), rather than a many-to-many. The brief's profile-fields section
  says "Service Categories" (plural); if you want a provider to be
  discoverable under more than one category, that's a follow-up change
  (a `ServiceProviderCategory` join table, mirroring how skills already
  work) rather than something folded into this pass.

## Known limitations / next steps

- **No booking/request system yet.** The workflow diagram in the brief ends
  with "Receive Job Requests -> Accept or Decline -> Complete Service" — that
  is a genuinely new subsystem (a request/booking entity, its own state
  machine, notifications) rather than a rename, and wasn't built in this
  refactor pass. Happy to scope and build it as a following piece of work.
- Image uploads for portfolio items are stored on local disk under
  `wwwroot/images/portfolio` — fine for a single-server deployment, but move
  to blob storage before scaling to multiple instances.
- No email sending is wired up (account confirmation, password reset) —
  `EmailConfirmed = true` is set directly on registration for simplicity.
  Add an `IEmailSender` implementation before relying on those flows.
- This codebase was written and reviewed in an environment without a .NET
  SDK or NuGet access, so it has not been compiled here. Run
  `dotnet build` locally as your first step and fix any environment-specific
  issues (e.g. exact NuGet package versions available to you) before relying
  on it.
