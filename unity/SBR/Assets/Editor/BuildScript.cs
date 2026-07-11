using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using SBR.Probe;

/// <summary>
/// PLATFORM PROBE build harness — disposable (M1, Phase 2). NOT product code.
///
/// Creates the DeterminismProbe scene if it does not yet exist, then batchmode-builds it for a target
/// via -executeMethod. Windows Standalone confirms the DLL replays the pins inside a real compiled
/// player; WebGL (always IL2CPP/AOT) is the itch.io reach that retires the cross-platform determinism
/// risk. Build results are Debug.Log'd so a headless caller can parse them from the log.
/// </summary>
public static class BuildScript
{
    private const string ProbeScenePath = "Assets/Probe/DeterminismProbe.unity";

    private static string EnsureProbeScene()
    {
        if (File.Exists(ProbeScenePath))
            return ProbeScenePath;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.tag = "MainCamera";

        var probeGo = new GameObject("DeterminismProbe");
        probeGo.AddComponent<DeterminismProbe>();

        Directory.CreateDirectory("Assets/Probe");
        EditorSceneManager.SaveScene(scene, ProbeScenePath);
        AssetDatabase.Refresh();
        Debug.Log($"[BuildScript] created probe scene at {ProbeScenePath}");
        return ProbeScenePath;
    }

    // Windows IL2CPP is only reachable if the "Windows Build Support (IL2CPP)" module is installed.
    public static void BuildWindowsIl2cpp() => BuildWindows(ScriptingImplementation.IL2CPP);

    // Mono standalone: runnable headless to confirm the compiled player replays the pins.
    public static void BuildWindowsMono() => BuildWindows(ScriptingImplementation.Mono2x);

    private static void BuildWindows(ScriptingImplementation backend)
    {
        string scene = EnsureProbeScene();
        string dir = Path.GetFullPath("Builds/Standalone");
        Directory.CreateDirectory(dir);
        string exe = Path.Combine(dir, "SBRProbe.exe");

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, backend);

        var opts = new BuildPlayerOptions
        {
            scenes = new[] { scene },
            locationPathName = exe,
            target = BuildTarget.StandaloneWindows64,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.None,
        };
        Report(BuildPipeline.BuildPlayer(opts), $"Windows Standalone ({backend})");
    }

    public static void BuildWebGL()
    {
        string scene = EnsureProbeScene();
        string dir = Path.GetFullPath("Builds/WebGL");
        Directory.CreateDirectory(dir);

        var opts = new BuildPlayerOptions
        {
            scenes = new[] { scene },
            locationPathName = dir,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        };
        Report(BuildPipeline.BuildPlayer(opts), "WebGL");
    }

    private static void Report(BuildReport report, string label)
    {
        BuildSummary s = report.summary;
        Debug.Log($"[BuildScript] {label} result={s.result} outputPath={s.outputPath} " +
                  $"totalSize={s.totalSize} errors={s.totalErrors} warnings={s.totalWarnings}");
        if (s.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
