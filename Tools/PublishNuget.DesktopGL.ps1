# Define variables.
$consoleId = 'MonoGame.QuakeConsole.DesktopGL'
$pythonInterpreterId = 'MonoGame.QuakeConsole.PythonInterpreter.DesktopGL'

$versionNumber = Read-Host 'What is the new version?'
$versionRegex = '(?<=(' + $consoleId + '" version\="|\<version\>))\d+.\d+.\d+'

# Working dir needs to be passed down to child jobs.
$workingDir = Get-Location

$createAndPublishPackage =
{
	param ([string]$packageId, [string]$versionRegex, [string]$versionNumber, [string]$workingDir, [bool]$prerelease)
	
	$outputDir = 'packages\'
	$nuspecSuffix = '.nuspec'    
    $nupkgSuffix = If ($prerelease) { '-alpha.nupkg' } Else { '.nupkg' }	
	
	$nuspecFile = $packageId + $nuspecSuffix;
	$nupkgFile = $outputDir + $packageId + '.' + $versionNumber + $nupkgSuffix
	
	# Set working dir, since child job's working dir is not inherited from the caller.
	Set-Location $workingDir	
	
	# Ensure output dir exists.
	md -Force $outputDir
	
	Write-Host Setting $nuspecFile version number to $versionNumber	
	(Get-Content $nuspecFile) | Foreach-Object {$_ -replace $versionRegex, $versionNumber} | Out-File $nuspecFile
	Write-Host Packing $nuspecFile 
	nuget pack $nuspecFile -OutputDirectory $outputDir -symbols	
	Write-Host Publishing $nupkgFile
	nuget push $nupkgFile
}

Start-Job -ScriptBlock $createAndPublishPackage -ArgumentList $consoleId, $versionRegex, $versionNumber, $workingDir, $True
Start-Job -ScriptBlock $createAndPublishPackage -ArgumentList $pythonInterpreterId, $versionRegex, $versionNumber, $workingDir, $True

# Wait for all jobs to complete and results ready to be received
Wait-Job * | Out-Null

# Process the results
foreach($job in Get-Job)
{    
	$result = Receive-Job $job
    Write-Host $result
}