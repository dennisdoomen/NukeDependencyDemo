using System;
using Nuke.Common;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Default);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] string AzureToken;

    bool HasChanges;
    readonly bool HasChangesDynamically1;

    Target DetermineChanges => _ => _
        .Executes(() =>
        {
            HasChanges = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Changes"));
        });

    Target Prepare => _ => _
        .Requires(() => AzureToken)
        .Executes(() =>
        {
        });

    Target TargetUsingDynamicCondition => _ => _
        .DependsOn(Prepare)
        .DependsOn(DetermineChanges)
        .OnlyWhenDynamic(() => HasChanges)
        .Executes(() =>
        {
            // Will run target Dependency even if HasChanges is false
            // and thus will also require SomeParameter
        });

    Target TargetUsingStaticCondition => _ => _
        .DependsOn(Prepare)
        .DependsOn(DetermineChanges)
        .OnlyWhenStatic(() => HasChanges)
        .Executes(() =>
        {
            // This will evaluate HasChanges before DetermineChanges is even executed, and
            // thus not run. And as a result, Dependency will also not run, nor will it
            // require SomeParameter
        });

    bool? HasChangesState = null;

    bool HasChangesDynamically
    {
        get
        {
            if (!HasChangesState.HasValue)
            {
                HasChangesState = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Changes"));
            }

            return HasChangesState.Value;
        }
    }

    Target TargetUsingDynamicProperty => _ => _
        .DependsOn(Prepare)
        .DependsOn(DetermineChanges)
        .OnlyWhenStatic(() => HasChangesDynamically)
        .Executes(() =>
        {
            // This will evaluate HasChanges before DetermineChanges is even executed, and
            // thus not run. And as a result, Dependency will also not run, nor will it
            // require SomeParameter
        });

    Target Default => _ => _
        .DependsOn(TargetUsingDynamicCondition)
        .DependsOn(TargetUsingStaticCondition)
        .DependsOn(TargetUsingDynamicProperty);
}