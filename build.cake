#addin "nuget:?package=Cake.Git&version=0.16.1"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var configuration = Argument("configuration", "Debug");
var revision = EnvironmentVariable("BUILD_NUMBER") ?? Argument("revision", "9999");
var target = Argument("target", "Default");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define git commit id
var commitId = "SNAPSHOT";

// Define product name and version
var product = "Htc.Vita.Wix.Extension";
var companyName = "HTC";
var version = "0.9.0";
var semanticVersion = string.Format("{0}.{1}", version, revision);
var ciVersion = string.Format("{0}.{1}", version, "0");
var nugetTags = new [] {"htc", "vita", "wix", "extension"};

// Define copyright
var copyright = string.Format("Copyright © 2017 - {0}", DateTime.Now.Year);

// Define timestamp for signing
var lastSignTimestamp = DateTime.Now;
var signIntervalInMilli = 1000 * 5;

// Define path
var solutionFile = File(string.Format("./source/{0}.sln", product));
var nuspecFile = File(string.Format("./source/{0}.nuspec", product));
var customActionProjectFile = File("./source/Htc.Vita.Wix.CustomAction/Htc.Vita.Wix.CustomAction.net45.csproj");
var customLibraryProjectFile = File("./source/Htc.Vita.Wix.CustomLibrary/Htc.Vita.Wix.CustomLibrary.wixproj");
var extensionProjectFile = File(string.Format("./source/{0}/{0}.net45.csproj", product));
var extensionTestBundleProjectFile = File(string.Format("./source/{0}.TestBundle/{0}.TestBundle.wixproj", product));

// Define directories.
var distDir = Directory("./dist");
var tempDir = Directory("./temp");
var generatedDir = Directory("./source/generated");
var packagesDir = Directory("./source/packages");
var nugetDir = Directory("./dist") + Directory(configuration) + Directory("nuget");
var homeDir = Directory(EnvironmentVariable("USERPROFILE") ?? EnvironmentVariable("HOME"));

// Define signing key, password and timestamp server
var signKeyEnc = EnvironmentVariable("SIGNKEYENC") ?? "NOTSET";
var signPass = EnvironmentVariable("SIGNPASS") ?? "NOTSET";
var signSha1Uri = new Uri("http://timestamp.verisign.com/scripts/timstamp.dll");
var signSha256Uri = new Uri("http://sha256timestamp.ws.symantec.com/sha256/timestamp");

// Define nuget push source and key
var nugetApiKey = EnvironmentVariable("NUGET_PUSH_TOKEN") ?? EnvironmentVariable("NUGET_APIKEY") ?? "NOTSET";
var nugetSource = EnvironmentVariable("NUGET_PUSH_PATH") ?? EnvironmentVariable("NUGET_SOURCE") ?? "NOTSET";


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Fetch-Git-Commit-ID")
    .ContinueOnError()
    .Does(() =>
{
    var lastCommit = GitLogTip(MakeAbsolute(Directory(".")));
    commitId = lastCommit.Sha;
});

Task("Display-Config")
    .IsDependentOn("Fetch-Git-Commit-ID")
    .Does(() =>
{
    Information("Build target: {0}", target);
    Information("Build configuration: {0}", configuration);
    Information("Build commitId: {0}", commitId);
    if ("Release".Equals(configuration))
    {
        Information("Build version: {0}", semanticVersion);
    }
    else
    {
        Information("Build version: {0}-CI{1}", ciVersion, revision);
    }
});

