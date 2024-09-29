git submodule update --init

# Function to list all .csproj files
function Get-ProjectFiles {
    $path = ".\infrastructure"
    return Get-ChildItem -Path $path -Recurse -Filter *.csproj
}

# Function to read project dependencies
function Get-ProjectDependencies {
    param ([string]$projectFile)
    [xml]$content = Get-Content $projectFile
    return $content.Project.ItemGroup.ProjectReference.Include
}

# Main script execution starts here
$projects = Get-ProjectFiles
$projects | ForEach-Object { Write-Output "$($_.FullName)" }

$selectedProject = Read-Host "Please select a project file from the above list"
$slnName = Read-Host "Enter the solution name"

# Create solution if it doesn't exist
$slnPath = ".\$slnName.sln"
if (-Not (Test-Path $slnPath)) {
    dotnet new sln -n $slnName
}

# Add selected project to the solution
dotnet sln $slnPath add $selectedProject -s "3. Shared Infrastructure"

# Get and add dependencies recursively
function Add-Dependencies {
    param ([string]$projectFile, [string]$slnPath)
    $dependencies = Get-ProjectDependencies -projectFile $projectFile
    foreach ($dep in $dependencies) {
        $depPath = Join-Path (Split-Path $projectFile -Parent) $dep
        dotnet sln $slnPath add $depPath -s "3. Shared Infrastructure"
        Add-Dependencies -projectFile $depPath -slnPath $slnPath
    }
}

# Recursively add all dependencies of the selected project
Add-Dependencies -projectFile $selectedProject -slnPath $slnPath
