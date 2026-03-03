# .NET 10 Upgrade Execution Tasks

## Metadata

- **Scenario:** .NET Framework 4.8 to .NET 10.0 Upgrade
- **Strategy:** All-At-Once
- **Project:** Webserver.API.WebApplicationManager.csproj
- **Branch:** upgrade-to-NET10
- **Plan:** [plan.md](plan.md)
- **Assessment:** [assessment.md](assessment.md)

## Progress Dashboard

- **Total Tasks:** 12
- **Completed:** 0
- **In Progress:** 0
- **Remaining:** 12
- **Failed:** 0

**Progress**: 9/12 tasks complete (75%) ![75%](https://progress-bar.xyz/75)

---

## Phase 0: Prerequisites

### [?] TASK-001: Verify .NET 10 SDK Installation *(Completed: 2026-03-03 15:29)*

**Objective:** Ensure .NET 10 SDK is installed and available

**Actions:**
- [?] (1) Run `dotnet --list-sdks` to verify .NET 10 SDK is installed
- [?] (2) If not installed, download and install from https://dotnet.microsoft.com/download/dotnet/10.0
- [?] (3) Verify installation: `dotnet --version` should show 10.x.x

**Verification:**
- .NET 10 SDK appears in SDK list
- Can create new .NET 10 project: `dotnet new console -f net10.0`

**Estimated Impact:** Minimal

---

### [?] TASK-002: Verify Clean Working Directory *(Completed: 2026-03-03 15:30)*

**Objective:** Ensure no uncommitted changes before starting upgrade

**Actions:**
- [?] (1) Run `git status` to check for uncommitted changes
- [?] (2) If changes exist, either commit them or stash them
- [?] (3) Confirm on `upgrade-to-NET10` branch

**Verification:**
- `git status` shows clean working directory
- Current branch is `upgrade-to-NET10`

**Estimated Impact:** None

---

### [?] TASK-003: Create Pre-Upgrade Checkpoint *(Completed: 2026-03-03 15:32)*

**Objective:** Tag current state for rollback capability

**Actions:**
- [?] (1) Create git tag: `git tag -a pre-net10-upgrade -m "Before .NET 10 upgrade"`
- [?] (2) Push tag to remote: `git push origin pre-net10-upgrade`

**Verification:**
- Tag exists: `git tag -l pre-net10-upgrade`
- Tag pushed to remote

**Estimated Impact:** None

---

## Phase 1: Atomic Upgrade

### [?] TASK-004: Backup Current Project File *(Completed: 2026-03-03 15:33)*

**Objective:** Create backup of classic .csproj before conversion

**Actions:**
- [?] (1) Copy `src\WebAppManager\Webserver.API.WebApplicationManager.csproj` to `Webserver.API.WebApplicationManager.csproj.backup`
- [?] (2) Verify backup exists

**Verification:**
- Backup file exists in same directory
- Backup file content matches original

**Estimated Impact:** None

---

### [?] TASK-005: Convert Project to SDK-Style *(Completed: 2026-03-03 15:36)*

**Objective:** Replace classic .csproj with SDK-style project format

**Actions:**
- [?] (1) Read current .csproj to understand structure
- [?] (2) Create new SDK-style .csproj with following structure:
  ```xml
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <OutputType>WinExe</OutputType>
      <TargetFramework>net10.0-windows</TargetFramework>
      <UseWPF>true</UseWPF>
      <UseWindowsForms>true</UseWindowsForms>
      <Nullable>disable</Nullable>
      <LangVersion>latest</LangVersion>
      <AssemblyName>Webserver.API.WebApplicationManager</AssemblyName>
      <RootNamespace>Webserver.Api.Gui</RootNamespace>
      <ApplicationManifest>app.manifest</ApplicationManifest>
    </PropertyGroup>
  </Project>
  ```
- [?] (3) Preserve any custom properties from original project
- [?] (4) Note: Do NOT add package references yet (next task)

**Verification:**
- New .csproj uses SDK format
- `<Project Sdk="Microsoft.NET.Sdk">` is present
- `UseWPF` and `UseWindowsForms` are `true`
- Target framework is `net10.0-windows`

**Estimated Impact:** Project structure changes

---

### [?] TASK-006: Update NuGet Package References *(Completed: 2026-03-03 16:06)*

**Objective:** Update package references for .NET 10 compatibility

**Actions:**
- [?] (1) Add updated packages to .csproj:
  ```xml
  <ItemGroup>
    <!-- Updated Packages -->
    <PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="10.0.3" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.3" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.3" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
    <PackageReference Include="System.Diagnostics.DiagnosticSource" Version="10.0.3" />
    
    <!-- Unchanged Packages -->
    <PackageReference Include="MimeMapping" Version="3.1.0" />
    <PackageReference Include="Siemens.Simatic.S7.Webserver.API" Version="3.2.27" />
    <PackageReference Include="System.Runtime.CompilerServices.Unsafe" Version="6.1.2" />
  </ItemGroup>
  ```
- [?] (2) DO NOT include these packages (now in framework):
  - Microsoft.NETCore.Platforms
  - NETStandard.Library
  - System.Buffers
  - System.IO
  - System.Memory
  - System.Net.Http
  - System.Numerics.Vectors
  - System.Runtime
  - System.Security.Cryptography.Algorithms
  - System.Security.Cryptography.Encoding
  - System.Security.Cryptography.Primitives
  - System.Security.Cryptography.X509Certificates
  - System.Threading.Tasks.Extensions

**Verification:**
- .csproj contains only 8 package references
- All package versions match specification
- No framework-included packages present

**Estimated Impact:** Package ecosystem changes

---

### [?] TASK-007: Restore NuGet Packages *(Completed: 2026-03-03 16:08)*

**Objective:** Download and restore all NuGet packages

**Actions:**
- [?] (1) Run `dotnet restore src\WebAppManager\Webserver.API.WebApplicationManager.csproj`
- [?] (2) Review output for warnings or errors
- [?] (3) If conflicts, resolve by specifying explicit versions

**Verification:**
- `dotnet restore` completes successfully
- No restore warnings
- All packages downloaded to cache
- obj/project.assets.json created

**Estimated Impact:** None (package download only)

---

### [?] TASK-008: Build Project (First Attempt) *(Completed: 2026-03-03 16:10)*

**Objective:** Attempt first build and identify compilation errors

**Actions:**
- [?] (1) Run `dotnet build src\WebAppManager\Webserver.API.WebApplicationManager.csproj`
- [?] (2) Capture all build errors and warnings
- [?] (3) Categorize errors:
  - SDK-related (should be minimal)
  - API incompatibilities
  - Missing references
  - Code changes needed

**Verification:**
- Build completes (may have errors, that's expected)
- Error list captured for analysis
- Understand what needs fixing

**Expected Outcome:** May have compilation errors that need fixing

**Estimated Impact:** None (diagnostic only)

---

### [?] TASK-009: Fix Compilation Errors *(Completed: 2026-03-03 16:13)*

**Objective:** Address any compilation errors found in TASK-008

**Actions:**
- [?] (1) Review compilation errors from previous task
- [?] (2) Fix source incompatibilities (~4 expected)
- [?] (3) Common fixes:
  - **Assembly.Location issue:** If `Assembly.GetExecutingAssembly().Location` causes issues:
    - Replace with `AppContext.BaseDirectory` in `MainWindow.xaml.cs`
  - **Configuration issues:** If app.config errors occur:
    - Add `<PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.0" />` if needed
  - **URI issues:** If XAML loading fails:
    - Review and update URI construction if needed
  - **Other issues:** Fix based on compiler error messages

**Verification:**
- Each compilation error addressed
- Code changes are minimal and targeted
- No unnecessary modifications

**Estimated Impact:** 15-20 lines of code changes

---

### [?] TASK-010: Build Project (Final) *(Completed: 2026-03-03 16:15)*

**Objective:** Achieve successful build with 0 errors

**Actions:**
- [?] (1) Run `dotnet clean src\WebAppManager\Webserver.API.WebApplicationManager.csproj`
- [?] (2) Run `dotnet build src\WebAppManager\Webserver.API.WebApplicationManager.csproj`
- [?] (3) Verify 0 errors
- [?] (4) Review warnings and document any that remain
- [?] (5) Verify output directory contains executable

**Verification:**
- Build exits with code 0
- 0 compilation errors
- Executable exists in bin\Debug\net10.0-windows or bin\Release\net10.0-windows
- All dependencies present in output directory

**Estimated Impact:** None

**Commit Checkpoint:** "Build succeeds - .NET 10 upgrade complete"

---

## Phase 2: Validation

### [ ] TASK-011: Runtime Validation

**Objective:** Verify application runs and core functionality works

**Actions:**
- [ ] (1) Launch application
- [ ] (2) Verify main window displays correctly
- [ ] (3) Test core functionality:
  - [ ] Settings load from JSON
  - [ ] Settings save to JSON
  - [ ] Log viewer opens
  - [ ] File dialogs work (Open/Save)
  - [ ] Windows position correctly
- [ ] (4) Check for runtime exceptions
- [ ] (5) Test WPF UI rendering (buttons, controls, layout)
- [ ] (6) Test Windows Forms components (dialogs, Screen detection)
- [ ] (7) Document any behavioral differences from .NET Framework 4.8

**Verification:**
- Application launches without exceptions
- UI renders correctly
- Core features functional
- No crashes in common paths

**Estimated Impact:** Testing only

---

### [ ] TASK-012: Create Post-Upgrade Checkpoint and Commit

**Objective:** Finalize upgrade with git commit and tag

**Actions:**
- [ ] (1) Review all changes made
- [ ] (2) Stage all changes: `git add .`
- [ ] (3) Commit with comprehensive message:
  ```
  feat: Upgrade to .NET 10.0 LTS
  
  Complete upgrade of WebApplicationManager from .NET Framework 4.8
  to .NET 10.0 (Long Term Support).
  
  Changes:
  - Convert project to SDK-style format
  - Update target framework: net48 ? net10.0-windows
  - Enable UseWPF and UseWindowsForms
  - Remove 11 framework-included packages
  - Update 5 packages to .NET 10 versions:
    * Microsoft.Bcl.AsyncInterfaces 9.0.8 ? 10.0.3
    * Microsoft.Extensions.DependencyInjection.Abstractions 9.0.8 ? 10.0.3
    * Microsoft.Extensions.Logging.Abstractions 9.0.8 ? 10.0.3
    * Newtonsoft.Json 13.0.3 ? 13.0.4
    * System.Diagnostics.DiagnosticSource 9.0.8 ? 10.0.3
  - Fix [X] compilation errors
  
  Testing:
  - ? Builds successfully with 0 errors
  - ? Application launches and runs
  - ? Core functionality verified
  - ? WPF UI renders correctly
  - ? Windows Forms dialogs work
  
  References:
  - Assessment: .github/upgrades/assessment.md
  - Plan: .github/upgrades/plan.md
  - Tasks: .github/upgrades/tasks.md
  ```
- [ ] (4) Create post-upgrade tag: `git tag -a post-net10-upgrade -m "After .NET 10 upgrade"`
- [ ] (5) Push commit and tags: `git push origin upgrade-to-NET10 --tags`

**Verification:**
- All changes committed
- Commit message is comprehensive
- Tags created and pushed
- Ready for pull request

**Estimated Impact:** None (source control only)

---

## Completion Checklist

### Technical Criteria
- [ ] Project is SDK-style
- [ ] Targets net10.0-windows
- [ ] UseWPF and UseWindowsForms enabled
- [ ] 11 framework packages removed
- [ ] 5 packages updated to .NET 10 versions
- [ ] 3 packages kept as-is
- [ ] Builds with 0 errors
- [ ] Application launches successfully
- [ ] Core functionality works

### Quality Criteria
- [ ] No unnecessary code changes
- [ ] Minimal modifications (only upgrade-related)
- [ ] Performance acceptable
- [ ] No regressions identified

### Process Criteria
- [ ] All tasks completed sequentially
- [ ] Checkpoints created (pre and post)
- [ ] Changes committed with clear message
- [ ] Ready for pull request/merge

---

## Notes

**Execution Approach:**
- Tasks must be executed sequentially in order
- Each task builds on previous tasks
- Verify each task before proceeding to next
- If task fails, stop and resolve before continuing

**Rollback:**
- If critical issues occur, revert to `pre-net10-upgrade` tag
- Use `git reset --hard pre-net10-upgrade`

**Support:**
- Refer to [plan.md](plan.md) for detailed guidance
- Refer to [assessment.md](assessment.md) for issue details
- Microsoft .NET 10 migration documentation
- Breaking changes documentation

---

**Last Updated:** [Auto-generated]  
**Status:** Ready for Execution
