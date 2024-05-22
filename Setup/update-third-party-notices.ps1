Push-Location "$PSScriptRoot\.."
dotnet-thirdpartynotices (Get-Location).Path --output-filename "THIRD-PARTY-NOTICES.txt"
Pop-Location