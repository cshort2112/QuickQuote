QuickQuote (MVP)

An end-to-end sample that demonstrates a minimal quote-generation system:
- A .NET 8 ASP.NET Core Minimal API that calculates quotes and returns a JSON result with a PDF URL.
- A .NET MAUI client app (Android, iOS, Windows, macOS Catalyst) that captures input, calls the API via HttpClient, and opens the generated PDF on tap.
- A shared DTO library (QuickQuote.Shared) to keep contracts consistent across API and client.
- SQLite persistence for configurable service rates.

This README defines the MVP requirements, the target architecture, and practical steps to run locally and deploy to Azure. It also includes an optional CI/CD example with GitHub Actions.

Repository Structure

- QuickQuote.Api — ASP.NET Core Minimal API (net8.0)
- QuickQuote.Maui — .NET MAUI app (net8.0-android, net8.0-ios, net8.0-maccatalyst, Windows via WinUI)
- QuickQuote.Shared — Shared DTOs (net8.0)
- QuickQuote.sln — Solution file

MVP Requirements and Acceptance Criteria (Checklist)

1. Web API (ASP.NET Core Minimal API)
   - Acceptance: POST /api/quote returns JSON with calculation results and a PDF URL.
2. Quote Calculation Logic
   - Acceptance: total = base + (hours × hourlyRate) + (travel × distance)
3. PDF Generation
   - Acceptance: 1-page PDF with client name, line items, total. File is accessible via URL returned by API.
4. SQLite Database
   - Acceptance: Stores ServiceConfig (base, hourly, travel rates) used for calculations.
5. Shared DTOs
   - Acceptance: QuoteRequest, QuoteResult, ServiceConfig types live in QuickQuote.Shared and are consumed by both API and MAUI app.
6. .NET MAUI App
   - Acceptance: Runs on Android, iOS, and Windows (and/or Mac Catalyst locally as applicable).
7. MAUI UI
   - Acceptance: Screen with input fields and a “Get Quote” button.
8. HttpClient Call
   - Acceptance: MAUI calls API, receives JSON including pdfUrl.
9. Open PDF
   - Acceptance: User taps a link/button to open the PDF in the device browser or PDF viewer.
10. CORS Enabled
   - Acceptance: MAUI can call local or Azure-hosted API without CORS errors.
11. Azure Deployment (Free Tier)
   - Acceptance: API is live at https://yourname.azurewebsites.net (replace with your site).
12. GitHub Repo
   - Acceptance: Clean structure, README (this file), .gitignore configured for .NET/MAUI.
13. CI/CD (Optional)
   - Acceptance: GitHub Actions workflow that builds and deploys API to Azure App Service.

Current Status

- The solution compiles with template projects in place. This README defines the MVP target. Implementations for the endpoint, DTOs, PDF generation, and SQLite wiring should follow the guidance below.

Shared DTOs (QuickQuote.Shared)

Create the following types in QuickQuote.Shared so both API and MAUI reference them:

- ServiceConfig
  - baseRate: decimal
  - hourlyRate: decimal
  - travelRate: decimal

- QuoteRequest
  - clientName: string (required)
  - hours: decimal
  - distance: decimal

- QuoteResult
  - clientName: string
  - baseRate: decimal
  - hourlyRate: decimal
  - travelRate: decimal
  - hours: decimal
  - distance: decimal
  - lineItems: array of { description: string, amount: decimal }
  - total: decimal
  - pdfUrl: string

Note: Keep property names PascalCase for C# and ensure System.Text.Json options match expectations if different casing is desired.

Quote Calculation Logic

- Formula: total = base + (hours × hourlyRate) + (travel × distance)
- Example: Given base=50, hourlyRate=100, travelRate=2, hours=3.5, distance=25 →
  - base: 50
  - labor: 3.5 × 100 = 350
  - travel: 2 × 25 = 50
  - total: 450

Line items should reflect these components for clarity in the PDF and JSON response.

API (QuickQuote.Api)

Target endpoint (Minimal API):

- POST /api/quote
  - Request body: QuoteRequest
  - Response 200: QuoteResult (JSON), includes pdfUrl pointing to the generated PDF
  - Errors: 400 for invalid input, 500 for unexpected exceptions

Recommended components:
- SQLite via Microsoft.Data.Sqlite or Entity Framework Core with the SQLite provider.
- PDF generation library such as QuestPDF (free, MIT) or iText7 (commercial-friendly with appropriate license) or PdfSharpCore.
- Local PDF storage under wwwroot/pdfs with static file serving enabled; in Azure App Service, this maps to persistent storage if configured or can be regenerated on demand.

Minimal API sketch (conceptual):

- Add services:
  - builder.Services.AddEndpointsApiExplorer();
  - builder.Services.AddSwaggerGen();
  - builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
      .AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));
  - builder.Services.AddDbContext<ServiceConfigDb>(...);

- Configure:
  - app.UseSwagger(); app.UseSwaggerUI();
  - app.UseStaticFiles();
  - app.UseCors();

- Map endpoint:
  - app.MapPost("/api/quote", (QuoteRequest req, ServiceConfigDb db, IPdfGenerator pdfGen) => { /* calculate, generate PDF, return JSON */ });

Response example (abbreviated):

