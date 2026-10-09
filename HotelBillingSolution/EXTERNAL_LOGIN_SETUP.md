# External Login Setup

This document explains how to configure Google, Microsoft and GitHub OAuth apps and how to provide the ClientId/ClientSecret for local development.

Important: do NOT store real secrets in source control. Use dotnet user-secrets or environment variables.

1) Redirect / Callback URLs (local development)

- Google:  https://localhost:5051/signin-google
- Microsoft: https://localhost:5051/signin-microsoft
- GitHub: https://localhost:5051/signin-github

Note: Ensure your application uses HTTPS during development. If your app uses a different port, update the URLs accordingly.

2) Create credentials

Google (Google Cloud Console):
- Go to https://console.cloud.google.com/apis/credentials
- Create OAuth 2.0 Client ID (Application type: Web application)
- Add the redirect URI above (https://localhost:5051/signin-google)
- Copy Client ID and Client Secret

Microsoft (Azure App registrations / Microsoft Accounts):
- For consumer Microsoft accounts, you can register via Azure Portal App registrations
- Go to https://portal.azure.com -> Azure Active Directory -> App registrations -> New registration
- Set Redirect URI (Web) to https://localhost:5051/signin-microsoft
- Under Certificates & secrets, create a client secret and copy it

GitHub (Developer settings -> OAuth Apps):
- Go to https://github.com/settings/developers -> OAuth Apps -> New OAuth App
- Set Authorization callback URL to https://localhost:5051/signin-github
- Copy Client ID and Client Secret

3) Provide credentials to the application (local development)

Use dotnet user-secrets to store secrets locally (recommended):

# From the web project folder run:
# Initialize if not already done
# dotnet user-secrets init

# Set values (example):
# dotnet user-secrets set "Authentication:Google:ClientId" "<your-google-client-id>"
# dotnet user-secrets set "Authentication:Google:ClientSecret" "<your-google-client-secret>"
# dotnet user-secrets set "Authentication:Microsoft:ClientId" "<your-ms-client-id>"
# dotnet user-secrets set "Authentication:Microsoft:ClientSecret" "<your-ms-client-secret>"
# dotnet user-secrets set "Authentication:GitHub:ClientId" "<your-gh-client-id>"
# dotnet user-secrets set "Authentication:GitHub:ClientSecret" "<your-gh-client-secret>"

Alternatively you can set environment variables or provide these keys in your environment-specific appsettings.Development.json (not checked into source control).

4) Notes / Troubleshooting

- Ensure the redirect URI in the provider configuration exactly matches the URL used by the application (including scheme, host and port).
- Use HTTPS for OAuth flows; some providers disallow HTTP redirect URIs.
- If you see "redirect_uri_mismatch" on provider side, double-check the callback URL.
- If external provider does not return email claim, the application will show an error and require manual handling.
- The application uses the existing AppUser table. External login will create a local AppUser (email/username) when none exists.

5) Testing

- Run the application (ensure HTTPS) and navigate to the Login page.
- Click "Continue with Google" (or Microsoft / GitHub). The browser should be redirected to the provider's consent screen.
- After consenting, you should be redirected back to the application and signed in.

6) Security

- Never commit ClientSecrets to the repository.
- Use secrets management and rotate client secrets regularly.
- Do not log access tokens or client secrets.

If you need assistance generating provider credentials, provide the provider name and I will give step-by-step guidance.