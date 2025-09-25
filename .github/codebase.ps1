$repoRoot = Resolve-Path "$PSScriptRoot/.."

Write-Host $repoRoot

$sourceDirectory = Join-Path $repoRoot 'src'

Write-Host $sourceDirectory

$outputFile = "$repoRoot/.github/instructions/codebase.txt"

Write-Host $outputFile

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