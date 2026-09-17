# Project Rules: Order Management App

## Tech Stack
- Framework: Blazor (.NET)
- Database: SQLite via EF Core
- UI Component Library: Radzen Blazor (`Radzen.Blazor`)

## Core Directives
1. **Lean Code Only**: Write minimal, functional code. Avoid extra abstractions (e.g., unnecessary repositories), unused interfaces, or boilerplate text.
2. **Plan Before Coding**: Before outputting implementation code, list 3-5 bullet points of affected files and proposed changes.
3. **Token Efficiency**: Keep explanations under 3 paragraphs, show only diffs or target methods, and omit conversational fluff.
4. **Validatation** Validate all API requests using ModelState and DataAnnotations; keep input validation out of services.
5. **Exception Handling** Controllers must catch, log, and return consistent 500 ProblemDetails for unexpected exceptions while preserving appropriate 4xx or 5xx business responses.

## UI & Component Guidelines (Radzen)
- **Blazor**: Use simple single-file components (`.razor` with `@code` block) using `@using Radzen` and `@using Radzen.Blazor`.
- **Radzen First**: Use Radzen components (`RadzenDataGrid`, `RadzenButton`, `RadzenTextBox`, `RadzenNumeric`, `RadzenDropDown`, `RadzenCard`, `RadzenDialog`, etc.) instead of plain HTML elements (`<table`, `<button`, `<input`).
- **Form Controls**: Wrap forms in `RadzenTemplateForm<T>` using Radzen validator components (`RadzenRequiredValidator`, `RadzenNumericRangeValidator`) rather than standard EditForm or raw HTML inputs.
- **Layout**: Use `RadzenStack`, `RadzenRow`, `RadzenColumn`, and `RadzenCard` for component layouts.
- **No Mixed UI**: Do not mix HTML UI elements with Radzen components unless a Radzen equivalent does not exist.

## Framework & Database Guidelines
- **SQLite / EF Core**: Use `async`/`await` for database operations. Ensure short-lived `DbContext` scope to prevent SQLite file-locking or concurrency issues.
