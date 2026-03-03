# .NET Framework 4.8 to .NET 10.0 Upgrade Plan

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

## Executive Summary

### Scenario Description

This plan outlines the upgrade of the WebApplicationManager solution from .NET Framework 4.8 to .NET 10.0 (Long Term Support). The solution consists of a single WPF desktop application for managing and deploying web applications to Siemens PLCs.

### Scope

**Projects to Upgrade:**
- Webserver.API.WebApplicationManager.csproj (WPF Application)

**Current State:**
- Target Framework: .NET Framework 4.8
- Project Format: Classic (non-SDK-style)
- Total LOC: 3,736
- NuGet Packages: 21 (11 framework-included, 5 requiring updates, 5 compatible)

**Target State:**
- Target Framework: .NET 10.0 (with Windows Desktop support)
- Project Format: SDK-style
- Modern NuGet package versions
- Maintained WPF and Windows Forms functionality

### Selected Strategy

**All-At-Once Strategy** - Single project upgraded in one atomic operation.

**Rationale:**
- Single project with no internal dependencies
- Clear, straightforward migration path
- WPF and Windows Forms are fully supported in .NET 10
- All required NuGet packages have compatible versions
- No security vulnerabilities requiring immediate attention
- Atomic upgrade minimizes risk and complexity

### Complexity Assessment

**Discovered Metrics:**
- **Projects:** 1
- **Total API References:** 5,962
- **API Issues:** 985 (955 binary incompatible, 4 source incompatible, 26 behavioral changes)
- **Compatible APIs:** 4,977 (83.5%)
- **Package Updates Required:** 5
- **Packages to Remove:** 11 (now included in framework)
- **Estimated LOC Impact:** 985+ lines (26.4%)

**Classification: Simple Solution**

Despite the high issue count, this is classified as "Simple" because:
1. Single project, no dependency complexity
2. API issues are primarily WPF/WinForms binary incompatibilities that resolve automatically with correct SDK and target framework
3. Well-documented migration path for WPF applications
4. All packages have clear upgrade paths

### Critical Issues

**None** - No security vulnerabilities detected in current packages.

### Recommended Approach

**All-at-once upgrade** with the following key steps:
1. Convert project to SDK-style format
2. Update target framework to `net10.0-windows`
3. Remove framework-included packages
4. Update remaining packages to .NET 10-compatible versions
5. Address compilation errors (primarily resolved by SDK conversion)
6. Validate application functionality

### Iteration Strategy

**Fast Batch Approach** (2-3 detail iterations):
- Iteration 2.x: Foundation sections (dependency analysis, strategy, risk overview)
- Iteration 3.1: Complete project details, package updates, breaking changes
- Iteration 3.2: Testing strategy, success criteria, source control

This approach is appropriate for a single-project solution with a clear migration path.

## Migration Strategy

### Approach Selection

**Selected: All-At-Once Strategy**

### Justification

**Why All-At-Once:**

1. **Single Project Architecture**
   - Only one project in the solution
   - No dependency chains to manage
   - No coordination between projects required

2. **Clear Migration Path**
   - WPF and Windows Forms are fully supported in .NET 10
   - All required packages have .NET 10-compatible versions
   - Microsoft provides comprehensive tooling for SDK-style conversion

3. **No Intermediate States Needed**
   - Cannot partially upgrade a single project
   - No value in staging or phasing
   - Atomic upgrade is cleaner and simpler

4. **Low Coordination Complexity**
   - Single codebase owner
   - No cross-team dependencies
   - Can upgrade and test as a complete unit

**Why Not Incremental:**
- Incremental strategy requires multiple projects or complex dependency graphs
- No benefits for single-project solutions
- Would add unnecessary complexity

### All-At-Once Strategy Implementation

**Ordering Principles:**

Since this is a single project, the ordering is linear:

1. **Project Structure Update**
   - Convert to SDK-style project format
   - This is a prerequisite for all subsequent changes

2. **Target Framework Update**
   - Change from `net48` to `net10.0-windows`
   - The `-windows` suffix is critical for WPF/WinForms support

3. **Package Management**
   - Remove 11 packages now included in .NET 10 framework
   - Update 5 packages to .NET 10-compatible versions
   - Retain 5 compatible packages as-is

4. **Code Compilation**
   - Build the project
   - Address any compilation errors
   - Most issues will resolve automatically with SDK conversion

5. **Validation**
   - Run application
   - Test core functionality
   - Verify WPF UI rendering
   - Test PLC deployment operations

### Risk Management per All-At-Once

**Acceptable Risks:**

- **Temporary Build Breaks:** Expected during conversion, resolved in same operation
- **API Adjustments:** Binary incompatibilities resolve with proper SDK and TFM
- **Configuration Changes:** Legacy app.config may need updates (2 issues detected)

**Mitigation:**

- Work on dedicated branch (`upgrade-to-NET10`)
- Complete all changes before testing
- Have rollback plan (revert commits)
- Test thoroughly before merging

### Parallel vs Sequential

**Sequential Execution Required:**

Even within the single project, certain operations must be sequential:

1. SDK-style conversion **BEFORE** target framework change
2. Target framework change **BEFORE** package cleanup
3. Package updates **BEFORE** code compilation
4. Successful compilation **BEFORE** runtime testing

**No Parallelization Possible:**
- Single project cannot be split
- Changes are dependent on each other
- Linear execution path

### Phase Definitions

**Phase 0: Prerequisites (if needed)**
- Verify .NET 10 SDK installed
- No `global.json` constraints detected in repository

**Phase 1: Atomic Upgrade**

All operations performed as single coordinated batch:

1. Convert project to SDK-style
2. Update target framework to `net10.0-windows`
3. Remove framework-included packages
4. Update packages to .NET 10 versions
5. Build project
6. Fix compilation errors
7. Rebuild and verify

**Deliverables:** Project builds successfully with 0 errors

**Phase 2: Validation**

1. Run application
2. Test core functionality
3. Document any behavioral changes
4. Verify WPF rendering
5. Test PLC connectivity and deployment

**Deliverables:** Application runs and core features work correctly

### Success Criteria per Phase

**Phase 0 Success:**
- .NET 10 SDK confirmed installed
- Development environment ready

**Phase 1 Success:**
- Project is SDK-style
- Targets `net10.0-windows`
- All package references correct
- Solution builds with 0 errors
- No compiler warnings related to APIs

**Phase 2 Success:**
- Application launches
- UI renders correctly
- PLC connection works
- Deployment operations function
- No runtime exceptions in core paths

### Rollback Strategy

**If Phase 1 Fails:**
- Revert all changes via git reset
- Return to `KircMax/UpdateLibrary` branch
- Reassess approach

**If Phase 2 Fails:**
- Keep Phase 1 changes (they build correctly)
- Create issues for runtime problems
- Fix incrementally in follow-up commits
- Behavioral changes may require code adjustments

### Timeline Expectations

**Phase 0:** Minimal (verify SDK)
**Phase 1:** Primary effort (conversion, packages, compilation)
**Phase 2:** Moderate (testing and validation)

**Note:** No time estimates provided. Complexity is rated as Medium overall, with most effort in Phase 1 addressing the 985+ LOC impacted by API changes.

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution has an extremely simple dependency structure:

```
Webserver.API.WebApplicationManager.csproj (Standalone WPF Application)
  ?? No project dependencies
  ?? External NuGet dependencies only
```

**Characteristics:**
- **Zero internal dependencies** - This is a standalone application
- **No dependants** - Not consumed by other projects
- **Leaf node and root node** - The project is both the entry point and has no dependencies

### Project Groupings

Since this is a single-project solution, there is only one migration phase:

**Phase 1: Complete Upgrade**
- Webserver.API.WebApplicationManager.csproj

**Migration Order:**
- No ordering required (single project)
- No intermediate states
- No coordination with other projects needed

### Critical Path

**Straightforward Path:**
1. SDK-style conversion
2. Target framework update to `net10.0-windows`
3. Package cleanup and updates
4. Compilation and testing

