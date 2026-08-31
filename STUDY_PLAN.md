# Study Plan, CIS Invoice Compliance Tool, PwC Presentation Prep

## Context (read this first if you're a new Claude session)

The user is presenting this codebase to a Tax Technology partner at PwC in about a week. They have a
Computer Science background but the codebase is vibe-coded and they haven't personally worked through
most of it, a database backend, .NET, and Angular are all genuinely new to them, though they already
understand backend concepts (APIs, databases, ORMs) in the abstract, just not this specific stack. They
want to actually understand the system deeply enough to present it confidently, not just memorize talking
points.

**Self-assessment from the user (given 2026-08-25):**
- Backend: knows the concepts, new to .NET/EF Core/SQL Server specifically.
- Frontend: has never used a frontend framework at all, Angular will need concepts built from zero.
- Presentation needs to cover: tax/CIS domain logic, architecture & data flow, live demo readiness, and
  the AI/OCR extraction pipeline. All four matter, nothing here is out of scope.
- Preferred format: guided walkthrough sessions together, a solo-study reference doc, and mock Q&A
  grilling closer to presentation day. ~1–2 focused hours/day.

**Key discovery:** the repo already contains most of the reference material needed, this plan doesn't
duplicate it, it sequences it:
- [`README.md`](README.md), orientation.
- [`CIS_POC_BRIEF.md`](CIS_POC_BRIEF.md), [`CIS_POC_RESULTS.md`](CIS_POC_RESULTS.md), the original Python
  POC and its 95.7%-accuracy validation.
- [`CIS_PRODUCTION_BUILD_GUIDE.md`](CIS_PRODUCTION_BUILD_GUIDE.md), a 449-line file-by-file walkthrough of
  the .NET/Angular production build, written explicitly for someone who knows the project but is new to
  .NET and Angular. Has dependency diagrams and sequence diagrams (Mermaid) for every major flow: classify,
  upload, override, OCR extraction. This is the solo-study spine for most days below.
- [`Presentation/Construction Industry Scheme.pptx`](Presentation/Construction%20Industry%20Scheme.pptx),
  an existing draft deck to polish on Day 7, not build from scratch.

Repo layout: `POC/` (Python prototype, validation record, not shipped) → `src/api/` (ASP.NET Core, layered
`Api`/`Core`/`Infrastructure`/`Tests`) → `src/web/` (Angular). Current git branch is `Persistence-Layer`.

## How to use this doc

Each day has: what to read solo, what we do together, and a goal you should be able to hit by the end of
the session. Check off days as they're completed so progress survives a new session. If you're a Claude
session picking this up cold: read the "Context" section above, check which days are marked done below,
and pick up from the next unchecked one, don't re-plan from scratch unless the user asks you to.

## Day-by-day

- [ ] **Day 1, The story arc.**
  Solo read: `CIS_POC_BRIEF.md`, `CIS_POC_RESULTS.md`, `README.md`, Build Guide §1–2 (~30 min, all short).
  Together: talk through the narrative, what CIS is, why misclassification is a real compliance risk, why
  a POC came before production, what "95.7% validated" actually means and doesn't mean.
  Goal: tell this story in 60 seconds, cold.

- [ ] **Day 2, The classification brain (Core layer).**
  Solo read: Build Guide §4.3, then the actual files: `src/api/CisCompliance.Core/Classification/ClassificationService.cs`,
  `ClassificationPrompt.cs`, `ClassificationSchemaBuilder.cs`, `CitationResolver.cs`.
  Together: how model reasoning ties back to real HMRC rule citations; why the JSON schema's field order
  (`line_id, reasoning, rule_ids, confidence, label`) is deliberately load-bearing, reason before
  committing to a label.
  Goal: explain how a classification is defensible, not just "the AI said so."

- [ ] **Day 3, Persistence (EF Core + SQL Server).**
  Solo read: Build Guide §4.4 (Persistence) + §3.5 sequence diagrams, then
  `src/api/CisCompliance.Infrastructure/Persistence/CisComplianceDbContext.cs`, `EfInvoiceRepository.cs`,
  the migration under `Migrations/`.
  Together: map EF Core concepts onto ORM concepts already known (DbContext ~ unit of work, migrations ~
  schema version control). Centerpiece: the `PredictedLabel` vs `EffectiveLabel` two-column design, the
  actual audit trail.
  Goal: explain why there are two label columns, not one, and what that buys a compliance reviewer.

- [ ] **Day 4, OCR/extraction pipeline.**
  Solo read: Build Guide §3.6 and the Extraction row of §4.3/§4.4, then `InvoiceExtractionService.cs`,
  `OpenAiExtractionModelClient.cs`.
  Together: the deliberate fallback design, synthesize a missing invoice number, but hard-fail on a
  missing/unparsable date (because date has real downstream weight and a fabricated one would corrupt
  silently). Good evidence of thought-through failure modes, not just happy-path demo code.
  Goal: explain the extract → classify chain end to end, and why the two failure-handling choices differ.

- [ ] **Day 5, Angular from zero.**
  Together first: bare concepts before touching this repo, what a component/service/state/routing/DI
  actually are, mapped to things already known from other languages.
  Solo read: Build Guide §4.7.
  Together: trace the Upload flow (§3.4) and Override flow (§3.5) end to end, tying the frontend back to
  what's already understood about the backend from Days 2–3.
  Goal: follow a user click (Upload, or Save correction) all the way through to the database and back.

- [ ] **Day 6, Run it live, full trace, demo rehearsal.**
  Start both servers (`dotnet run --project CisCompliance.Api` in `src/api`, `npm start` in `src/web`),
  click through the app. Pick one invoice and trace it through upload → extract → classify → persist →
  override → verify in the database directly.
  Goal: narrate this trace out loud, live, without notes.

- [ ] **Day 7, Mock grilling + deck polish.**
  Claude plays the PwC partner: architecture rationale, "why send invoice data to OpenAI at all," audit
  defensibility, what's still genuinely open (Build Guide §5 lists two honest open questions, own them
  rather than dodge them). Then polish `Presentation/Construction Industry Scheme.pptx` together.
  Goal: handle unscripted questions without freezing, and a tightened deck.

## Notes / decisions made along the way

*(Add anything that changes the plan here, e.g. if a day gets skipped, compressed, or reordered, so a
new session knows the plan evolved on purpose.)*
