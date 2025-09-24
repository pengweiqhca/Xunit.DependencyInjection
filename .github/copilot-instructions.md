# Xunit.DependencyInjection

Xunit.DependencyInjection is a .NET library that enables Microsoft.Extensions.DependencyInjection to resolve xUnit test cases. It provides dependency injection for xUnit tests with support for multiple target frameworks and integrates with ASP.NET Core TestHost.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### CRITICAL REQUIREMENTS
**STOP**: Do not proceed without .NET 10.x preview SDK. All build/test commands will fail otherwise.

### Environment Setup
- **CRITICAL**: Install .NET 10.x preview SDK. This project CANNOT be built without .NET 10.x preview.
  ```bash
  # Check current version
  dotnet --version
  # Must show 10.x.x preview version for building to work
  
  # If you see 8.x.x or 9.x.x, all builds will fail with:
  # "error NU1011: Centrally defined floating package versions are not allowed"
  ```
- The project multi-targets .NET Framework 4.7.2, .NET 8.0, and .NET 9.0 but requires .NET 10.x SDK to build
- Uses Central Package Management with floating versions (requires .NET 10.x preview)
- **DO NOT ATTEMPT TO BUILD** without .NET 10.x preview - it will fail consistently

### Build Process
- **NEVER CANCEL builds or test runs** - they can take significant time and must complete
- Build the entire solution:
  ```bash
  dotnet build -c Release -v n
  ```
  **TIMING**: Build takes approximately 5-15 minutes. NEVER CANCEL. Set timeout to 30+ minutes.
- Individual project builds:
  ```bash
  dotnet build src/Xunit.DependencyInjection/Xunit.DependencyInjection.csproj -c Release -v n
  ```
- **IMPORTANT**: If floating package versions cause build failures, the issue is likely .NET SDK version mismatch. Ensure .NET 10.x preview is installed.

### Test Execution
- Run core tests:
  ```bash
  dotnet test -c Release --no-build ./test/Xunit.DependencyInjection.Test/Xunit.DependencyInjection.Test.csproj
  ```
  **TIMING**: Test execution takes 3-10 minutes. NEVER CANCEL. Set timeout to 20+ minutes.
- Run ASP.NET Core tests:
  ```bash
  dotnet test -c Release --no-build ./test/Xunit.DependencyInjection.Test.AspNetCore/Xunit.DependencyInjection.Test.AspNetCore.csproj
  ```
- Run parallelization tests:
  ```bash
  dotnet test -c Release --no-build ./test/Xunit.DependencyInjection.Test.Parallelization/Xunit.DependencyInjection.Test.Parallelization.csproj
  ```
- Run F# tests:
  ```bash
  dotnet test -c Release --no-build ./test/Xunit.DependencyInjection.Test.FSharp/Xunit.DependencyInjection.Test.FSharp.fsproj
  ```
- **CRITICAL**: Always use `--no-build` when running tests after a successful build to save time.

### Packaging
- Package all projects:
  ```bash
  # Windows with Visual Studio
  powershell -ExecutionPolicy Bypass -File src/PackAllProject.ps1
  
  # Alternative: dotnet pack
  dotnet pack -c Release -o .packages
  ```
- Packages are created in `.packages/` directory

## Validation Scenarios

### Basic Test Scenario
After making changes, always validate with this complete scenario:
1. Create a simple test project using the template:
   ```bash
   dotnet new install src/Xunit.DependencyInjection.Template/bin/Release/Xunit.DependencyInjection.Template.*.nupkg
   mkdir TestValidation
   cd TestValidation
   dotnet new create xunit-di -n SampleTest
   cd SampleTest
   dotnet test
   ```
2. Verify the test project runs successfully with dependency injection
3. Clean up: `dotnet new uninstall Xunit.DependencyInjection.Template`

### ASP.NET Core Integration Test
1. Navigate to the MinimalAPI sample: `cd test/MinimalApiSample`
2. The sample demonstrates integration with ASP.NET Core TestHost
3. Verify tests in `Xunit.DependencyInjection.Test.AspNetCore` can access the sample via HTTP client injection

### Parallelization Test
1. Run parallelization tests to ensure threading works correctly:
   ```bash
   dotnet test ./test/Xunit.DependencyInjection.Test.Parallelization/Xunit.DependencyInjection.Test.Parallelization.csproj
   ```
2. Verify both sequential and parallel test execution modes work properly

## Key Project Structure

### Source Projects (`src/`)
- `Xunit.DependencyInjection/` - Main library (multi-targets net472;net8.0)
- `Xunit.DependencyInjection.Analyzer/` - Roslyn analyzer for compile-time validation
- `Xunit.DependencyInjection.AspNetCoreTesting/` - ASP.NET Core TestHost integration
- `Xunit.DependencyInjection.Logging/` - Microsoft.Extensions.Logging integration
- `Xunit.DependencyInjection.Template/` - dotnet new template for creating test projects
- `Xunit.DependencyInjection.StaFact/` - STA thread support for UI tests
- `Xunit.DependencyInjection.Demystifier/` - Enhanced stack trace support
- `Xunit.DependencyInjection.xRetry/` - Test retry functionality

