param([string]$betaver)

if ([string]::IsNullOrEmpty($betaver)) 
{
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\..\main\CosmosResourceTokenBroker\bin\Release\netstandard2.1\CosmosResourceTokenBroker.dll')).Version.ToString(3)
}
else 
{
		$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\..\main\CosmosResourceTokenBroker\bin\Release\netstandard2.1\CosmosResourceTokenBroker.dll')).Version.ToString(3) + "-" + $betaver
}

.\build.ps1 $version

c:\tools\nuget\Nuget.exe push ".\NuGet\ResourceTokenBroker.Cosmos.$version.symbols.nupkg" -Source https://www.nuget.org