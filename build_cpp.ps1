# build_cpp.ps1
$vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path $vsWhere) {
    $msBuildPath = & $vsWhere -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
    if ($msBuildPath) {
        Write-Host "Found MSBuild: $msBuildPath" -ForegroundColor Green
        & $msBuildPath SortingCore\SortingCore.vcxproj /p:Configuration=Debug /p:Platform=x64
        & $msBuildPath SortingCore\SortingCore.vcxproj /p:Configuration=Release /p:Platform=x64
        Write-Host "Compiled successfully with MSBuild!" -ForegroundColor Green
        exit
    }
}

$gppPath = "g++"
if (!(Get-Command g++ -ErrorAction SilentlyContinue)) {
    $wingetGpp = Resolve-Path "$env:LocalAppData\Microsoft\WinGet\Packages\MartinStorsjo.LLVM-MinGW.UCRT_*\*\bin\g++.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Path
    if ($wingetGpp) {
        $gppPath = $wingetGpp
    }
}

if (Get-Command $gppPath -ErrorAction SilentlyContinue) {
    Write-Host "MSBuild (Visual Studio) not found, but g++ is available at '$gppPath'. Compiling with GCC..." -ForegroundColor Green
    New-Item -ItemType Directory -Force -Path "SortingRunner/bin/Debug/net10.0" | Out-Null
    New-Item -ItemType Directory -Force -Path "SortingRunner/bin/Release/net10.0" | Out-Null
    
    & $gppPath -m64 -shared -o SortingRunner/bin/Debug/net10.0/SortingCore.dll SortingCore/sorting_c.c SortingCore/sorting_cpp.cpp
    & $gppPath -m64 -O3 -shared -o SortingRunner/bin/Release/net10.0/SortingCore.dll SortingCore/sorting_c.c SortingCore/sorting_cpp.cpp
    
    Write-Host "Compiled successfully with g++!" -ForegroundColor Green
} else {
    Write-Error "Could not find MSBuild (Visual Studio) or g++ (MinGW). Please install a C++ compiler."
}
