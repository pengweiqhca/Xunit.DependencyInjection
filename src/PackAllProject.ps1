function FindMSBuild () {
    $cmd = Get-Command msbuild -type Application -ErrorAction SilentlyContinue | Select-Object -First 1;
    if ($null -ne $cmd) {
        return $cmd.Source;
    }

    if ($PSVersionTable.PSVersion.Major -ge 6 -and -not $IsWindows) {
        return $null;
    }

    if ([Environment]::Is64BitProcess) {
        $vsv = "$Env:ProgramFiles (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
    }
    else {
        $vsv = "$Env:ProgramFiles\Microsoft Visual Studio\Installer\vswhere.exe"
    }

    if (Test-Path $vsv) {
        $vsv = & $vsv -all -requires Microsoft.Component.MSBuild -format json | ConvertFrom-Json | Sort-Object { [version]$_.installationVersion } -Descending | Select-Object -First 1

        if (($null -ne $vsv) -and (Test-Path ($vsv.installationPath + "\MSBuild\Current\Bin\amd64\MSBuild.exe"))) {
            return $vsv.installationPath + "\MSBuild\Current\Bin\amd64\MSBuild.exe";
        }
    }

    foreach ($program in ("C:\Program Files", "D:\Program Files")) {
        foreach ($vs in (Get-ChildItem ($program + "\Microsoft Visual Studio") | Sort-Object { $_.Name.Length -gt 2 }, @{ Expression = 'Name'; Descending = $true })) {
            foreach ($vsv in (Get-ChildItem $vs.FullName)) {
                if (Test-Path ($vsv.FullName + "\MSBuild\Current\Bin\amd64\MSBuild.exe")) {
                    return $vsv.FullName + "\MSBuild\Current\Bin\amd64\MSBuild.exe";
                }
            }
        }
    }

    return $null;
}

if (-not(Test-Path .packages)) {
    mkdir .packages
}

foreach ($csproj in (Get-ChildItem -r -filter *.csproj)) {
    $dir = "$([System.IO.Path]::GetDirectoryName($csproj.FullName))\bin\Release";
    if (Test-Path $dir) {
        Remove-Item -Recurse -Force $dir;
    }
}

$MSBuild = FindMSBuild
if ($null -eq $MSBuild) {
    dotnet build -c Release /p:IsPacking=true ..
}
else {
    Write-Host $MSBuild
    & "$MSBuild" /r /m /v:m /p:Configuration=Release /p:IsPacking=true ..
}

foreach ($csproj in (Get-ChildItem -r -filter *.csproj)) {
    $dir = "$([System.IO.Path]::GetDirectoryName($csproj.FullName))\bin\Release";

    if (Test-Path $dir) {
        $nupkg = Get-ChildItem "$([System.IO.Path]::GetDirectoryName($csproj.FullName))\bin\Release" |
        Where-Object { $_.Name.Endswith(".nupkg") } |
        Sort-Object -Property LastWriteTime -Descending |
        Select-Object -First 1;

        if ($null -ne $nupkg) {
            Copy-Item $nupkg.VersionInfo.FIleName (".packages\" + $nupkg.Name) -Force
        }
    }
}
