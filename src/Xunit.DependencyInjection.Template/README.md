# Xunit.DependencyInjection.Template

## Intro

used for create a xunit test project with Xunit.DependencyInjection

## Package

``` bash
dotnet pack -o out
```

publish the nupkg file to nuget for release

## Install

``` bash
dotnet new install Xunit.DependencyInjection.Template
```

## Use

> Create test project within folder:

``` bash
dotnet new create xunit-di
```

> Create test project with Specific TargetFramework:

By default, we create test project targeted at `net8.0`, you can change the target framework via `-f <targetFrameworkName>` or `--framework <targetFrameworkName>`

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
dotnet pack Xunit.DependencyInjection.Template.csproj

# install
dotnet new install Xunit.DependencyInjection.Template.1.1.0.nupkg

# testing
dotnet new create xunit-di -n TestProject

# uninstall
dotnet new uninstall Xunit.DependencyInjection.Template
```
