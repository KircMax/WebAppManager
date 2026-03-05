# System Requirements - WebApplicationManager v2.0

**WebApplicationManager v2.0** requires **.NET 10 Desktop Runtime**.

## Quick Start

1. **Download .NET 10 Desktop Runtime**
   - https://dotnet.microsoft.com/download/dotnet/10.0
   - Choose "Desktop Runtime" (not ASP.NET Core)

2. **Install**
   - Run the installer
   - Follow the wizard

3. **Verify**
   ```powershell
   dotnet --version
   ```
   Should show `10.0.x` or higher

## System Requirements

For detailed .NET 10 system requirements, see:
- **[.NET 10 System Requirements](https://learn.microsoft.com/dotnet/core/install/windows)**
- **[Supported Operating Systems](https://github.com/dotnet/core/blob/main/release-notes/10.0/README.md)**

**TL;DR:** Windows 7 SP1 or later, 200 MB disk space, .NET 10 Desktop Runtime

## Support

- Verify .NET: `dotnet --version`
- Check Event Viewer for errors
- See MIGRATION_GUIDE.md if upgrading from v1.0