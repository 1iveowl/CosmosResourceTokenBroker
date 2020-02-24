param([string]$betaver)

if ([string]::IsNullOrEmpty($betaver)) 
{
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\..\main\B2CAuthClient\bin\Release\netstandard2.1\B2CAuthClient.dll')).Version.ToString(3)
}
else 
{
		$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\..\main\B2CAuthClient\bin\Release\netstandard2.1\B2CAuthClient.dll')).Version.ToString(3) + "-" + $betaver
}

.\build.ps1 $version

c:\tools\nuget\Nuget.exe push ".\NuGet\B2CAuth.Xamarin.$version.symbols.nupkg" -Source https://www.nuget.org