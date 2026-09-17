# Deploying UFAD Garowe to MonsterASP.NET (free tier)

This deploys the app to MonsterASP.NET's Free Hosting plan (Windows/IIS,
free subdomain) with a free MSSQL database, using the GitHub Actions
workflow at `.github/workflows/monsterasp-deploy.yml`. Total cost: $0 —
MonsterASP's free plan requires no credit card at signup.

Verified against MonsterASP.NET's own documentation before starting this:
.NET Core 9 and Razor Pages are explicitly supported, the default IIS
hosting model (InProcess) needs no project changes, and their
`Environment Variables` feature uses the same `Key__NestedKey` convention
ASP.NET Core already reads out of the box — so nothing here required
rewriting the app or its database provider.

Everything below is done by you in the MonsterASP Control Panel / GitHub
UI — account creation, and anything requiring your credentials, isn't
something an AI assistant should do on your behalf.

## 1. Create your free account

Go to https://monsterasp.net → **Try for FREE** → enter your email only
(no credit card, no other personal info required). Your login details
are emailed to you.

## 2. Create the website

Control Panel → **Websites** → **Add website** → choose **Free** →
pick a name → you'll get a subdomain like
`https://yoursite.runasp.net` (or `.tryasp.net`).

## 3. Create the database

Control Panel → **Databases** → **Add database** → choose **Free** →
type **MSSQL**. Note the server name, database name, login, and password
shown after creation (or find them again under the database's **Users
and remote** section).

## 4. Set the connection string and passwords as Environment Variables

Control Panel → **Websites** → select your site → **Manage website** →
**Scripting** → **Environment Variables**. Add:

| Name | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | `Server=<db-server-from-step-3>;Database=<db-name>;User Id=<db-login>;Password=<db-password>;TrustServerCertificate=True;` |
| `Seed__AdminPassword` | a strong password **you** choose — this becomes the live admin login for `admin@garoweartisans.so` |
| `Seed__DemoAccountPassword` | a different strong password for the seeded demo provider/client accounts |

Same reasoning as before: without `Seed__AdminPassword` set, the app
will not create an administrator account outside local development,
because the old fixed demo password is published in the README and
thesis appendix (see `Data/Seed/DbInitializer.cs`).

## 5. Activate WebDeploy

Control Panel → your website → **Deploy (FTP/WebDeploy/Git)** →
**WebDeploy** → **Enabled**. Note the four values shown — you'll need
them in the next step:

- Website name (e.g. `siteXXXX`)
- Server computer name (e.g. `https://siteXXXX.siteasp.net:8172`)
- Username
- Password

## 6. Add GitHub secrets

In your GitHub repo ([UFAD-Project](https://github.com/mohaabdirahman345-alt/UFAD-Project))
→ **Settings → Secrets and variables → Actions → Secrets** → add these
four, using the values from step 5:

- `WEBSITE_NAME`
- `SERVER_COMPUTER_NAME`
- `SERVER_USERNAME`
- `SERVER_PASSWORD`

## 7. Deploy

Push to `main` (or run the workflow manually from the **Actions** tab).
The workflow builds and publishes the app (`dotnet publish -r win-x86
--self-contained false`, matching MonsterASP's documented IIS setup),
then deploys it via WebDeploy using
[rasmusbuchholdt/simply-web-deploy](https://github.com/rasmusbuchholdt/simply-web-deploy).

## 8. Verify

Visit your subdomain. On first request, `DbInitializer` applies EF Core
migrations and seeds categories/skills/demo accounts automatically,
exactly as it does locally — no separate migration step needed. Log in
as `admin@garoweartisans.so` with the password you set in step 4.

## Things to check once it's live (couldn't verify these without an account)

- **HTTPS on the free subdomain**: MonsterASP's pricing table lists
  "HTTPS (Let's Encrypt)" as a Premium-only feature, which is about
  provisioning certificates for *custom* domains. Their own marketing
  material shows free-style subdomains served over HTTPS by default
  (a shared platform certificate), but I couldn't find a doc page
  confirming this outright — check `https://` loads without a warning
  once your site is up. If it doesn't, `app.UseHttpsRedirection()` in
  `Program.cs` would need to be relaxed for the free tier.
- **Idle behavior**: free-tier shared IIS app pools commonly recycle
  after a period of inactivity (the free plan lists "Limited features
  and traffic" without further detail) — the first request after a
  quiet period may be a few seconds slower while the app pool restarts.
  This doesn't affect availability, just first-hit latency.
- **256 MB dedicated RAM** on the free plan is modest for an EF Core +
  Identity app; fine for a demo/thesis-defense audience, worth knowing
  if you later see performance issues under heavier concurrent load.
