Push-Location "$PSScriptRoot\.."

try {
	dotnet restore
	dotnet build --configuration Release --no-restore
	dotnet test --configuration Release --no-build
}
finally {
	Pop-Location
}
