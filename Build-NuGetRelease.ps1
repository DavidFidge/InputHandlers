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

msbuild .\InputHandlers.sln /p:Configuration=Release
nuget pack .\InputHandlers\
nuget push ".\InputHandlers.MonoGame$($WindowsDX).$($Version).nupkg" -Source https://nuget.org -ApiKey $NuGetApiKey
