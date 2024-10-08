#addin nuget:?package=Cake.Unity&version=0.9.0

var target = Argument("target", "Pack");
var configuration = Argument("configuration", "Release");
var ksp2Root = Argument("ksp2-root", string.Empty);
var skipUnity = HasArgument("skip-unity");

Task("Clean")
    .Does(() =>
    {
        CleanDirectory($"./build");
        Information("Cleaned ./build");
        DotNetClean("./ksp2-inputbinder/ksp2-inputbinder.csproj", new DotNetCleanSettings
        {
            Configuration = configuration,
            NoLogo = true,
            Verbosity = (DotNetVerbosity)1
        });
    });

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        Information("Create output directory");
        CreateDirectory("./build/BepInEx/plugins/inputbinder/");

        DotNetBuild("./ksp2-inputbinder/ksp2-inputbinder.csproj", new DotNetBuildSettings
        {
            Configuration = configuration,
            NoLogo = true
        });

        Information("Copying assembly build products to output directory");
        CopyFileToDirectory($"./ksp2-inputbinder/bin/{configuration}/netstandard2.1/Inputbinder.dll", Directory("./build/BepInEx/plugins/inputbinder/"));
        if (configuration == "Debug")
            CopyFileToDirectory($"./ksp2-inputbinder/bin/{configuration}/netstandard2.1/Inputbinder.pdb", Directory("./build/BepInEx/plugins/inputbinder/"));

        if (!skipUnity)
        {
            if (BuildSystem.GitHubActions.IsRunningOnGitHubActions)
            {
                Information("Skipping Unity Asset Build as it will be done by GameCI");
            }
            else
            {
                // Find unity editor 2022.3.5f1
                Information("Trying to find Unity Editor version 2022.3.5f1");
                var unityEditor = FindUnityEditor(2022, 3, 5, 'f');
                if (unityEditor is null)
                {
                    throw new CakeException("Could not find Unity Editor 2022.3.5f1");
                }
                else
                {
                    Information($"Unity Editor 2022.3.5f1 found: {unityEditor}");
                    // Set unity log file location according to: https://docs.unity3d.com/Manual/LogFiles.html
                    FilePath unityLogLocation = File("");
                    if (IsRunningOnLinux()) unityLogLocation = File(@"~/.config/unity3d/Editor.log");
                    if (IsRunningOnMacOs()) unityLogLocation = File(@"~/Library/Logs/Unity/Editor.log");
                    if (IsRunningOnWindows()) unityLogLocation = ExpandEnvironmentVariables(File(@"%LOCALAPPDATA%\Unity\Editor\Editor.log"));

                    Information("Building assets");
                    UnityEditor(unityEditor, new UnityEditorArguments
                    {
                        BatchMode = true,
                        NoGraphics = true,
                        Quit = true,
                        ProjectPath = MakeAbsolute(Directory("./ksp2-inputbinder-assets")),
                        ExecuteMethod = "BuildAssets.PerformBuild",
                        LogFile = unityLogLocation
                    }, new UnityEditorSettings
                    {
                        RealTimeLog = false
                    });
                }
                Information("Copying assets to output directory");
                CopyDirectory("./ksp2-inputbinder-assets/Library/com.unity.addressables/aa/Windows", "./build/BepInEx/plugins/inputbinder/addressables");
            }
        }
        else
        {
            Warning("\x1B[33mWARNING: Skipping Unity Build as requested by --skip-unity\x1B[0m");
        }
    });

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
    {
        if (BuildSystem.GitHubActions.IsRunningOnGitHubActions)
        {
            Information("Copying assets to output directory");
            CopyDirectory("./ksp2-inputbinder-assets/Library/com.unity.addressables/aa/Windows", "./build/BepInEx/plugins/inputbinder/addressables");
        }
        CopyFile("./README.md", "./build/BepInEx/plugins/inputbinder/README.md");
        CopyFile("./LICENSE.txt", "./build/BepInEx/plugins/inputbinder/LICENSE.txt");
        Zip("./build", "build/ksp2-inputbinder.zip", "./build/**/*");
    });


Task("Install")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var gameDir = ksp2Root != string.Empty ? ksp2Root : EnvironmentVariable("KSP2_PATH") ?? throw new ArgumentNullException("ksp2-root");
        CopyDirectory("./build/", Directory(gameDir));
    });

Task("Uninstall")
    .Does(() =>
    {
        var gameDir = ksp2Root != string.Empty ? ksp2Root : EnvironmentVariable("KSP2_PATH") ?? throw new ArgumentNullException("ksp2-root");
        DeleteDirectory(Directory(gameDir) + Directory("BepInEx/plugins/inputbinder"), new DeleteDirectorySettings
        {
            Recursive = true
        });
    });

Task("Start")
    .IsDependentOn("Install")
    .Does(() =>
    {
        var gameDir = ksp2Root != string.Empty ? ksp2Root : EnvironmentVariable("KSP2_PATH") ?? throw new ArgumentNullException("ksp2-root");
        StartAndReturnProcess(Directory(gameDir) + File("KSP2_x64.exe"), new ProcessSettings
        {
            Arguments = ProcessArgumentBuilder.FromString("-single-instance"),
            WorkingDirectory = Directory(gameDir)
        });
    });

RunTarget(target);