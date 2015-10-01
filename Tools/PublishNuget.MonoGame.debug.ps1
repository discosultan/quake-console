# Define variables.
$consoleId = 'QuakeConsole.MonoGame.WindowsDX'
$pythonInterpreterId = 'QuakeConsole.PythonInterpreter.MonoGame.WindowsDX'
$manualInterpreterId = 'QuakeConsole.ManualInterpreter.MonoGame.WindowsDX'

$nuspecSuffix = '.debug.nuspec'
$nupkgSuffix = '-alpha.nupkg'

$versionRegex = '(?<=(' + $consoleId + '" version\="|\<version\>))\d+.\d+.\d+'
$newVersion = Read-Host 'What is the new version?'

# Define functions.
Function UpdateVersionInFile($fileName)
{
	(Get-Content $fileName) | 
	Foreach-Object {$_ -replace $versionRegex, $newVersion} |
	Out-File $fileName
}
Function GetNuspecFilename($id)
{
	return $id + $nuspecSuffix
}
Function GetNupkgFilename($id)
{
	return $id + '.' + $newVersion + $nupkgSuffix
}

# Replace version numbers in nuspec files.
Write-Host Replacing version numbers
UpdateVersionInFile (GetNuspecFilename $consoleId) $versionRegex $newVersion
UpdateVersionInFile (GetNuspecFilename $pythonInterpreterId) $versionRegex $newVersion
UpdateVersionInFile (GetNuspecFilename $manualInterpreterId) $versionRegex $newVersion

# Create nuget packages with symbol packages.
Write-Host Creating packages
nuget pack (GetNuspecFilename $consoleId) -symbols
nuget pack (GetNuspecFilename $pythonInterpreterId) -symbols
nuget pack (GetNuspecFilename $manualInterpreterId) -symbols

# Publish nuget packages to nuget.org and symbol packages to symbolsource.org
Write-Host Publishing packages
nuget push (GetNupkgFilename $consoleId)
nuget push (GetNupkgFilename $pythonInterpreterId)
nuget push (GetNupkgFilename $manualInterpreterId)