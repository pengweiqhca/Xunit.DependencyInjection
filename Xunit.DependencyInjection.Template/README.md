# Xunit.DependencyInjection.Template

## Intro

used for create a xunit test project with Xunit.DependencyInjection

## Package

``` bash
nuget pack Xunit.DependencyInjection.Template.nuspec
```

publish the nupkg file to nuget for release

## Install

``` bash
dotnet new -i Xunit.DependencyInjection.Template
```

## Use

> Create test project within folder:

``` bash
dotnet new xunit-di
```

> Create test project with Specific TargetFramework:

By default, we create test project targeted at `net6.0`, you can change the target framework via `-f <targetFrameworkName>` or `--framework <targetFrameworkName>`

``` bash
dotnet new xunit-di -f net5.0
```

> Create test project include folder:

``` bash
dotnet new xunit-di -n <TestProjectName>
```

## Develop

dotnet templating Wiki: <https://github.com/dotnet/templating/wiki>

``` bash
# package
nuget pack Xunit.DependencyInjection.Template.nuspec

# install
dotnet new -i Xunit.DependencyInjection.Template.1.1.0.nupkg

# testing
dotnet new xunit-di -n TestProject

# uninstall
dotnet new -u Xunit.DependencyInjection.Template
```