{
  "clientName": "Acme Co",
  "baseRate": 50,
  "hourlyRate": 100,
  "travelRate": 2,
  "hours": 3.5,
  "distance": 25,
  "lineItems": [
    { "description": "Base", "amount": 50 },
    { "description": "Labor (3.5 h × 100)", "amount": 350 },
    { "description": "Travel (25 mi × 2)", "amount": 50 }
  ],
  "total": 450,
  "pdfUrl": "https://localhost:5001/pdfs/quote-2025-11-13-2010.pdf"
}

PDF Generation (1-page)

- Required content (at minimum):
  - Client name
  - Date/time
  - Line items (Base, Labor, Travel)
  - Total amount prominently
- Storage: write PDF files into wwwroot/pdfs and return a URL to that resource.
- Libraries: QuestPDF is recommended for a simple 1-page document.
- File naming: quote-{yyyyMMdd-HHmmss}-{guid}.pdf to avoid collisions.

SQLite Database

- Table: ServiceConfig
  - Id (int, PK)
  - BaseRate (decimal)
  - HourlyRate (decimal)
  - TravelRate (decimal)

- Seed a single row on first run (e.g., Base=50, Hourly=100, Travel=2). Allow future admin endpoints to update.
- Connection: Data Source=quickquote.db in API content root. For Azure, use local file or App Service persistent storage. EF Core migrations recommended.

CORS

Enable CORS to allow the MAUI app (running on device/emulator) to call the API:

In Program.cs:

- builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(_ => true)));
- app.UseCors();

For production, scope origins to your known hosts.

.NET MAUI App (QuickQuote.Maui)

UI (single page for MVP):
- Inputs: Client Name (Entry), Hours (Entry), Distance (Entry)
- Button: Get Quote
- Output: Total + a tappable “Open PDF” link once returned

HttpClient flow:
- Read API base URL from configuration (e.g., Preferences or an embedded setting). For local dev on Android emulator, use the machine IP or special host (10.0.2.2 for Android emulators) and the API port.
- POST QuoteRequest to {ApiBase}/api/quote
- On success, display total and store pdfUrl; on tap, open via Launcher.OpenAsync(new Uri(pdfUrl)).

Sample pseudo-code:

var http = new HttpClient { BaseAddress = new Uri(apiBase) };
var req = new QuoteRequest { ClientName = name, Hours = hours, Distance = distance };
var res = await http.PostAsJsonAsync("/api/quote", req);
var result = await res.Content.ReadFromJsonAsync<QuoteResult>();
await Launcher.OpenAsync(new Uri(result.PdfUrl));

Running Locally

Prerequisites:
- .NET 8 SDK
- .NET MAUI workloads (Visual Studio 2022 with MAUI or `dotnet workload install maui`)
- Android/iOS emulators or devices (Xcode for iOS/macOS dev), Windows for WinUI

Steps:
1) Restore and build the solution in your IDE (JetBrains Rider or Visual Studio).
2) Start QuickQuote.Api.
   - Ensure Swagger loads at https://localhost:xxxxx/swagger
   - Test POST /api/quote with a payload
3) Start QuickQuote.Maui on an emulator or device.
   - Configure API base URL appropriately (see notes below)

Local base URL hints:
- Android emulator → use http://10.0.2.2:PORT
- iOS simulator on macOS → https://localhost:PORT usually works; otherwise use your Mac’s LAN IP.
- Enable HTTP (non-SSL) if desired for easier local testing, or trust dev certs on device.

Deployment to Azure (Free Tier)

Goal: API live at https://yourname.azurewebsites.net

- Create Azure App Service (Linux or Windows), runtime: .NET 8 LTS. Free F1 tier for experimentation.
- Publish from your IDE or use GitHub Actions (below). Ensure `app.UseStaticFiles()` is on for PDF access.
- Configure environment variables or app settings for DB path if needed.
- Enable CORS in Azure as needed or let your API handle it.

After deploy, update the MAUI app’s API base URL to https://yourname.azurewebsites.net.

Optional: GitHub Actions for CI/CD (API)

Create .github/workflows/deploy-api.yml with a workflow like:

name: Build and Deploy API
on:
  push:
    branches: [ main ]
    paths:
      - 'QuickQuote.Api/**'
      - '.github/workflows/deploy-api.yml'
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: dotnet restore QuickQuote.sln
      - name: Build
        run: dotnet build QuickQuote.Api/QuickQuote.Api.csproj --configuration Release --no-restore
      - name: Publish
        run: dotnet publish QuickQuote.Api/QuickQuote.Api.csproj -c Release -o publish
      # Deploy step examples:
      # - uses: azure/webapps-deploy@v2
      #   with:
      #     app-name: yourname
      #     publish-profile: ${{ secrets.AZUREAPPSERVICE_PUBLISHPROFILE }}
      #     package: publish

Store the Azure App Service Publish Profile as a GitHub secret named AZUREAPPSERVICE_PUBLISHPROFILE.

.gitignore

Ensure a .gitignore includes typical .NET, MAUI, and OS artifacts, for example (excerpt):
- bin/
- obj/
- .vs/
- .idea/
- .vscode/
- user-specific files
- platforms build outputs
- local DB files if desired (quickquote.db) unless you want to commit a seed

Roadmap (beyond MVP)

- Authentication/authorization for admin endpoints to update ServiceConfig
- Multiple service profiles per customer or per region
- Persisted quote history and email delivery
- Branded PDF templates and localization
- Offline caching in MAUI app

License

This project is provided as-is for educational/demo purposes. Adapt licensing to your organization’s needs.