function FindMSBuild () {
    if ($null -eq $env:OS) {
        try {
            return (Get-Command msbuild).Source;
        }
        catch {
            return $null;
        }
    }

    foreach ($program in ("C:\Program Files", "D:\Program Files")) {
        foreach ($vs in (Get-ChildItem ($program + "\Microsoft Visual Studio"))) {
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
