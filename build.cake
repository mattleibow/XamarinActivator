#tool nuget:?package=ILRepack
#tool nuget:?package=Obfuscar

#addin nuget:?package=Cake.FileHelpers

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

if (!DirectoryExists("./output")) {
    CreateDirectory("./output");
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    var dirs = new [] { 
        "./output",
        "./XamarinActivator/packages",
        "./XamarinActivator/*/bin", 
        "./XamarinActivator/*/obj", 
    };
    foreach (var dir in dirs) {
        CleanDirectories(dir);
    }
});

Task("Build")
    .Does(() =>
{
    var solution = "XamarinActivator/XamarinActivator.sln";
    
    NuGetRestore(solution);
    DotNetBuild(solution);
    
    CopyFiles("XamarinActivator/XamarinActivatorRunner/bin/" + configuration + "/*", "./output");
});

Task("Merge")
    .IsDependentOn("Build")
    .Does(() =>
{
    if (!DirectoryExists("./output/merged")) {
        CreateDirectory("./output/merged");
    }
    
    var tool = "./tools/ILRepack/tools/ILRepack.exe";
    var args = 
        "/keyfile:XamarinActivator/XamarinActivator.snk " + 
        "/out:output/merged/XamarinActivator.exe " + 
        "output/XamarinActivatorRunner.exe " + 
        "output/XamarinActivator.dll " + 
        "output/Mono.Options.dll " + 
        "output/Newtonsoft.Json.dll ";
    StartProcess(tool, args);
});

Task("Obfuscate")
    .IsDependentOn("Merge")
    .Does(() =>
{
    if (!DirectoryExists("./output/obfuscated")) {
        CreateDirectory("./output/obfuscated");
    }
    
    var tool = "./tools/Obfuscar/tools/Obfuscar.Console.exe";
    var args = "XamarinActivator/obfuscar.xml ";
    StartProcess(tool, args);
});

Task("Package")
    .IsDependentOn("Obfuscate")
    .Does(() =>
{
    NuGetPack("./NuGet/XamarinActivator.nuspec", new NuGetPackSettings {
        OutputDirectory = "./output",
        BasePath = IsRunningOnUnix() ? "././" : "./",
        Verbosity = NuGetVerbosity.Detailed
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Merge")
    .IsDependentOn("Obfuscate")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
