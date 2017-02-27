#tool "nuget:?package=xunit.runner.console"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
/*Task("Default")
    .IsDependentOn("xUnit");
    
Task("Build")
    .Does(() => {
        MSBuild("./src/Example.sln");
    });

Task("xUnit")
    .IsDependentOn("Build")
    .Does(() => {
        XUnit2("./src/Example.Tests/bin/Debug/Example.Tests.dll");
    });

*/

Task("Default")
    .IsDependentOn("Test");

Task("Build")  
.Does(() =>{

    // MA - Build the libraries
    DotNetCoreBuild("./API/**/project.json");

    DotNetCoreBuild("./Models/**/project.json");

    DotNetCoreBuild("./Services/**/project.json");

    // MA - Build the test libraries
    DotNetCoreBuild("./Tests/**/project.json");
});
/*Task("xUnit")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetCoreTest("./Tests/project.json");
    });
*/
Task("Test") 
.IsDependentOn("Build")
//.WithCriteria(() => HasArgument("test"))
.Does(() =>
{
    var tests = GetFiles("./Tests/project.json");
    foreach (var test in tests) 
    {
        
        string projectFolder = System.IO.Path.GetDirectoryName(test.FullPath);
        string projectName = projectFolder.Substring(projectFolder.LastIndexOf('\\') + 1);
        string resultsFile = "./test-results/" + projectName + ".xml";

        DotNetCoreTest(test.FullPath, new DotNetCoreTestSettings
        {
            ArgumentCustomization = args => args.Append("-xml " + resultsFile)
        });
        // MA - Transform the result XML into NUnit-compatible XML for the build server.
        XmlTransform("./tools/NUnitXml.xslt", "./test-results/" + projectName + ".xml", "./test-results/NUnit." + projectName + ".xml");
    }
});
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