Task("Clean-Workspace")
    .IsDependentOn("Display-Config")
    .Does(() =>
{
    CleanDirectory(distDir);
    CleanDirectory(tempDir);
    CleanDirectory(generatedDir);
    CleanDirectory(packagesDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean-Workspace")
    .Does(() =>
{
    NuGetRestore(string.Format("./source/{0}.sln", product));
});

Task("Generate-AssemblyInfo")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    CreateDirectory(generatedDir);
    var file = "./source/Generated/SharedAssemblyInfo.cs";
    var assemblyVersion = semanticVersion;
    if (!"Release".Equals(configuration))
    {
        assemblyVersion = ciVersion;
    }
    CreateAssemblyInfo(
            file,
            new AssemblyInfoSettings
            {
                    Company = companyName,
                    Copyright = copyright,
                    Product = string.Format("{0} : {1}", product, commitId),
                    Version = version,
                    FileVersion = assemblyVersion,
                    InformationalVersion = assemblyVersion
            }
    );
});

Task("Build-Wix-Custom-Action")
    .IsDependentOn("Generate-AssemblyInfo")
    .Does(() =>
{
    // Use MSBuild
    MSBuild(
            customActionProjectFile,
            settings => settings.SetConfiguration(configuration)
    );
});

Task("Sign-Wix-Custom-Action")
    .WithCriteria(() => "Release".Equals(configuration) && !"NOTSET".Equals(signPass) && !"NOTSET".Equals(signKeyEnc))
    .IsDependentOn("Build-Wix-Custom-Action")
    .Does(() =>
{
    var currentSignTimestamp = DateTime.Now;
    Information("Last timestamp:    " + lastSignTimestamp);
    Information("Current timestamp: " + currentSignTimestamp);
    var totalTimeInMilli = (DateTime.Now - lastSignTimestamp).TotalMilliseconds;

    var signKey = "./temp/key.pfx";
    System.IO.File.WriteAllBytes(signKey, Convert.FromBase64String(signKeyEnc));

    var file = string.Format("./temp/{0}/{1}/bin/net45/{1}.CA.dll", configuration, "Htc.Vita.Wix.CustomAction");

    if (totalTimeInMilli < signIntervalInMilli)
    {
        System.Threading.Thread.Sleep(signIntervalInMilli - (int)totalTimeInMilli);
    }
    Sign(
            file,
            new SignToolSignSettings
            {
                    TimeStampUri = signSha1Uri,
                    CertPath = signKey,
                    Password = signPass
            }
    );
    lastSignTimestamp = DateTime.Now;

    System.Threading.Thread.Sleep(signIntervalInMilli);
    Sign(
            file,
            new SignToolSignSettings
            {
                    AppendSignature = true,
                    TimeStampUri = signSha256Uri,
                    DigestAlgorithm = SignToolDigestAlgorithm.Sha256,
                    TimeStampDigestAlgorithm = SignToolDigestAlgorithm.Sha256,
                    CertPath = signKey,
                    Password = signPass
            }
    );
    lastSignTimestamp = DateTime.Now;
});

Task("Build-Wix-Custom-Library")
    .IsDependentOn("Sign-Wix-Custom-Action")
    .Does(() =>
{
    // Use MSBuild
    MSBuild(
            customLibraryProjectFile,
            settings => settings.SetConfiguration(configuration)
    );
});

Task("Build-Wix-Extension")
    .IsDependentOn("Build-Wix-Custom-Library")
    .Does(() =>
{
    // Use MSBuild
    MSBuild(
            extensionProjectFile,
            settings => settings.SetConfiguration(configuration)
    );
});

Task("Sign-Wix-Extension")
    .WithCriteria(() => "Release".Equals(configuration) && !"NOTSET".Equals(signPass) && !"NOTSET".Equals(signKeyEnc))
    .IsDependentOn("Build-Wix-Extension")
    .Does(() =>
{
    var currentSignTimestamp = DateTime.Now;
    Information("Last timestamp:    " + lastSignTimestamp);
    Information("Current timestamp: " + currentSignTimestamp);
    var totalTimeInMilli = (DateTime.Now - lastSignTimestamp).TotalMilliseconds;

    var signKey = "./temp/key.pfx";
    System.IO.File.WriteAllBytes(signKey, Convert.FromBase64String(signKeyEnc));

    var file = string.Format("./temp/{0}/{1}/bin/net45/{1}.dll", configuration, "Htc.Vita.Wix.Extension");

    if (totalTimeInMilli < signIntervalInMilli)
    {
        System.Threading.Thread.Sleep(signIntervalInMilli - (int)totalTimeInMilli);
    }
    Sign(
            file,
            new SignToolSignSettings
            {
                    TimeStampUri = signSha1Uri,
                    CertPath = signKey,
                    Password = signPass
            }
    );
    lastSignTimestamp = DateTime.Now;

    System.Threading.Thread.Sleep(signIntervalInMilli);
    Sign(
            file,
            new SignToolSignSettings
            {
                    AppendSignature = true,
                    TimeStampUri = signSha256Uri,
                    DigestAlgorithm = SignToolDigestAlgorithm.Sha256,
                    TimeStampDigestAlgorithm = SignToolDigestAlgorithm.Sha256,
                    CertPath = signKey,
                    Password = signPass
            }
    );
    lastSignTimestamp = DateTime.Now;
});

Task("Build-Wix-Extension-Test")
    .IsDependentOn("Sign-Wix-Extension")
    .Does(() =>
{
    // Use MSBuild
    MSBuild(
            extensionTestBundleProjectFile,
            settings => settings.SetConfiguration(configuration)
    );
});

Task("Build-NuGet-Package")
    .IsDependentOn("Build-Wix-Extension-Test")
    .Does(() =>
{
    CreateDirectory(nugetDir);
    var nugetPackVersion = semanticVersion;
    if (!"Release".Equals(configuration))
    {
        nugetPackVersion = string.Format("{0}-CI{1}", ciVersion, revision);
    }
    Information("Pack version: {0}", nugetPackVersion);
    var nuGetPackSettings = new NuGetPackSettings
    {
            Id = product,
            Version = nugetPackVersion,
            Authors = new[] {"HTC"},
            Description = "[CommitId: " + commitId + "]",
            Copyright = copyright,
            Tags = nugetTags,
            RequireLicenseAcceptance= false,
            Files = new []
            {
                    new NuSpecContent
                    {
                            Source = string.Format("{0}/bin/net45/{0}.dll", product),
                            Target = "tools\\lib"
                    },
                    new NuSpecContent
                    {
                            Source = string.Format("{0}/bin/net45/{0}.pdb", product),
                            Target = "tools\\lib"
                    },
            },
            Properties = new Dictionary<string, string>
            {
                    {"Configuration", configuration}
            },
            BasePath = tempDir + Directory(configuration),
            OutputDirectory = nugetDir
    };

    NuGetPack(nuspecFile, nuGetPackSettings);
});

Task("Publish-NuGet-Package")
    .WithCriteria(() => "Release".Equals(configuration) && !"NOTSET".Equals(nugetApiKey) && !"NOTSET".Equals(nugetSource))
    .IsDependentOn("Build-NuGet-Package")
    .Does(() =>
{
    var nugetPushVersion = semanticVersion;
    if (!"Release".Equals(configuration))
    {
        nugetPushVersion = string.Format("{0}-CI{1}", ciVersion, revision);
    }
    Information("Publish version: {0}", nugetPushVersion);
    var package = string.Format("./dist/{0}/nuget/{1}.{2}.nupkg", configuration, product, nugetPushVersion);
    NuGetPush(
            package,
            new NuGetPushSettings
            {
                    Source = nugetSource,
                    ApiKey = nugetApiKey
            }
    );
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build-NuGet-Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