**No Blocking Dependencies:**
- The project can be upgraded in a single operation
- No need to wait for library projects to upgrade first
- No need to maintain compatibility with other projects

### Circular Dependencies

**None** - No circular dependencies exist.

### Technology Dependencies

**Windows Desktop Technologies:**
- **WPF (Windows Presentation Foundation)** - Primary UI framework
  - 400 API references requiring Windows Desktop support
  - Fully supported in .NET 10 via `net10.0-windows` TFM
  
- **Windows Forms** - Secondary UI components
  - 123 API references (used for dialogs and some controls)
  - Fully supported in .NET 10 via `net10.0-windows` TFM

**External Library:**
- **Siemens.Simatic.S7.Webserver.API (v3.2.27)** - Core functionality library
  - Compatible with .NET Standard 2.0+
  - No upgrade required
  - Will continue to work with .NET 10

### Dependency-Based Migration Strategy

**All-At-Once Rationale:**

Given the single-project structure:
- No dependency ordering constraints
- No multi-project coordination needed
- No intermediate compatibility requirements
- Clean, atomic upgrade possible

The project can and should be upgraded as a single unit, making the All-At-Once strategy the natural and only viable approach.

## Project-by-Project Migration Plans

### Project: Webserver.API.WebApplicationManager.csproj

**Location:** `src\WebAppManager\Webserver.API.WebApplicationManager.csproj`

**Current State:**
- Target Framework: `net48`
- Project Format: Classic (non-SDK-style)
- Project Type: WPF Application (ClassicWinForms)
- Dependencies: 0 project references
- NuGet Packages: 21
- Code Files: 68 (28 with API issues)
- Lines of Code: 3,736
- Estimated Impact: 985+ lines (26.4%)

**Target State:**
- Target Framework: `net10.0-windows`
- Project Format: SDK-style
- NuGet Packages: 10 (11 removed, 5 updated, 5 unchanged)

### Project: Webserver.API.WebApplicationManager.csproj

**Location:** `src\WebAppManager\Webserver.API.WebApplicationManager.csproj`

**Current State:**
- Target Framework: `net48`
- Project Format: Classic (non-SDK-style)
- Project Type: WPF Application (ClassicWinForms)
- Dependencies: 0 project references
- NuGet Packages: 21
- Code Files: 68 (28 with API issues)
- Lines of Code: 3,736
- Estimated Impact: 985+ lines (26.4%)

**Target State:**
- Target Framework: `net10.0-windows`
- Project Format: SDK-style
- NuGet Packages: 10 (11 removed, 5 updated, 5 unchanged)

**Technologies Used:**
- WPF (Windows Presentation Foundation) - Primary UI framework
- Windows Forms - Dialog components (SaveFileDialog, OpenFileDialog, Screen)
- Newtonsoft.Json - JSON serialization
- Siemens.Simatic.S7.Webserver.API - PLC communication
- Microsoft.Extensions.Logging - Logging infrastructure

#### Migration Steps

**Step 1: Convert to SDK-Style Project**

The current project uses the classic .csproj format. This must be converted to SDK-style.

**Actions:**
1. Create backup of current `.csproj` file
2. Use .NET Upgrade Assistant or manual conversion
3. Replace project file content with SDK-style structure:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>disable</Nullable>
    <LangVersion>latest</LangVersion>
    <ApplicationManifest>app.manifest</ApplicationManifest>
  </PropertyGroup>
  
  <!-- Package references will be added in next step -->
</Project>
```

**Key Properties:**
- `Sdk="Microsoft.NET.Sdk"` - Use modern SDK
- `OutputType>WinExe` - Windows application (not console)
- `UseWPF>true` - Enable WPF support
- `UseWindowsForms>true` - Enable Windows Forms support
- `Nullable>disable` - Maintain C# 7.3 nullable behavior
- `LangVersion>latest` - Enable modern C# features

**Expected Impact:**
- Project file shrinks significantly (classic files are verbose)
- XAML files auto-included via glob patterns
- Code files auto-included
- References simplified

**Step 2: Update Target Framework**

Already included in SDK-style conversion above:
- Change from `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>`
- To `<TargetFramework>net10.0-windows</TargetFramework>`

**Critical:** The `-windows` suffix is required for WPF and Windows Forms support.

**Step 3: Update Package References**

See [Package Update Reference](#package-update-reference) section for complete details.

**Actions:**
1. Remove 11 packages now included in .NET 10 framework
2. Update 5 packages to .NET 10-compatible versions
3. Retain 5 packages as-is (already compatible)

**Package Summary:**
- **Remove:** Microsoft.NETCore.Platforms, NETStandard.Library, System.Buffers, System.IO, System.Memory, System.Net.Http, System.Numerics.Vectors, System.Runtime, System.Security.Cryptography.*, System.Threading.Tasks.Extensions
- **Update:** Microsoft.Bcl.AsyncInterfaces, Microsoft.Extensions.*, Newtonsoft.Json, System.Diagnostics.DiagnosticSource
- **Keep:** MimeMapping, Siemens.Simatic.S7.Webserver.API, System.Runtime.CompilerServices.Unsafe

**Step 4: Address Expected Breaking Changes**

See [Breaking Changes Catalog](#breaking-changes-catalog) for comprehensive list.

**Primary Categories:**

1. **WPF Binary Incompatibilities (400 issues)**
   - **Expected:** Most resolve automatically with SDK conversion and `net10.0-windows` TFM
   - **Action:** Build project after conversion
   - **If errors persist:** Check that `UseWPF` is set to `true`

2. **Windows Forms Binary Incompatibilities (123 issues)**
   - **Expected:** Resolve with `UseWindowsForms` enabled
   - **Action:** Verify Windows Forms dialogs still work
   - **Specific attention:** `System.Windows.Forms.Screen`, `SaveFileDialog`, `OpenFileDialog`

3. **URI Behavioral Changes (26 issues)**
   - **Impact:** XAML resource loading uses URIs
   - **Action:** Test application startup and resource loading
   - **Location:** `MainWindow.xaml.cs`, `WebAppConfigCreatorWindow.xaml.cs`, etc.
   - **Example:** `new Uri("/Pages/LoginDialog.xaml", UriKind.Relative)`

4. **Assembly.Location Changes**
   - **Location:** `MainWindow.xaml.cs` line ~91: `Assembly.GetExecutingAssembly().Location`
   - **Issue:** Single-file deployments return empty string
   - **Solution:** Use `AppContext.BaseDirectory` or test current implementation

5. **Legacy Configuration (2 issues)**
   - **Impact:** If using app.config for application settings
   - **Action:** Either migrate to modern configuration or add `System.Configuration.ConfigurationManager` NuGet package

**Step 5: Build and Fix Compilation Errors**

**Expected Compilation State:**
- Most API issues resolve with proper SDK and TFM
- Possible remaining errors:
  - Missing package references
  - Assembly location issues
  - Configuration-related errors

**Build Process:**
1. Restore NuGet packages: `dotnet restore`
2. Build project: `dotnet build`
3. Review errors
4. Fix any actual code issues (not SDK-related)
5. Rebuild until 0 errors

**Common Fixes:**

| Error Type | Solution |
| :--- | :--- |
| WPF types not found | Verify `UseWPF>true` in .csproj |
| Windows Forms types not found | Verify `UseWindowsForms>true` in .csproj |
| Missing System.Configuration | Add `System.Configuration.ConfigurationManager` NuGet package |
| File path issues | Replace `Assembly.Location` with `AppContext.BaseDirectory` |

**Step 6: Validation Checklist**

After successful build:

- [ ] **Project Structure**
  - [ ] Project is SDK-style
  - [ ] Targets `net10.0-windows`
  - [ ] All XAML files included
  - [ ] All code files included
  - [ ] Resources embedded correctly

- [ ] **Dependencies**
  - [ ] No framework-included packages remain
  - [ ] All packages at correct versions
  - [ ] No dependency conflicts

- [ ] **Build**
  - [ ] Builds with 0 errors
  - [ ] Builds with 0 warnings (or documented warnings only)
  - [ ] Output directory contains executable

- [ ] **Runtime (Initial)**
  - [ ] Application launches
  - [ ] Main window displays
  - [ ] No immediate exceptions

**Step 7: Functional Testing**

See [Testing & Validation Strategy](#testing--validation-strategy) for detailed testing plan.

**Core Functionality to Verify:**
1. Application startup and UI rendering
2. Settings load/save (JSON serialization)
3. PLC connection and authentication
4. Web app deployment operations
5. File dialogs (Open/Save)
6. Window positioning across multiple monitors
7. Logging functionality

**Risk Level: Medium**

**Justification:**
- High API issue count (985) but mostly auto-resolved
- SDK conversion is well-documented
- Active technology stack (WPF is supported)
- Clear package migration paths
- No security vulnerabilities

**Dependencies:**
- None (standalone project)

**Dependants:**
- None (not consumed by other projects)

## Package Update Reference

### Package Categories

All packages fall into three categories for this upgrade:

1. **Remove (11 packages)** - Now included in .NET 10 framework
2. **Update (5 packages)** - Need version updates for .NET 10 compatibility
3. **Keep (5 packages)** - Already compatible, no changes needed

### Packages to Remove

These packages provided .NET Standard 2.0 / .NET Framework compatibility but are now built into .NET 10:

| Package | Current Version | Reason for Removal |
| :--- | :---: | :--- |
| Microsoft.NETCore.Platforms | 1.1.0 | Platform abstractions included in .NET 10 |
| NETStandard.Library | 2.0.3 | .NET Standard support built-in |
| System.Buffers | 4.6.1 | Core library, included in framework |
| System.IO | 4.3.0 | Core library, included in framework |
| System.Memory | 4.6.3 | Core library, included in framework |
| System.Net.Http | 4.3.4 | Core library, included in framework |
| System.Numerics.Vectors | 4.6.1 | Core library, included in framework |
| System.Runtime | 4.3.1 | Core library, included in framework |
| System.Security.Cryptography.Algorithms | 4.3.1 | Core library, included in framework |
| System.Security.Cryptography.Encoding | 4.3.0 | Core library, included in framework |
| System.Security.Cryptography.Primitives | 4.3.0 | Core library, included in framework |
| System.Security.Cryptography.X509Certificates | 4.3.2 | Core library, included in framework |
| System.Threading.Tasks.Extensions | 4.6.3 | Core library, included in framework |

**Action:** Remove these `<PackageReference>` entries from the .csproj file.

**Impact:** None - functionality is maintained through framework references.

### Packages to Update

These packages need version updates for optimal .NET 10 compatibility:

| Package | Current Version | Target Version | Projects | Update Reason |
| :--- | :---: | :---: | :--- | :--- |
| Microsoft.Bcl.AsyncInterfaces | 9.0.8 | 10.0.3 | Webserver.API.WebApplicationManager | .NET 10 compatibility and improvements |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8 | 10.0.3 | Webserver.API.WebApplicationManager | .NET 10 compatibility and improvements |
| Microsoft.Extensions.Logging.Abstractions | 9.0.8 | 10.0.3 | Webserver.API.WebApplicationManager | .NET 10 compatibility and improvements |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Webserver.API.WebApplicationManager | Bug fixes and compatibility |
| System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.3 | Webserver.API.WebApplicationManager | .NET 10 compatibility and improvements |

**Action:** Update package versions in .csproj:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="10.0.3" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.3" />
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.3" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
  <PackageReference Include="System.Diagnostics.DiagnosticSource" Version="10.0.3" />
</ItemGroup>
```

