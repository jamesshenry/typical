$assetsPath = Join-Path $PSScriptRoot 'src\Typical.DataAccess\obj\project.assets.json'
if (-not (Test-Path $assetsPath)) {
    Write-Error "Assets file not found: $assetsPath"
    exit 1
}

Write-Output "Searching raw assets file for identity-related package strings..."
Get-Content $assetsPath -Raw | Select-String -Pattern 'Azure.Identity|Microsoft.Identity.Client|System.IdentityModel.Tokens.Jwt|Microsoft.IdentityModel.JsonWebTokens|IdentityModel|Identity' | ForEach-Object {
    Write-Output $_.Line
}
