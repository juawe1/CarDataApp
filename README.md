# BMWConnectedApp

BMWConnectedApp is a WinUI 3 desktop dashboard (targeting .NET 8) that demonstrates integration with the BMW Cardata APIs using the OAuth2 Device Code flow with PKCE.

Purpose
-------
The app collects vehicle data from the BMW Cardata API, stores it locally in a SQLite database, and presents a dashboard of the vehicle state. The long-term goal is to accumulate historical data so you can build richer metrics (trip heatmaps, fuel consumption trends, estimated running costs, etc.).

Key features
------------
- Device Code + PKCE authentication flow for initial authorization (user opens a browser and enters a short code).
- Refresh token handling and automatic token refresh (refresh tokens stored in the platform password vault).
- Local persistence using EF Core + SQLite to keep historical data (vehicle mappings, timestamps, etc.).
- MVVM architecture (CommunityToolkit.Mvvm) and DI via Microsoft.Extensions.Hosting.
- Simple dashboard UI (WinUI 3) that shows connection status and basic vehicle widgets.

Security & secrets
------------------
- Do NOT commit secrets (client secrets, refresh tokens, access tokens, personal VINs) to source control.
- appsettings.json intentionally contains placeholders (ClientId and VIN). For local development use one of these approaches:
  - dotnet user-secrets: dotnet user-secrets init then dotnet user-secrets set "BmwApi:ClientId" "your-client-id" etc.
  - Environment variables (BmwApi__ClientId, BmwApi__VIN).
  - CI secret stores for automated builds.
- Refresh tokens are stored in the Windows Credential Manager via the PasswordVaultService; they are not stored in source control.

Local setup
-----------
1. Install Visual Studio 2026 (or VS 2022/2025 with WinUI 3 support) and .NET 8 SDK.
2. Open the solution: BMWConnectedApp.slnx
3. Configure secrets (recommended):
   - From the project directory run: dotnet user-secrets init
   - Set values: dotnet user-secrets set "BmwApi:ClientId" "YOUR_CLIENT_ID"
   - Optionally set VIN: dotnet user-secrets set "BmwApi:VIN" "YOUR_VIN"
4. Build and run from Visual Studio.

How authentication works
------------------------
1. The app starts the Device Code flow (GetDeviceCodeAsync) which returns a verification URL and short user code.
2. The user opens the URL and enters the code to authenticate the app.
3. The app polls the token endpoint (PollForTokenAsync) and receives access and refresh tokens when the user completes the flow.
4. The refresh token is stored securely using the platform password vault and used later to obtain new access tokens.

Database and local storage
--------------------------
- The app uses EF Core (SQLite) to persist configuration and collected data. The DB file is located in the local application data folder by default.
- The DataSyncService caches access tokens in memory and saves refresh tokens in the platform vault.

Development notes
-----------------
- Project uses CommunityToolkit.Mvvm for ViewModels and Microsoft.Extensions.Hosting for DI. The IBmwApiService and DataSyncService encapsulate API calls and polling logic.
- UI views are WinUI 3 Pages and use x:Bind / MVVM patterns.

Publishing & repository hygiene
------------------------------
- This repository includes a .gitignore that excludes build artifacts, user secrets files, and database files. Keep secrets out of source control.
- If you ever add a client secret or personal data in your local copy, rotate it and do not push it.

Planned improvements / roadmap
----------------------------
- Collect and persist more vehicle telemetry to enable historical metrics (trip length, distance by day/week, fuel consumption trends).
- Add visualizations for usage patterns (maps, charts) and estimated running costs over time.
- Improve background sync and automatic scheduling to collect data periodically.

Help / contribution
-------------------
If you want help with any of the above tasks (adding metrics, charts, or CI configuration), open an issue or request specific changes and I can implement them.

License
-------
Add a license file appropriate for your project before publishing (MIT is commonly used for personal projects).