**Impact:** 
- **Microsoft.Extensions.*** packages: Better integration with .NET 10 dependency injection and logging
- **Newtonsoft.Json**: Bug fixes, continues to work alongside System.Text.Json
- **System.Diagnostics.DiagnosticSource**: Improved diagnostic capabilities

### Packages to Keep (No Changes)

These packages are already compatible with .NET 10:

| Package | Version | Compatibility Note |
| :--- | :---: | :--- |
| MimeMapping | 3.1.0 | Targets .NET Standard 2.0+, fully compatible |
| Siemens.Simatic.S7.Webserver.API | 3.2.27 | Targets .NET Standard 2.0+, fully compatible |
| System.Runtime.CompilerServices.Unsafe | 6.1.2 | Compatible with .NET 10 |

**Action:** No changes required. Keep existing `<PackageReference>` entries.

**Note:** `System.Runtime.CompilerServices.Unsafe` may be a transitive dependency and could potentially be removed if not directly used.

### Complete Package Reference Section

After migration, the `<ItemGroup>` for packages should look like this:

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

### Verification Steps

After package updates:

1. **Restore packages:** `dotnet restore`
2. **Check for conflicts:** Review restore output for warnings
3. **Verify versions:** Check that all packages resolved to correct versions
4. **Build:** Ensure build succeeds with updated packages
5. **Test:** Verify functionality dependent on updated packages (logging, serialization)

### Package Update Priority

**High Priority:**
- **Microsoft.Extensions.Logging.Abstractions** - Used by custom logging infrastructure
- **Newtonsoft.Json** - Core to settings serialization/deserialization

**Medium Priority:**
- **System.Diagnostics.DiagnosticSource** - Diagnostic and telemetry features
- **Microsoft.Extensions.DependencyInjection.Abstractions** - Used by Siemens API

**Low Priority:**
- **Microsoft.Bcl.AsyncInterfaces** - Likely a transitive dependency
- **System.Runtime.CompilerServices.Unsafe** - Likely a transitive dependency

### Dependency Resolution Notes

**No conflicts expected:**
- All updated packages are aligned to .NET 10.0.x versions
- Siemens.Simatic.S7.Webserver.API targets .NET Standard 2.0, which is compatible with .NET 10
- No known incompatibilities between package versions

**If conflicts occur:**
1. Use `dotnet list package --include-transitive` to view full dependency graph
2. Identify conflicting package versions
3. Add explicit `<PackageReference>` to force specific version if needed

## Breaking Changes Catalog

### Overview

The analysis identified **985 API issues** across 28 files. These break down into three categories:

| Category | Count | Auto-Resolves | Requires Code Changes |
| :--- | :---: | :---: | :---: |
| Binary Incompatible | 955 | ~940 | ~15 |
| Source Incompatible | 4 | 0 | 4 |
| Behavioral Changes | 26 | N/A | Testing required |
| **Total** | **985** | **~940** | **~19** |

**Key Insight:** Most issues (95%+) are binary incompatibilities in WPF and Windows Forms that resolve automatically when using the correct SDK and target framework (`net10.0-windows` with `UseWPF` and `UseWindowsForms` enabled).

### Category 1: WPF Binary Incompatibilities (400 issues)

**Description:** WPF types moved to separate assemblies in .NET Core/.NET 5+. The binary contract changed but APIs remained functionally the same.

**Resolution:** Enable WPF support in SDK-style project.

**Project Configuration:**
```xml
<UseWPF>true</UseWPF>
<TargetFramework>net10.0-windows</TargetFramework>
```

**Affected APIs (Top 20):**

