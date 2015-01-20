@ECHO OFF
pushd %~dp0

nuget pack Paradox.Console.debug.nuspec -symbols
nuget pack Paradox.Console.PythonInterpreter.debug.nuspec -symbols
nuget push Paradox.Console.0.0.5-alpha.nupkg
nuget push Paradox.Console.PythonInterpreter.0.0.5-alpha.nupkg

PAUSE
popd