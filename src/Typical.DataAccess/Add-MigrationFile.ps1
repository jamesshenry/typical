$dateString = Get-Date -Format 'yyyyMMddHHmm' 

$file = $dateString + '_description.sql'

new-item "$PSScriptRoot/Migrations/$file"