| API | Occurrences | Category | Auto-Resolves |
| :--- | :---: | :--- | :---: |
| System.Windows.Controls.Button | 67 | Binary Incompatible | ? Yes |
| System.Windows.RoutedEventHandler | 48 | Binary Incompatible | ? Yes |
| System.Windows.Application | 30 | Binary Incompatible | ? Yes |
| System.Windows.DependencyProperty | 26 | Binary Incompatible | ? Yes |
| System.Windows.RoutedEventArgs | 24 | Binary Incompatible | ? Yes |
| System.Windows.MessageBoxResult | 22 | Binary Incompatible | ? Yes |
| System.Windows.Controls.RadioButton | 21 | Binary Incompatible | ? Yes |
| System.Windows.Controls.ComboBox | 21 | Binary Incompatible | ? Yes |
| System.Windows.Window | 20 | Binary Incompatible | ? Yes |
| System.Windows.Controls.Primitives.ButtonBase.Click | 20 | Binary Incompatible | ? Yes |
| System.Windows.Controls.ListBox | 20 | Binary Incompatible | ? Yes |
| System.Windows.WindowStartupLocation | 18 | Binary Incompatible | ? Yes |
| System.Windows.MessageBox | 18 | Binary Incompatible | ? Yes |
| System.Windows.Threading.DispatcherTimer | 18 | Binary Incompatible | ? Yes |
| System.Windows.FrameworkElement.DataContext | 16 | Binary Incompatible | ? Yes |
| System.Windows.Controls.UserControl | 12 | Binary Incompatible | ? Yes |
| System.Windows.Controls.MenuItem | 12 | Binary Incompatible | ? Yes |
| System.Windows.Markup.IComponentConnector | 11 | Binary Incompatible | ? Yes |
| System.Windows.Controls.SelectionChangedEventHandler | 10 | Binary Incompatible | ? Yes |
| System.Windows.Media.CompositionTarget | 9 | Binary Incompatible | ? Yes |

**Action Required:** None (automatic)

**Validation:** Build project and verify WPF UI renders correctly.

### Category 2: Windows Forms Binary Incompatibilities (123 issues)

**Description:** Windows Forms types also moved to separate assemblies. Same binary incompatibility, same resolution.

**Resolution:** Enable Windows Forms support in SDK-style project.

**Project Configuration:**
```xml
<UseWindowsForms>true</UseWindowsForms>
<TargetFramework>net10.0-windows</TargetFramework>
```

**Affected APIs (Top 10):**

| API | Occurrences | Category | Auto-Resolves |
| :--- | :---: | :--- | :---: |
| System.Windows.Forms.DialogResult | 22 | Binary Incompatible | ? Yes |
| System.Windows.Forms.Screen | 18 | Binary Incompatible | ? Yes |
| System.Windows.Forms.MessageBoxIcon | 10 | Binary Incompatible | ? Yes |
| System.Windows.Forms.MessageBoxButtons | 10 | Binary Incompatible | ? Yes |
| System.Windows.Forms.MessageBox | 7 | Binary Incompatible | ? Yes |
| System.Windows.Forms.OpenFileDialog | 6 | Binary Incompatible | ? Yes |
| System.Windows.Forms.SaveFileDialog | 6 | Binary Incompatible | ? Yes |
| System.Windows.Forms.FileDialog | 4 | Binary Incompatible | ? Yes |

**Action Required:** None (automatic)

**Validation:** Test file dialogs (Open/Save) and message boxes.

### Category 3: URI Behavioral Changes (26 issues)

**Description:** URI parsing and construction behavior changed in .NET Core to be more standards-compliant.

**Impact:** Affects XAML resource loading and any URI manipulation.

**Affected Code:**

| API | Occurrences | Impact |
| :--- | :---: | :--- |
| System.Uri | 14 | Behavioral Change |
| System.Uri.#ctor(System.String,System.UriKind) | 12 | Behavioral Change |

**Locations:**
- XAML files using Pack URIs for resources
- `Application.LoadComponent(object, Uri)` calls (11 occurrences)
- Programmatic URI construction

**Example from codebase:**
```csharp
// MainWindow.xaml.cs, LoginDialog.xaml.cs, etc.
System.Windows.Application.LoadComponent(this, new System.Uri("/Webserver.API.WebApplicationManager;component/MainWindow.xaml", System.UriKind.Relative));
```

**Known Changes:**
1. **Stricter URI validation** - Invalid URIs may now throw where they were previously accepted
2. **Changed escaping behavior** - Some characters are now escaped differently
3. **Pack URI handling** - WPF pack URIs should still work but may behave differently in edge cases

**Action Required:**
1. ? **Test application startup** - Verify XAML loading works
2. ? **Test window creation** - Verify all windows/dialogs load correctly
3. ?? **Review any URI manipulation code** - Check if any code builds or parses URIs dynamically

**Mitigation:** If issues occur:
- Update URI strings to be standards-compliant
- Use `UriKind.Absolute` or `UriKind.Relative` explicitly
- Test edge cases thoroughly

### Category 4: Assembly.Location Changes

**Description:** `Assembly.GetExecutingAssembly().Location` behaves differently in .NET Core/5+, especially with single-file deployments.

**Affected Code:**
- **File:** `MainWindow.xaml.cs`
- **Line:** ~91
- **Property:** `CurrentExeDir`

```csharp
public static DirectoryInfo CurrentExeDir
{
    get
    {
        string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        return (new FileInfo(dllPath)).Directory;
    }
}
```

**Issue:**
- In single-file deployments, `Location` returns an empty string
- In regular deployments, returns location of assembly (may be different from .exe location)

**Recommended Fix:**

**Option 1 (Recommended):** Use `AppContext.BaseDirectory`
```csharp
public static DirectoryInfo CurrentExeDir
{
    get
    {
        return new DirectoryInfo(AppContext.BaseDirectory);
    }
}
```

**Option 2:** Use `Environment.ProcessPath` + `Path.GetDirectoryName`
```csharp
public static DirectoryInfo CurrentExeDir
{
    get
    {
        string processPath = Environment.ProcessPath 
            ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
        return (new FileInfo(processPath)).Directory;
    }
}
```

**Action Required:**
- ?? **Test** current implementation with .NET 10
- If file path issues occur, apply recommended fix
- Verify settings file loading still works

### Category 5: Legacy Configuration System (2 issues)

**Description:** The old `app.config` XML configuration system is not included by default in .NET Core/5+.

**Affected APIs:**
- System.Configuration APIs (2 references detected)

**Resolution Options:**

**Option 1 (Quick Fix):** Add compatibility package
```xml
<PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.0" />
```

**Option 2 (Recommended Long-Term):** Migrate to modern configuration
- Use `Microsoft.Extensions.Configuration` (already referenced via dependencies)
- Use JSON configuration files
- Current codebase already uses JSON for settings (`SaveSettingsToJsonFile`), so may not need app.config

**Action Required:**
1. ? **Check if app.config exists** in project
2. ? **Determine if it's used** for application settings
3. ?? **If used:** Add ConfigurationManager package or migrate to JSON configuration
4. ? **If not used:** No action needed

### Category 6: Source Incompatibilities (4 issues)

**Description:** APIs that changed signatures or were removed entirely.

**Impact:** Will cause compilation errors that must be fixed.

**Status:** Assessment did not provide specific API details for the 4 source incompatibilities.

**Action Required:**
1. ?? **Build the project** after SDK conversion
2. ?? **Review compilation errors**
3. ?? **Fix any source incompatibilities** based on error messages
4. Common patterns:
   - Changed method signatures
   - Removed overloads
   - Changed return types

**Mitigation:**
- Check Microsoft's breaking changes documentation for .NET 10
- Use compiler error messages to identify exact issues
- Apply fixes based on recommended alternatives

### Summary of Required Actions

| Category | Auto-Resolves | Requires Testing | Requires Code Changes |
| :--- | :---: | :---: | :---: |
| WPF (400) | ? Yes | ? Yes | ? No |
| Windows Forms (123) | ? Yes | ? Yes | ? No |
| URI Behavioral (26) | N/A | ? Yes | ?? Maybe |
| Assembly.Location | N/A | ? Yes | ?? Maybe |
| Legacy Config (2) | N/A | ? Yes | ?? Maybe |
| Source Incompatible (4) | ? No | ? Yes | ?? Likely |

**Overall Action Plan:**
1. ? Convert to SDK-style with WPF/WinForms enabled
2. ? Build and fix source incompatibilities (~4 issues)
3. ? Test application thoroughly
4. ?? Fix any runtime issues (URI, Assembly.Location, Configuration)
5. ? Validate all functionality

