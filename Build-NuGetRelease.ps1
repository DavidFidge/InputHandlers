param
(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    [Parameter(Mandatory=$true)]
    [string]$NuGetApiKey
)

msbuild .\InputHandlers.sln /p:Configuration=Release
nuget pack .\InputHandlers\
nuget push ".\InputHandlers.MonoGame.$($version).nupkg" -Source https://nuget.org -ApiKey $nuGetApiKey
