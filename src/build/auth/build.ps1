
param([string]$version)

if ([string]::IsNullOrEmpty($version)) {$version = "0.0.1"}

$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
&$msbuild ..\..\main\B2CAuthClient\B2CAuthClient.csproj /t:Build /p:Configuration="Release"

&$msbuild ..\..\core\B2CAuthClient.Abstract\B2CAuthClient.Abstract.csproj /t:Build /p:Configuration="Release"

Remove-Item .\NuGet -Force -Recurse
New-Item -ItemType Directory -Force -Path .\NuGet
NuGet.exe pack B2CAuth.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version