**Estimated Code Changes:** 15-20 lines of actual code changes (most issues auto-resolve).

## Risk Management

### High-Level Risk Assessment

**Overall Risk Level: Medium**

**Risk Factors:**

1. **WPF/Windows Forms Dependencies (523 API issues combined)**
   - **Severity:** Medium
   - **Likelihood:** Low
   - **Impact:** These are binary incompatibilities that resolve with proper SDK conversion
   - **Mitigation:** Use `net10.0-windows` TFM and modern SDK

2. **Assembly Location Changes**
   - **Severity:** Low
   - **Impact:** Code uses `Assembly.GetExecutingAssembly().Location` which behaves differently
   - **Mitigation:** Test file path resolution; consider using `AppContext.BaseDirectory`

3. **Legacy Configuration System (2 issues)**
   - **Severity:** Low
   - **Impact:** May use app.config for settings
   - **Mitigation:** Migrate to modern configuration or use ConfigurationManager NuGet package

4. **Third-Party Package Compatibility**
   - **Severity:** Low
   - **Impact:** Siemens.Simatic.S7.Webserver.API may have hidden dependencies
   - **Mitigation:** Package targets netstandard2.0, should work; test thoroughly

5. **URI Behavioral Changes (26 issues)**
   - **Severity:** Low
   - **Impact:** URI parsing behavior has changed in .NET Core/10
   - **Mitigation:** Test XAML resource loading and any URI manipulation

### Detailed Risk Table

| Risk | Severity | Likelihood | Impact | Mitigation | Owner |
| :--- | :---: | :---: | :--- | :--- | :--- |
| SDK conversion fails | Medium | Low | Cannot proceed with upgrade | Use .NET Upgrade Assistant; manual fallback | Developer |
| WPF UI fails to render | High | Low | Application unusable | Ensure `UseWPF=true`; test on clean .NET 10 install | Developer |
| Windows Forms dialogs fail | Medium | Low | Some features broken | Ensure `UseWindowsForms=true`; test file dialogs | Developer |
| Assembly.Location returns empty | Medium | Medium | Settings/files not found | Use `AppContext.BaseDirectory` instead | Developer |
| URI loading fails | Medium | Low | XAML resources not loaded | Test thoroughly; update URI strings if needed | Developer |
| Siemens API incompatibility | High | Very Low | Core functionality broken | Package is .NET Standard 2.0, should work; test early | Developer |
| Package dependency conflicts | Medium | Low | Build failures | Use explicit package versions; check transitive dependencies | Developer |
| Source incompatibilities | Medium | Medium | Compilation errors | Fix based on compiler errors; ~4 issues expected | Developer |
| Performance regression | Low | Low | Slower application | Benchmark critical paths; .NET 10 typically faster | Developer |
| Single-file deployment issues | Low | Low | Deployment complexity | Test standard deployment first; single-file is optional | DevOps |

### Security Vulnerabilities

**Status:** ? **None detected**

The analysis found no security vulnerabilities in the current NuGet packages.

**Post-Upgrade Monitoring:**
- Run `dotnet list package --vulnerable` after upgrade
- Review updated package release notes
- Subscribe to security advisories for Siemens.Simatic.S7.Webserver.API

### Contingency Plans

#### If SDK Conversion Fails

**Symptoms:**
- Project won't load in Visual Studio
- Build fails with SDK-related errors
- XAML files not recognized

**Contingency:**
1. Revert to classic .csproj (git reset)
2. Use .NET Upgrade Assistant tool instead of manual conversion
3. Review conversion logs for specific issues
4. Convert incrementally (remove packages first, then convert SDK)

**Rollback:** Full revert via git

#### If WPF/Windows Forms APIs Don't Resolve

**Symptoms:**
- Build errors referencing System.Windows.* types
- "Type or namespace not found" errors for WPF/WinForms

**Contingency:**
1. Verify `.csproj` has `<UseWPF>true</UseWPF>` and `<UseWindowsForms>true</UseWindowsForms>`
2. Verify target framework is `net10.0-windows` (not just `net10.0`)
3. Clean and rebuild (`dotnet clean`, `dotnet build`)
4. Check .NET 10 SDK installation
5. Try explicit framework references if needed (should not be necessary)

**Rollback:** Fix .csproj configuration

#### If Siemens API Fails

**Symptoms:**
- Runtime exceptions when calling PLC API
- Assembly loading failures
- Dependency resolution errors

**Contingency:**
1. Check if Siemens has a .NET 6+ native version
2. Test Siemens API in isolation (simple console app)
3. Review Siemens documentation for .NET Core compatibility notes
4. Contact Siemens support
5. Consider wrapping in .NET Framework shim library (worst case)

**Rollback:** If no resolution, may need to defer upgrade until Siemens updates

#### If Assembly.Location Issues Occur

**Symptoms:**
- Settings file not found
- README.html not found
- Directory operations fail

**Contingency:**
1. Replace `Assembly.GetExecutingAssembly().Location` with `AppContext.BaseDirectory`
2. Test file path resolution
3. Verify working directory assumptions

**Example Fix:**
```csharp
// Before
string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
return (new FileInfo(dllPath)).Directory;

// After
return new DirectoryInfo(AppContext.BaseDirectory);
```

**Rollback:** Apply code fix (no rollback needed)

#### If URI/XAML Loading Fails

**Symptoms:**
- Application crashes on startup
- "Cannot locate resource" exceptions
- XAML files not loaded

**Contingency:**
1. Review Pack URI syntax in XAML
2. Verify assembly name matches in pack URIs
3. Test relative vs absolute URIs
4. Check build action for XAML files (should be "Page")
5. Review .NET 10 WPF breaking changes documentation

**Rollback:** Update URI strings based on new behavior

#### If Source Incompatibilities Can't Be Resolved

**Symptoms:**
- Compilation errors that can't be fixed
- No available replacement APIs
- Critical functionality depends on removed API

**Contingency:**
1. Research Microsoft's breaking changes documentation
2. Search for community solutions
3. Refactor code to use alternative approach
4. If truly blocked, may need to defer upgrade

**Rollback:** Full revert via git; reassess upgrade path

### Risk Mitigation Strategies

**Pre-Upgrade:**
1. ? Create dedicated upgrade branch (`upgrade-to-NET10`)
2. ? Ensure clean working directory
3. ? Document current working state
4. ? Verify .NET 10 SDK installed

**During Upgrade:**
1. ? Make incremental commits
2. ? Test at each major milestone
3. ? Document any unexpected issues
4. ? Keep communication open with team

**Post-Upgrade:**
1. ? Comprehensive testing before merge
2. ? Create rollback plan
3. ? Monitor for runtime issues
4. ? Gather user feedback

### Success Probability

**High Confidence (>90%):** 
- SDK conversion will succeed
- WPF/WinForms APIs will resolve
- Build will complete successfully

**Medium Confidence (70-90%):**
- No Assembly.Location issues
- No URI/XAML loading issues
- Source incompatibilities easily fixed

**Lower Confidence (50-70%):**
- Zero runtime issues on first run
- Perfect compatibility with Siemens API
- No unexpected behavioral changes

**Overall Assessment:** 
- **85% probability** of smooth upgrade with minor fixes
- **15% probability** of needing significant troubleshooting
- **<1% probability** of upgrade being infeasible

## Testing & Validation Strategy

### Multi-Level Testing Approach

Testing proceeds in three levels: per-project (compilation), phase (functional), and full solution (integration).

#### Level 1: Per-Project Testing

After SDK conversion and before moving to runtime testing:

**Build Validation:**
- [ ] Project loads in Visual Studio/IDE without errors
- [ ] `dotnet restore` completes successfully
- [ ] `dotnet build` completes with 0 errors
- [ ] `dotnet build` completes with 0 warnings (or only expected warnings documented)
- [ ] Output directory contains `.exe` file
- [ ] Output directory contains all dependencies

