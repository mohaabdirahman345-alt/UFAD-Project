# Deploying UFAD Garowe to Azure (free tier)

This deploys the app to Azure App Service (Linux, free F1 tier) with Azure
SQL Database's free offer, using the GitHub Actions workflow at
`.github/workflows/azure-deploy.yml`. Total cost: $0, as long as you stay
within the free-tier limits described below.

Everything here is done in the Azure Portal / GitHub UI by you — an AI
assistant can't create accounts, enter payment details, or generate
secrets on your behalf.

## 1. Create an Azure account

Go to https://azure.microsoft.com/free and sign up. Azure asks for a
card for identity verification even for free-tier resources — you won't
be charged unless you explicitly upgrade a resource past its free limits.

## 2. Create a resource group

Portal → **Resource groups** → **Create**. Any name (e.g. `ufad-garowe-rg`),
any region close to you (e.g. `East US` or `UAE North`).

## 3. Create the database — Azure SQL free offer

Portal → **Create a resource** → **SQL Database**.

- **Resource group**: the one from step 2
- **Database name**: `Unified`
- **Server**: create new
  - Server name: anything globally unique, e.g. `ufad-garowe-sql`
  - Authentication: **SQL authentication**
  - Admin login: choose one (e.g. `ufadadmin`) and a strong password —
    **save both**, you'll need them below
- **Workload environment**: Development
- **Compute + storage**: click **Configure database**, choose the
  **Free offer** (serverless, up to 100,000 vCore-seconds/month and 32 GB —
  one per Azure subscription). If you don't see it, look for a banner/link
  offering the free SQL Database tier on the pricing tier screen.

After it's created: open the new **SQL server** (not the database) →
**Networking** → under "Firewall rules" enable **Allow Azure services and
resources to access this server** → Save.

## 4. Create the web app

Portal → **Create a resource** → **Web App**.

- **Resource group**: same as above
- **Name**: globally unique, e.g. `ufad-garowe` → your URL will be
  `https://ufad-garowe.azurewebsites.net`
- **Publish**: Code
- **Runtime stack**: **.NET 9 (STS)**
- **Operating System**: Linux
- **Pricing plan**: click **Explore pricing plans** → pick **F1 (Free)**

## 5. Configure the connection string

On the new Web App → **Settings → Environment variables** (older portals:
**Configuration**) → **Connection strings** tab → **New connection string**:

- Name: `DefaultConnection`
- Value:
  ```
  Server=tcp:<your-server-name>.database.windows.net,1433;Initial Catalog=Unified;Persist Security Info=False;User ID=<your-admin-login>;Password=<your-admin-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
  ```
- Type: **SQLAzure**

Save. Azure exposes this to the app in the exact format ASP.NET Core's
configuration system already expects — no code change needed.

## 6. Set the admin/demo passwords (the security fix that made this safe)

Same **Environment variables / Configuration** page → **Application
settings** tab → add:

| Name | Value |
|---|---|
| `Seed__AdminPassword` | a strong password **you** choose — this becomes the live admin login for `admin@garoweartisans.so` |
| `Seed__DemoAccountPassword` | a different strong password for the seeded demo provider/client accounts |

Without `Seed__AdminPassword` set, the app deliberately will **not** create
an administrator account on a non-Development deployment (see
`Data/Seed/DbInitializer.cs`) — this is intentional, since the old fixed
demo password is published in the README and thesis appendix.

Save. The app will restart.

## 7. Get the publish profile

Web App → **Overview** → **Get publish profile** (downloads a `.PublishSettings` XML file).

## 8. Wire up GitHub Actions

In your GitHub repo ([UFAD-Project](https://github.com/mohaabdirahman345-alt/UFAD-Project)):

- **Settings → Secrets and variables → Actions → Secrets** → New repository secret:
  - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
  - Value: paste the entire contents of the `.PublishSettings` file from step 7
- **Settings → Secrets and variables → Actions → Variables** → New repository variable:
  - Name: `AZURE_WEBAPP_NAME`
  - Value: the Web App name from step 4 (e.g. `ufad-garowe`)

## 9. Deploy

Push to `main` (or run the workflow manually from the **Actions** tab —
it's already set to trigger on both). The workflow builds and publishes
the app, then deploys it to Azure. First deploy takes a few minutes;
watch progress under the **Actions** tab.

## 10. Verify

Visit `https://<your-app-name>.azurewebsites.net`. On first request, the
app applies EF Core migrations and seeds categories/skills/demo accounts
automatically (`DbInitializer`) — the homepage should show the same
directory you saw locally. Log in as `admin@garoweartisans.so` with the
password you set in step 6.

## Free-tier limits to know about

- **F1 App Service**: 60 CPU-minutes/day, no always-on (the app sleeps
  after ~20 min idle and takes a few seconds to wake on the next request —
  normal for a free tier, not a bug).
- **Azure SQL free offer**: one per subscription, 100,000 vCore-seconds
  and 32 GB/month; the database auto-pauses/scales down under light load
  (serverless), which is fine for a demo site.