### Test Projects (`test/`)
- `Xunit.DependencyInjection.Test/` - Core functionality tests (dependency injection, startup patterns)
- `Xunit.DependencyInjection.Test.AspNetCore/` - ASP.NET Core TestHost integration tests  
- `Xunit.DependencyInjection.Test.Parallelization/` - Parallel execution validation tests
- `Xunit.DependencyInjection.Test.DisableTestParallelization/` - Sequential execution validation tests
- `Xunit.DependencyInjection.Test.FSharp/` - F# language compatibility tests
- `Xunit.DependencyInjection.Analyzer.Test/` - Roslyn analyzer validation tests
- `MinimalApiSample/` - Sample ASP.NET Core application used by integration tests

### Important Files
- `Directory.Build.props` - Global MSBuild properties
- `Directory.Packages.props` - Central package management (uses floating versions)
- `global.json` - .NET SDK version specification
- `Xunit.DependencyInjection.slnx` - Solution file (newer format)

## Common Development Tasks

### Template Development
1. Navigate to template: `cd src/Xunit.DependencyInjection.Template/`
2. Package template: `dotnet pack -o out`
3. Install locally: `dotnet new install ./out/Xunit.DependencyInjection.Template.*.nupkg`
4. Test template: `dotnet new create xunit-di -n TestProject`
5. Uninstall: `dotnet new uninstall Xunit.DependencyInjection.Template`

### Running Sample Applications
- MinimalAPI sample is not directly runnable as a web app - it's designed for testing via TestHost
- Test projects demonstrate various usage patterns - examine test classes for implementation examples

### Code Analysis
- The project includes a Roslyn analyzer that validates Startup class configurations
- Analyzer tests are in `test/Xunit.DependencyInjection.Analyzer.Test/`
- Always run analyzer tests when modifying analyzer rules

## Troubleshooting

### Build Issues
- **"Centrally defined floating package versions are not allowed"**: The most common error. Upgrade to .NET 10.x preview SDK immediately.
- **"CS0106: The modifier 'public' is not valid for this item"**: Language version issue - this project uses C# 14 features that require .NET 10.x preview SDK
- **Cannot restore packages**: Verify internet connectivity and NuGet package sources
- **Projects fail to build on .NET 8.x/9.x**: Expected behavior. This project is designed for .NET 10.x preview only.

### Test Issues
- **Tests hang or timeout**: Allow sufficient time - some tests verify timing behavior and intentionally delay
- **Parallelization tests fail**: May indicate threading issues - check test output for timing information
- **ASP.NET Core tests fail**: Verify MinimalApiSample builds correctly as a dependency

### Template Issues
- **"Template not found"**: Ensure template is packaged and installed correctly
- **Template creates project with wrong framework**: Use `-f` parameter to specify target framework

## Common Commands Output Reference

### Repository Structure
```
ls -la /
.editorconfig
.git/
.github/
.gitignore
azure-pipelines.yml
Directory.Build.props
Directory.Packages.props
global.json
README.md
src/
test/
Xunit.DependencyInjection.slnx
```

### Key Configuration Files
```
cat global.json
{
  "sdk": {
    "version": "10.0",
    "rollForward": "latestMajor", 
    "allowPrerelease": true
  }
}
```

### Source Project List  
```
find src/ -name "*.csproj" -o -name "*.fsproj"
src/Xunit.DependencyInjection/Xunit.DependencyInjection.csproj
src/Xunit.DependencyInjection.Analyzer/Xunit.DependencyInjection.Analyzer.csproj
src/Xunit.DependencyInjection.AspNetCoreTesting/Xunit.DependencyInjection.AspNetCoreTesting.csproj
src/Xunit.DependencyInjection.Demystifier/Xunit.DependencyInjection.Demystifier.csproj
src/Xunit.DependencyInjection.Logging/Xunit.DependencyInjection.Logging.csproj
src/Xunit.DependencyInjection.StaFact/Xunit.DependencyInjection.StaFact.csproj
src/Xunit.DependencyInjection.Template/Xunit.DependencyInjection.Template.csproj
src/Xunit.DependencyInjection.xRetry/Xunit.DependencyInjection.xRetry.csproj
```

## Development Guidelines

### When Modifying Code
1. Always build the entire solution first to establish baseline
2. Run all relevant test suites for the area you're changing
3. Validate template functionality if changes affect project generation
4. Test both .NET Framework 4.7.2 and .NET 8.0 targets where applicable

### CI/CD Validation
- GitHub Actions and Azure Pipelines both expect .NET 10.x preview
- All test projects must pass on Windows, Linux, and macOS
- Package generation must succeed for all projects

### Performance Considerations
- Dependency injection setup happens once per test class
- Parallelization can be controlled via `ParallelizationMode` property
- Use `[DisableParallelization]` attribute for tests that require sequential execution