**Package Validation:**
- [ ] No dependency conflicts in restore output
- [ ] All packages resolved to expected versions
- [ ] No security vulnerabilities (`dotnet list package --vulnerable`)
- [ ] No deprecated packages (`dotnet list package --deprecated`)

**Project Structure Validation:**
- [ ] All XAML files included in build
- [ ] All code files included in build
- [ ] Embedded resources present (icons, README.html, etc.)
- [ ] Build configuration preserved (Debug/Release)

#### Level 2: Phase Testing

**Phase 1 Testing (After Atomic Upgrade):**

**Build Testing:**
- [ ] Clean build succeeds
- [ ] Rebuild succeeds
- [ ] Release configuration builds
- [ ] Debug configuration builds
- [ ] No unexpected warnings

**Static Analysis:**
- [ ] No new code analysis warnings
- [ ] No new nullable reference warnings (if enabled)
- [ ] Assembly attributes correct
- [ ] Version information intact

**Phase 2 Testing (Runtime Validation):**

**Application Launch:**
- [ ] Application starts without exceptions
- [ ] Main window displays correctly
- [ ] Window title shows version number
- [ ] No console errors/warnings

**UI Rendering:**
- [ ] All WPF controls render correctly
- [ ] Layout is preserved
- [ ] Styles and themes apply correctly
- [ ] Images and icons display
- [ ] Menu items functional

**Core Functionality:**
- [ ] Settings load from JSON file
- [ ] Settings save to JSON file
- [ ] Log viewer window opens
- [ ] Logging infrastructure works
- [ ] Progress bar updates correctly

**Windows Forms Integration:**
- [ ] File dialogs (Open/Save) display correctly
- [ ] File dialogs can browse and select files
- [ ] Message boxes display correctly
- [ ] Screen/monitor detection works (multi-monitor support)

**PLC Integration:**
- [ ] Can connect to PLC (if test PLC available)
- [ ] Authentication works
- [ ] Permission checks work
- [ ] Deployment operations function
- [ ] Error handling works correctly

**Configuration:**
- [ ] Rack configuration loading works
- [ ] Web app configuration loading works
- [ ] Settings persistence works
- [ ] Recent items tracked correctly

#### Level 3: Full Solution Testing

Since this is a single-project solution, Level 3 is the same as Level 2.

### Functional Test Cases

#### Test Case 1: Fresh Start
**Precondition:** No existing settings file  
**Steps:**
1. Launch application
2. Verify default settings created
3. Close application
4. Relaunch
5. Verify settings persisted

**Expected:** Settings file created in `Settings/WebApplicationManager.json`

#### Test Case 2: Settings Management
**Precondition:** Application running  
**Steps:**
1. Add a rack configuration
2. Add a web app configuration
3. Select items for deployment
4. Save settings
5. Close and relaunch
6. Verify selections restored

**Expected:** All selections and configurations persist across restarts

#### Test Case 3: File Dialogs
**Precondition:** Application running  
**Steps:**
1. Click "Add Rack Configuration"
2. Browse to file
3. Select file
4. Verify file added
5. Repeat for web app configuration

**Expected:** Windows Forms file dialogs work correctly

#### Test Case 4: PLC Connection (if PLC available)
**Precondition:** Test PLC accessible on network  
**Steps:**
1. Configure PLC in rack
2. Select deployment items
3. Start deployment
4. Enter credentials
5. Verify deployment proceeds

**Expected:** Connection established, deployment executes

#### Test Case 5: Multi-Monitor Support
**Precondition:** Multiple monitors connected  
**Steps:**
1. Open application on Monitor 1
2. Create rack configuration window
3. Verify window positions correctly
4. Move to Monitor 2
5. Close and reopen

**Expected:** Windows position correctly across monitors

#### Test Case 6: Error Handling
**Precondition:** Application running  
**Steps:**
1. Attempt deployment with no PLC configured
2. Verify error message
3. Attempt to load invalid JSON file
4. Verify error handling

**Expected:** Graceful error messages, no crashes

#### Test Case 7: Logging
**Precondition:** Application running  
**Steps:**
1. Open log viewer window
2. Perform various operations
3. Verify log entries appear
4. Change log level
5. Verify filtering works

**Expected:** Logging infrastructure functional

### Performance Testing

**Baseline Metrics (to be established):**
- Application startup time
- Settings load time
- UI responsiveness
- Deployment operation time

**Comparison:**
- Measure same metrics in .NET 10
- Document any regressions >10%
- .NET 10 should be comparable or faster

**Critical Paths:**
- Application startup
- Main window rendering
- PLC connection establishment
- Deployment operation

### Regression Testing

**Critical Functionality:**
- [ ] All existing features still work
- [ ] No new bugs introduced
- [ ] UI behavior unchanged
- [ ] File format compatibility maintained

**Known Issues:**
- Document any known behavioral differences
- Compare against .NET Framework 4.8 version
- Validate workarounds if needed

### Test Environment

**Minimum Test Environments:**
1. **Development Machine:** Windows 10/11 with .NET 10 SDK
2. **Clean Machine:** Windows 10/11 with .NET 10 Runtime only
3. **Production-Like:** Environment matching deployment target

**Test Data:**
- Sample rack configurations
- Sample web app configurations
- Test settings files
- (Optional) Test PLC if available

### Acceptance Criteria

**Must Pass:**
- [ ] Application builds with 0 errors
- [ ] Application launches successfully
- [ ] Main window displays correctly
- [ ] Settings load and save
- [ ] File dialogs work
- [ ] No crashes in core paths

**Should Pass:**
- [ ] PLC connection works (if testable)
- [ ] Deployment operations work (if testable)
- [ ] Multi-monitor support works
- [ ] Logging works correctly
- [ ] Performance acceptable

**Nice to Have:**
- [ ] All tests pass on first try
- [ ] Zero behavioral differences
- [ ] Performance improvements

### Test Documentation

**Record:**
- Test execution date
- Tester name
- Environment details
- Test results (Pass/Fail)
- Issues found
- Screenshots if applicable

**Issue Tracking:**
- Log any failures as issues
- Categorize by severity
- Link to test case
- Track resolution

## Complexity & Effort Assessment

### Overall Complexity: Medium

**Complexity Drivers:**

1. **High API Issue Count (985)**
   - BUT: Concentrated in WPF/WinForms which are well-supported
   - Most resolve automatically with SDK conversion

2. **SDK-Style Conversion**
   - Requires careful migration of project file
   - Need to preserve build configurations, resources, XAML files

3. **Package Ecosystem Changes**
   - 11 packages to remove
   - 5 packages to update
   - Dependency graph verification needed

**Complexity by Phase:**

**Phase 0 (Prerequisites):** Low
- Simple SDK verification

**Phase 1 (Atomic Upgrade):** Medium-High
- SDK conversion is non-trivial
- Package management requires attention
- Compilation error fixing requires .NET knowledge

**Phase 2 (Validation):** Medium
- WPF application testing
- PLC connectivity verification
- UI rendering validation

### Relative Complexity Ratings

| Aspect | Complexity | Rationale |
| :--- | :---: | :--- |
| Project Conversion | Medium-High | Classic WPF to SDK-style has some manual steps |
| Package Updates | Low | Clear upgrade paths available |
| API Compatibility | Low-Medium | Most issues auto-resolve; some may need code changes |
| Testing | Medium | Desktop app with external hardware (PLC) dependencies |
| Rollback | Low | Git-based, clean revert possible |

### Per-Project Complexity

**Webserver.API.WebApplicationManager.csproj:**

| Factor | Rating | Notes |
| :--- | :---: | :--- |
| Overall | Medium | WPF application with external dependencies |
| SDK Conversion | Medium-High | Classic WPF \u2192 SDK requires care |
| Package Management | Low | Clear paths for all packages |
| API Issues | Medium | 985 issues but mostly auto-resolve |
| Code Changes | Low | Minimal actual code changes expected (~15-20 lines) |
| Testing Complexity | Medium | External PLC dependency; UI testing required |
| Risk Level | Medium | Active technology, well-supported |

