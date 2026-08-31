# CIS Invoice Compliance Tool

An LLM-based tool that classifies UK subcontractor invoice line items as "Construction" (CIS-relevant) or "Services" (non-CIS) under HMRC's Construction Industry Scheme, with cited reasoning suitable for an audit trail.

## Layout

- **`POC/`**, the research and validation phase. A Python classifier (`classify.py`) tested against synthetic invoices and HMRC guidance (`cis_rules.md`), reaching 95.7% accuracy on a 47-line-item test set. See `CIS_POC_RESULTS.md` at the repo root for the full write-up. `POC/Mock Up/` is a clickable HTML/JS prototype built on real output from the validated POC runs.
- **`src/api/`**, the production ASP.NET Core Web API (`CisCompliance.sln`), layered as `Api` (controllers) / `Core` (domain + application logic) / `Infrastructure` (external clients) / `Tests` (xUnit).
- **`src/web/`**, the production Angular app.

`POC/` is kept as-is going forward as the validation record the production build is based on, not as code that ships.

## Running the API

```bash
cd src/api
dotnet run --project CisCompliance.Api --launch-profile http
```

Serves on `http://localhost:5142`. Swagger UI at `/swagger`.

## Running the web app

```bash
cd src/web
npm start
```

Serves on `http://localhost:4200`.
