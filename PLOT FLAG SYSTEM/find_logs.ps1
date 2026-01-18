Get-ChildItem -Path "C:\Users\Bora\AppData\LocalLow" -Recurse -Filter "Player*.log" -ErrorAction SilentlyContinue | Select-Object -First 5 | ForEach-Object { Write-Host $_.FullName }