### Resource Requirements

**Skills Required:**
- **Essential:**
  - .NET SDK-style project knowledge
  - WPF application development
  - NuGet package management
  - Git/source control
  
- **Helpful:**
  - Experience with .NET Framework to .NET Core/5+ migrations
  - WPF migration experience
  - Understanding of breaking changes in .NET
  - PLC communication knowledge (for testing)

**Team Capacity:**
- **Minimum:** 1 developer with .NET migration experience
- **Recommended:** 1 developer + 1 reviewer/tester
- **Parallel Work:** Not applicable (single project)

**Tools Required:**
- Visual Studio 2022 (17.8+) or VS Code with C# extension
- .NET 10 SDK
- Git client
- (Optional) .NET Upgrade Assistant
- (Optional) Test PLC for integration testing

## Source Control Strategy

### Branching Strategy

**Main Branch:** `KircMax/UpdateLibrary` (source branch)  
**Feature Branch:** `upgrade-to-NET10` (upgrade branch)  
**Target Branch:** `KircMax/UpdateLibrary` (after successful upgrade)

**Branch Structure:**
```
KircMax/UpdateLibrary (source)
  ??? upgrade-to-NET10 (working branch)
        ??? [Upgrade work happens here]
        ??? [Merge back to KircMax/UpdateLibrary when complete]
```

### Commit Strategy

**Frequency:** Commit at each significant milestone

**Recommended Commits:**

1. **Initial Commit:** "Start .NET 10 upgrade - create branch"
   - Just branch creation, no changes

2. **SDK Conversion:** "Convert project to SDK-style"
   - Changes to `.csproj` file
   - Before attempting to build

3. **Package Cleanup:** "Remove framework-included packages"
   - Remove 11 packages
   - Update `.csproj`

4. **Package Updates:** "Update packages to .NET 10 versions"
   - Update 5 packages to new versions
   - Update `.csproj`

5. **Build Fixes:** "Fix compilation errors"
   - Code changes to resolve build errors
   - May be multiple commits if many fixes

6. **Assembly.Location Fix (if needed):** "Replace Assembly.Location with AppContext.BaseDirectory"
   - Code change for file path resolution

7. **Configuration Fix (if needed):** "Add ConfigurationManager package" or "Migrate to modern configuration"
   - Configuration-related changes

8. **Final Validation:** "Validate .NET 10 upgrade - all tests pass"
   - Final commit before PR/merge
   - Includes any last adjustments

**Commit Message Format:**
```
<category>: <short description>

<detailed description>
- Bullet points for specific changes
- Reference any issues fixed
- Note any breaking changes
```

**Example:**
```
chore: Convert project to SDK-style format

Convert Webserver.API.WebApplicationManager.csproj from classic
format to SDK-style to enable .NET 10 migration.

- Replace classic .csproj with SDK-style structure
- Enable UseWPF and UseWindowsForms
- Update target framework to net10.0-windows
- Preserve build configurations and settings
```

### Checkpoint Strategy

**Major Checkpoints:**

1. **Pre-Conversion Checkpoint**
   - Tag: `pre-net10-upgrade`
   - Purpose: Last known good state before any changes
   - Command: `git tag -a pre-net10-upgrade -m "Before .NET 10 upgrade"`

2. **Post-SDK-Conversion Checkpoint**
   - Commit after SDK conversion completes
   - Purpose: Rollback point if package updates fail

3. **Post-Build Checkpoint**
   - Commit after project builds successfully
   - Purpose: Rollback point if runtime issues occur

4. **Post-Testing Checkpoint**
   - Commit after all tests pass
   - Tag: `post-net10-upgrade`
   - Purpose: Mark completion of upgrade

### Review and Merge Process

**Pull Request Requirements:**

**Title:** ".NET 10 Upgrade - Webserver.API.WebApplicationManager"

**Description Template:**
```markdown
## Summary
Upgrade WebApplicationManager from .NET Framework 4.8 to .NET 10.0 LTS.

## Changes
- ? Converted project to SDK-style
- ? Updated target framework to net10.0-windows
- ? Removed 11 framework-included packages
- ? Updated 5 packages to .NET 10 versions
- ? Fixed [X] compilation errors
- ? [Any code fixes applied]

## Testing
- [ ] Application builds successfully
- [ ] Application launches and runs
- [ ] Core functionality tested
- [ ] PLC integration tested (if applicable)
- [ ] No regressions identified

## Breaking Changes
[List any behavioral changes or known issues]

## Rollback Plan
Revert to `pre-net10-upgrade` tag if issues found in production.

## Related Issues
Fixes #[issue number] (if applicable)
```

**Review Checklist:**

**Code Review:**
- [ ] .csproj changes reviewed
- [ ] Package versions correct
- [ ] Code changes minimal and justified
- [ ] No unnecessary changes included

**Build Review:**
- [ ] Project builds on reviewer's machine
- [ ] No unexpected warnings
- [ ] Output structure correct

**Functionality Review:**
- [ ] Application runs on reviewer's machine
- [ ] Core features work
- [ ] No obvious regressions

**Documentation Review:**
- [ ] README updated (if needed)
- [ ] Version number updated
- [ ] Breaking changes documented

### Merge Criteria

**Must Meet:**
1. ? All commits follow commit message format
2. ? Project builds with 0 errors
3. ? Application launches successfully
4. ? Core functionality works
5. ? PR review approved
6. ? All checklist items completed

**Should Meet:**
1. ? All tests pass
2. ? No performance regressions
3. ? No new warnings
4. ? Documentation updated

### Single Commit Preference for All-At-Once Strategy

**Rationale:**
For a single-project All-At-Once upgrade, a **single comprehensive commit** can be appropriate because:
- All changes are interrelated (SDK, TFM, packages are one unit)
- Cannot partially upgrade a single project
- Easier to revert atomically if issues arise
- Cleaner history for single-project solution

**Single Commit Structure:**
```
feat: Upgrade to .NET 10.0 LTS

Complete upgrade of WebApplicationManager from .NET Framework 4.8
to .NET 10.0 (Long Term Support).

Changes:
- Convert project to SDK-style format
- Update target framework: net48 ? net10.0-windows
- Enable UseWPF and UseWindowsForms
- Remove 11 framework-included packages:
  - Microsoft.NETCore.Platforms, NETStandard.Library, System.Buffers,
    System.IO, System.Memory, System.Net.Http, System.Numerics.Vectors,
    System.Runtime, System.Security.Cryptography.*, 
    System.Threading.Tasks.Extensions
- Update 5 packages to .NET 10 versions:
  - Microsoft.Bcl.AsyncInterfaces 9.0.8 ? 10.0.3
  - Microsoft.Extensions.DependencyInjection.Abstractions 9.0.8 ? 10.0.3
  - Microsoft.Extensions.Logging.Abstractions 9.0.8 ? 10.0.3
  - Newtonsoft.Json 13.0.3 ? 13.0.4
  - System.Diagnostics.DiagnosticSource 9.0.8 ? 10.0.3
- [Any code fixes applied]

Testing:
- ? Builds successfully with 0 errors
- ? Application launches and runs
- ? Core functionality verified
- ? WPF UI renders correctly
- ? Windows Forms dialogs work

Breaking Changes:
[None or list any]

References:
- Assessment: .github/upgrades/assessment.md
- Plan: .github/upgrades/plan.md
```

**However, Multiple Commits Are Also Valid:**

If you prefer granular commits for easier review/debugging:
1. SDK conversion
2. Package updates
3. Code fixes
4. Final validation

Both approaches are acceptable. Choose based on team preference.

### Rollback Strategy

**If Issues Found After Merge:**

**Option 1: Revert Commit**
```bash
git revert <commit-hash>
git push origin KircMax/UpdateLibrary
```

**Option 2: Reset to Tag**
```bash
git reset --hard pre-net10-upgrade
git push origin KircMax/UpdateLibrary --force
```

