# Define variables.
$consoleId = 'Paradox.Console'
$pythonInterpreterId = 'Paradox.Console.PythonInterpreter'
$customInterpreterId = 'Paradox.Console.CustomInterpreter'

$nuspecSuffix = '.nuspec'
$nupkgSuffix = '.nupkg'

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
UpdateVersionInFile (GetNuspecFilename $consoleId) $versionRegex $newVersion
UpdateVersionInFile (GetNuspecFilename $pythonInterpreterId) $versionRegex $newVersion
UpdateVersionInFile (GetNuspecFilename $customInterpreterId) $versionRegex $newVersion

# Create nuget packages with symbol packages.
nuget pack (GetNuspecFilename $consoleId) -symbols
nuget pack (GetNuspecFilename $pythonInterpreterId) -symbols
nuget pack (GetNuspecFilename $customInterpreterId) -symbols

# Publish nuget packages to nuget.org and symbol packages to symbolsource.org
nuget push (GetNupkgFilename $consoleId)
nuget push (GetNupkgFilename $pythonInterpreterId)
nuget push (GetNupkgFilename $customInterpreterId)