---
description: Standardize projects using StyleCop, apply best practices, and clean up code
---

1. Ensure the standardization script exists.
// turbo
```powershell
if (-not (Test-Path "StandardizeProject.ps1")) { throw "StandardizeProject.ps1 not found" }
```

2. Standardize all core projects in the workspace.
// turbo
```powershell
Get-ChildItem -Directory "CommandMan.*" | ForEach-Object { .\StandardizeProject.ps1 -ProjectDir $_.FullName }
```

3. Perform deep cleanup for each project:
    - [ ] Review all classes and ensure they follow the "Single Type per File" rule.
    - [ ] Add missing XML documentation to all public members.
    - [ ] Remove unused `using` statements and template files (e.g., `Class1.cs`).
    - [ ] Apply file headers with copyright information.
    - [ ] Fix all remaining StyleCop warnings.

4. Verify the integrity of the workspace.
// turbo
```powershell
dotnet build CommandMan.sln
```
