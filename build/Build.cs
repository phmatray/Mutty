using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.OctoVersion;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Octokit.Internal;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

[GitHubActions(
    "continuous",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    FetchDepth = 0,
    OnPushBranches = [MainBranch, DevelopBranch, ReleasesBranch],
    OnPullRequestBranches = [ReleasesBranch],
    InvokedTargets = [nameof(Pack)],
    EnableGitHubToken = true,
    ImportSecrets = [nameof(NuGetApiKey)]
)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Nuke.Common.Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Nuke.Common.Parameter("Nuget Feed Url for Public Access of Pre Releases")]
    readonly string NugetFeed;
    
    [Nuke.Common.Parameter("Nuget Api Key"), Secret]
    readonly string NuGetApiKey;

    [Nuke.Common.Parameter("Copyright Details")]
    readonly string Copyright;

    [Nuke.Common.Parameter("Artifacts Type")]
    readonly string ArtifactsType;

    [Nuke.Common.Parameter("Excluded Artifacts Type")]
    readonly string ExcludedArtifactsType = ".snupkg";
    
    // The Required Attribute will automatically throw an exception if the 
    // OctoVersionInfo parameter is not set due to an error or misconfiguration in Nuke.
    [Required]
    [OctoVersion(
        AutoDetectBranch = true,
        UpdateBuildNumber = true,
        Framework = "net9.0",
        Major = 1)]
    readonly OctoVersionInfo OctoVersionInfo;

    [GitRepository]
    readonly GitRepository GitRepository;
    
    [Solution(SuppressBuildProjectCheck = true, GenerateProjects = true)]
    readonly Solution Solution;
    
    const string MainBranch = "main";
    const string DevelopBranch = "develop";
    const string ReleasesBranch = "releases/**";

    const string PackageContentType = "application/octet-stream";

    GitHubActions GitHubActions => GitHubActions.Instance;
    
    AbsolutePath SourceDirectory => RootDirectory / "src";
    
    AbsolutePath ArtifactsDirectory => RootDirectory / ".artifacts";

    static string ChangeLogFile => RootDirectory / "CHANGELOG.md";
    
    string GithubNugetFeed => GitHubActions != null
        ? $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner}/index.json"
        : null;
    
    Target Version => _ => _
        .Description("Logs the GitVersion")
        .Executes(() =>
        {
            Log.Information("GitVersion = {Value}", OctoVersionInfo.NuGetVersion);
        });
    
    Target Clean => _ => _
        .Description("Clean solution and projects directories")
        .Before(Restore)
        .Executes(() =>
        {
            // Clean projects
            DotNetClean(c => c.SetProject(Solution.src.library.Mutty));
            DotNetClean(c => c.SetProject(Solution.src.tests.Mutty_Tests));
            DotNetClean(c => c.SetProject(Solution.src.demo.ConsoleApp));
            
            // Clean artifacts directory
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Description("Restore project dependencies")
        .DependsOn(Clean)
        .Executes(() =>
        {
            Log.Information("Restoring NuGet packages");
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetVerbosity(DotNetVerbosity.minimal)
                .SetNoCache(true));
            Log.Information("NuGet packages restored successfully");
        });
    
    Target Compile => _ => _
        .Description("Compile the solution")
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information("Building solution {Solution} with configuration {Configuration}", Solution, Configuration);
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(OctoVersionInfo.NuGetVersion)
                .SetAssemblyVersion(OctoVersionInfo.MajorMinorPatch)
                .SetInformationalVersion(OctoVersionInfo.InformationalVersion)
                .SetFileVersion(OctoVersionInfo.MajorMinorPatch)
                .SetDeterministic(true)
                .EnableNoRestore());
            Log.Information("Solution built successfully");
        });
    
    Target UnitTests => _ => _
        .Description("Run unit tests")
        .DependsOn(Compile)
        .Executes(() =>
        {
            Log.Information("Running unit tests");
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore());
            Log.Information("Unit tests passed successfully");
        });

    Target Pack => _ => _
        .Description("Pack library into NuGet packages with the version")
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Produces(ArtifactsDirectory / ArtifactsType)
        .DependsOn(Compile, UnitTests)
        .Triggers(PublishToGithub, PublishToNuGet, ValidateNuGetPackage)
        .Executes(() =>
        {
            Log.Information("Packing NuGet package");
            DotNetPack(s => s
                .SetProject(Solution.src.library.Mutty)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableNoBuild()
                .EnableNoRestore()
                .SetIncludeSymbols(false)
                .SetIncludeSource(false)
                .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                .SetCopyright(Copyright)
                .SetVersion(OctoVersionInfo.NuGetVersion)
                .SetAssemblyVersion(OctoVersionInfo.MajorMinorPatch)
                .SetInformationalVersion(OctoVersionInfo.InformationalVersion)
                .SetFileVersion(OctoVersionInfo.MajorMinorPatch));
            Log.Information("NuGet package packed successfully");
        });

    Target PublishToGithub => _ => _
        .Description($"Publishing to Github for Development only.")
        .Triggers(CreateRelease)
        .Requires(() => Configuration.Equals(Configuration.Release))
        .OnlyWhenStatic(() => GitRepository.IsOnDevelopBranch() || GitHubActions?.IsPullRequest == true)
        .Executes(() =>
        {
            ArtifactsDirectory.GlobFiles(ArtifactsType)
                .Where(x => !x.Name.EndsWith(ExcludedArtifactsType))
                .ForEach(x =>
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSource(GithubNugetFeed)
                        .SetApiKey(GitHubActions.Token)
                        .EnableSkipDuplicate()
                    );
                });
        });
    
    Target PublishToNuGet => _ => _
        .Description($"Publishing to NuGet with the version.")
        .Triggers(CreateRelease)
        .Requires(() => Configuration.Equals(Configuration.Release))
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .Executes(() =>
        {
            ArtifactsDirectory.GlobFiles(ArtifactsType)
                .Where(x => !x.Name.EndsWith(ExcludedArtifactsType))
                .ForEach(x =>
                {
                    DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSource(NugetFeed)
                        .SetApiKey(NuGetApiKey)
                        .EnableSkipDuplicate()
                    );
                });
        });
    
    Target CreateRelease => _ => _
        .Description($"Creating release for the publishable version.")
        .Requires(() => Configuration.Equals(Configuration.Release))
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
        .Executes(async () =>
        {
            var credentials = new Credentials(GitHubActions.Token);
            GitHubTasks.GitHubClient = new GitHubClient(
                new ProductHeaderValue(nameof(NukeBuild)),
                new InMemoryCredentialStore(credentials));

            string owner = GitRepository.GetGitHubOwner();
            string name = GitRepository.GetGitHubName();

            var releaseTag = OctoVersionInfo?.NuGetVersion;
            var changeLogSectionEntries = ChangelogTasks.ExtractChangelogSectionNotes(ChangeLogFile);
            var latestChangeLog = changeLogSectionEntries
                .Aggregate((c, n) => c + Environment.NewLine + n);

            var newRelease = new NewRelease(releaseTag)
            {
                TargetCommitish = GitRepository.Branch,
                Draft = true,
                Name = $"v{releaseTag}",
                Prerelease = !string.IsNullOrEmpty(OctoVersionInfo.PreReleaseTag),
                Body = latestChangeLog
            };

            var createdRelease = await GitHubTasks
                .GitHubClient
                .Repository
                .Release.Create(owner, name, newRelease);

            ArtifactsDirectory.GlobFiles(ArtifactsType)
                .Where(x => !x.Name.EndsWith(ExcludedArtifactsType))
                .ForEach(async void (x) => await UploadReleaseAssetToGithub(createdRelease, x));

            await GitHubTasks
                .GitHubClient
                .Repository
                .Release
                .Edit(owner, name, createdRelease.Id, new ReleaseUpdate { Draft = false });
        });

    static async Task UploadReleaseAssetToGithub(Release release, string asset)
    {
        await using var artifactStream = File.OpenRead(asset);
        var fileName = Path.GetFileName(asset);
        var assetUpload = new ReleaseAssetUpload
        {
            FileName = fileName,
            ContentType = PackageContentType,
            RawData = artifactStream,
        };
        await GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, assetUpload);
    }
    
    Target InstallNuGetValidator => _ => _
        .Description("Restore local dotnet tools (includes meziantou.validate-nuget-package)")
        .DependsOn(Pack)
        .Executes(() =>
        {
            // Restore local dotnet tools declared in .config/dotnet-tools.json
            ProcessTasks.StartProcess("dotnet", "tool restore").AssertZeroExitCode();
        });

    Target ValidateNuGetPackage => _ => _
        .Description("Validate packed NuGet packages with Meziantou's validator")
        .DependsOn(InstallNuGetValidator)
        .Executes(() =>
        {
            // Validate each .nupkg in the artifacts directory
            var nuGetPackages = (RootDirectory / ".artifacts").GlobFiles("*.nupkg");
            foreach (var package in nuGetPackages)
            {
                Log.Information("Validating NuGet package: {Package}", package);
                IProcess process = ProcessTasks.StartProcess("dotnet", $"tool run meziantou.validate-nuget-package {package}");
                process.AssertZeroExitCode();
            }
        });
}