**Option 3: Create Hotfix Branch**
- Keep upgrade changes
- Fix specific issues in hotfix branch
- Merge hotfix back

**Communication:**
- Notify team of rollback
- Document reason for rollback
- Plan remediation strategy

### Source Control Best Practices

**Do:**
- ? Create dedicated upgrade branch
- ? Commit at logical milestones
- ? Write descriptive commit messages
- ? Tag important checkpoints
- ? Keep commits focused
- ? Test before committing
- ? Push regularly to remote

**Don't:**
- ? Commit broken code (unless explicitly WIP)
- ? Mix unrelated changes
- ? Force push without team agreement
- ? Commit sensitive data
- ? Skip commit messages
- ? Work directly on main branch

## Success Criteria

### Technical Criteria

**All Must Be Met:**

#### Project Migration
- ? **Project converted to SDK-style**
  - Project file uses `<Project Sdk="Microsoft.NET.Sdk">` format
  - XAML files auto-included via glob patterns
  - Code files auto-included
  - Project file significantly simplified

- ? **Target framework updated**
  - Project targets `net10.0-windows`
  - `UseWPF` property set to `true`
  - `UseWindowsForms` property set to `true`
  - No references to `net48` remain

#### Package Management
- ? **Framework-included packages removed**
  - All 11 identified packages removed from project
  - No redundant package references
  - Clean package dependency graph

- ? **Packages updated to .NET 10 versions**
  - Microsoft.Bcl.AsyncInterfaces ? 10.0.3
  - Microsoft.Extensions.DependencyInjection.Abstractions ? 10.0.3
  - Microsoft.Extensions.Logging.Abstractions ? 10.0.3
  - Newtonsoft.Json ? 13.0.4
  - System.Diagnostics.DiagnosticSource ? 10.0.3

- ? **Compatible packages retained**
  - MimeMapping 3.1.0
  - Siemens.Simatic.S7.Webserver.API 3.2.27
  - System.Runtime.CompilerServices.Unsafe 6.1.2

- ? **No package conflicts**
  - `dotnet restore` succeeds without warnings
  - No version conflicts
  - No missing dependencies

#### Build Success
- ? **Zero build errors**
  - `dotnet build` exits with code 0
  - All projects compile successfully
  - All XAML compiles correctly

- ? **Zero or minimal warnings**
  - No API compatibility warnings
  - No deprecated API warnings
  - Only expected/documented warnings present

- ? **Output structure correct**
  - Executable generated
  - Dependencies copied to output
  - Resources embedded correctly
  - Manifest included

#### Code Quality
- ? **No security vulnerabilities**
  - `dotnet list package --vulnerable` shows no issues
  - All packages from trusted sources
  - No deprecated packages with security issues

- ? **Source incompatibilities resolved**
  - All ~4 source incompatibilities fixed
  - Code compiles without errors
  - Appropriate replacements used

- ? **Assembly location handling**
  - File path resolution works correctly
  - Settings directory accessible
  - README.html accessible

#### Runtime Success
- ? **Application launches**
  - No startup exceptions
  - Main window displays
  - UI renders correctly

- ? **Core functionality works**
  - Settings load and save
  - Logging infrastructure functional
  - Navigation between windows works

- ? **WPF features work**
  - All WPF controls render
  - Data binding works
  - XAML resources load
  - Events fire correctly

- ? **Windows Forms integration works**
  - File dialogs (Open/Save) display and function
  - Message boxes work
  - Screen/monitor detection works

- ? **External integrations work**
  - Siemens.Simatic.S7.Webserver.API functions (if testable)
  - JSON serialization/deserialization works
  - PLC communication works (if testable)

### Quality Criteria

**All Should Be Met:**

#### Code Quality Maintained
- ? **No unnecessary changes**
  - Only upgrade-related modifications made
  - Existing logic preserved
  - Code style consistent

- ? **Code clarity preserved**
  - Readable and maintainable
  - Comments preserved
  - Structure intact

#### Test Coverage Maintained
- ? **Existing functionality works**
  - All features from .NET Framework 4.8 version work
  - No regressions identified
  - Behavior matches previous version

- ? **Performance acceptable**
  - Application startup time comparable or better
  - UI responsiveness comparable or better
  - No significant performance regressions

#### Documentation Updated
- ? **README updated (if needed)**
  - Target framework mentioned
  - Build instructions updated
  - Prerequisites updated (.NET 10 SDK)

- ? **Version number updated**
  - Assembly version incremented
  - Package version updated if applicable

- ? **Breaking changes documented**
  - Any behavioral changes noted
  - Migration guide included if needed

### Process Criteria

**All Should Be Met:**

#### All-At-Once Strategy Followed
- ? **Atomic upgrade completed**
  - All changes made in single coordinated operation
  - No partial states
  - Clean execution

- ? **Ordering respected**
  - SDK conversion before TFM change
  - TFM change before package updates
  - Packages before compilation
  - Compilation before testing

#### Source Control Followed
- ? **Branch strategy followed**
  - Work done on `upgrade-to-NET10` branch
  - Commits at appropriate milestones
  - Clear commit messages

- ? **Checkpoints created**
  - Pre-upgrade tag created
  - Major milestones committed
  - Post-upgrade tag created

- ? **Review process followed**
  - Pull request created
  - Code review completed
  - Approval obtained

### Acceptance Sign-Off

**Final Validation:**

**Technical Lead Sign-Off:**
- [ ] Code changes reviewed and approved
- [ ] Build verification completed
- [ ] No technical concerns

**QA Sign-Off:**
- [ ] All test cases passed
- [ ] No regressions found
- [ ] Performance acceptable

**Product Owner Sign-Off:**
- [ ] Functionality meets requirements
- [ ] Ready for deployment
- [ ] Risks acceptable

### Definition of Done

**The upgrade is considered DONE when:**

1. ? **All technical criteria met** (listed above)
2. ? **All quality criteria met** (listed above)
3. ? **All process criteria met** (listed above)
4. ? **Code merged to main branch** (`KircMax/UpdateLibrary`)
5. ? **Documentation updated** (README, version numbers)
6. ? **Team notified** of completion
7. ? **Knowledge shared** (lessons learned documented)

### Success Metrics

**Quantitative:**
- Build time: [Document before/after]
- Startup time: [Document before/after]
- Memory usage: [Document before/after]
- Package count: 21 ? 10 (47.6% reduction)
- Project file size: [Document before/after reduction]

**Qualitative:**
- Code maintainability: Improved (SDK-style is cleaner)
- Future upgrade path: Improved (modern .NET ecosystem)
- Developer experience: Improved (better tooling)
- Community support: Improved (active .NET 10 ecosystem)

### Post-Upgrade Validation

**Within 1 week of merge:**
- [ ] Monitor for runtime issues
- [ ] Gather user feedback
- [ ] Document any unexpected behaviors
- [ ] Create follow-up issues if needed

**Within 1 month:**
- [ ] Confirm long-term stability
- [ ] Evaluate performance in production
- [ ] Assess developer satisfaction
- [ ] Plan for future improvements

---

## Conclusion

This plan provides a comprehensive, step-by-step approach to upgrading the WebApplicationManager solution from .NET Framework 4.8 to .NET 10.0 LTS using an All-At-Once strategy.

**Key Success Factors:**
1. Clear understanding of changes required (assessment-driven)
2. Systematic execution following defined steps
3. Comprehensive testing at multiple levels
4. Strong rollback capabilities via source control
5. Well-defined success criteria

**Next Steps:**
1. Review and approve this plan
2. Ensure .NET 10 SDK installed
3. Execute Phase 1: Atomic Upgrade
4. Execute Phase 2: Validation
5. Merge and monitor

**Timeline:**
No specific timeline provided due to the nature of the work. Complexity is rated as Medium, with most effort concentrated in SDK conversion, package management, and thorough testing.

Good luck with the upgrade! ??
