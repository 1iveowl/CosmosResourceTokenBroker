param([string]$betaver)

if ([string]::IsNullOrEmpty($betaver)) 
{
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\..\main\CosmosResourceTokenClient\bin\Release\netstandard2.1\CosmosResourceTokenClient.dll')).Version.ToString(3)
}
else 
{
		$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\..\main\CosmosResourceTokenClient\bin\Release\netstandard2.1\CosmosResourceTokenClient.dll')).Version.ToString(3) + "-" + $betaver
}

.\build.ps1 $version

c:\tools\nuget\Nuget.exe push ".\NuGet\ResourceTokenClient.Cosmos.$version.symbols.nupkg" -Source https://www.nuget.org