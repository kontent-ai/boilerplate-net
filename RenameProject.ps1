<#

.SYNOPSIS
This is a simple Powershell script to ease change of the .NET Project name

.DESCRIPTION
Changes project name to the new one by renaming all files, folders and text occurences of the default project name.

.EXAMPLE
./RenameProject.ps1 "MyNewWebSite"

#>
Param(
    [Parameter(Mandatory=$true, HelpMessage=”You must provide a project name”)]
    [String]$ProjectName
 )

 $oldProjectName = "CloudBoilerplateNet"

# Replace occurences within a files
$files = Get-ChildItem . -Recurse -File
foreach ($file in $files)
{
    (Get-Content $file.PSPath) | 
        Foreach-Object { $_ -replace $oldProjectName, $NewProjectName } | 
            Set-Content $file.PSPath
}

# Rename files & directories
Get-ChildItem . -Recurse -Filter *$oldProjectName* | 
    Rename-Item -NewName { $_.Name -replace $oldProjectName, $NewProjectName }
