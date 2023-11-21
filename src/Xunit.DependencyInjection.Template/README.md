# Xunit.DependencyInjection.Template

## Intro

Used to create a xunit test project with `Xunit.DependencyInjection`

## Install

``` bash
dotnet new install Xunit.DependencyInjection.Template
```

## Use

> Create a test project within a folder:

``` bash
dotnet new create xunit-di
```

> Create a test project with Specific TargetFramework:

By default, we create the test project targeted at `net8.0`, you can change the target framework via `-f <targetFrameworkName>` or `--framework <targetFrameworkName>`

``` bash
dotnet new create xunit-di -f net9.0
```

> Create test project include folder:

``` bash
dotnet new create xunit-di -n <TestProjectName>
```

## Develop

dotnet templating Wiki: <https://github.com/dotnet/templating/wiki>

``` bash
# package
dotnet pack Xunit.DependencyInjection.Template.csproj -o out

# install
dotnet new install ./out/Xunit.DependencyInjection.Template.1.2.0.nupkg

# testing
dotnet new create xunit-di -n TestProject

# uninstall
dotnet new uninstall Xunit.DependencyInjection.Template
```

## Package

``` bash
dotnet pack -o out
```

publish the nupkg file to nuget for release
