# No longer in use.  Git workflows is being used to publish to nuget.
param
(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    [Parameter(Mandatory=$true)]
    [string]$NuGetApiKey,
    [switch]$UseWindowsDX
)

$WindowsDX = ""

if ($UseWindowsDX -eq $true)
{
    $WindowsDX = ".WindowsDX"
}

dotnet build .\InputHandlers.sln /p:Configuration=Release
nuget pack .\InputHandlers\ -Properties Configuration=Release
nuget push ".\InputHandlers.MonoGame$($WindowsDX).$($Version).nupkg" -Source https://nuget.org -ApiKey $NuGetApiKey
