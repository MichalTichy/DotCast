$projectpath = $PSScriptRoot

Write-Host "Deleting obj folders in " + $projectpath -ForegroundColor Green
Get-ChildItem -Directory -Recurse -Path $projectpath |  where {$_.Name -eq "obj" -and $_.FullName -notmatch "node_modules" } | Get-ChildItem | Remove-Item -Recurse -Force
    
Write-Host "Deleting bin folders in " + $projectpath -ForegroundColor Green
Get-ChildItem -Directory -Recurse -Path $projectpath |  where {$_.Name -eq "bin" -and $_.FullName -notmatch "node_modules" } | Get-ChildItem | Remove-Item -Recurse -Force


Write-Host "Deleting packages in " + $projectpath -ForegroundColor Green
Get-ChildItem -Directory -Recurse -Path $projectpath |  where {$_.Name -eq "packages" -and $_.FullName -notmatch "node_modules" } | Get-ChildItem | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Sucessfull" -ForegroundColor Green

Write-Host "Press any key to continue bro..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

    
