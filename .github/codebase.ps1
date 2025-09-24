$repoRoot = Resolve-Path "$PSScriptRoot/.."

$sourceDirectory = Join-Path $repoRoot 'src'

$outputFile = Join-Path $repoRoot '.github' 'instructions' 'codebase.txt'

$directoryTree = Get-ChildItem -Directory -Path $sourceDirectory -Recurse | ForEach-Object {
    $indent = '  ' * ($_.FullName.Split('\').Length - $sourceDirectory.Split('\').Length)
    "$indent- $($_.Name)"
} | Out-String

$contextBlock = "$directoryTree`n# --- Start of Code Files ---`n`n"

Set-Content -Path $outputFile -Value $contextBlock

$csharpFiles = Get-ChildItem -Path $sourceDirectory -Recurse -Include '*.cs'

foreach ($file in $csharpFiles) {
    $filePathHeader = "`n// File: $($file.FullName.Substring($PWD.Path.Length + 1))`n`n"
    $fileContent = Get-Content -Path $file.FullName | Out-String
    Add-Content -Path $outputFile -Value ($filePathHeader + $fileContent)
}

Write-Host "C# codebase with contextual messages has been concatenated into $outputFile"