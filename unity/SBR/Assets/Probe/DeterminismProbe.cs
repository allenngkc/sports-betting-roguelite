using UnityEngine;

namespace SBR.Probe
{
    /// <summary>
    /// PLATFORM PROBE — disposable (M1, Phase 2). NOT product code; deleted once the platform risk is retired.
    ///
    /// On Awake it replays the golden scripted round through SBR.Engine.dll, compares against the pins in
    /// <see cref="GoldenReplay"/>, and renders PASS (green) / FAIL (red) plus the event-stream FNV-1a hash
    /// as on-screen text (OnGUI is fine here — this is a probe, not product). It also Debug.Logs the verdict
    /// so it lands in the player log for headless standalone runs, and, when running as a built player (not
    /// in the editor), quits after a couple of frames with an exit code that mirrors the verdict.
    /// </summary>
    public sealed class DeterminismProbe : MonoBehaviour
    {
        private GoldenReplay.Result _result;
        private int _frames;

        private void Awake()
        {
            _result = GoldenReplay.Verify();
            Debug.Log($"[DeterminismProbe] {(_result.Pass ? "PASS" : "FAIL")} " +
                      $"seed={GoldenReplay.Seed} hash={_result.Hash} :: {_result.Detail}");
        }

        private void Update()
        {
            // In a built player, self-terminate so headless (-batchmode) runs exit; the code mirrors the verdict.
            if (!Application.isEditor && ++_frames >= 2)
                Application.Quit(_result.Pass ? 0 : 1);
        }

        private void OnGUI()
        {
            var title = new GUIStyle { fontSize = 48, fontStyle = FontStyle.Bold };
            title.normal.textColor = _result.Pass ? Color.green : Color.red;
            GUI.Label(new Rect(40, 40, Screen.width - 80, 120),
                _result.Pass ? "DETERMINISM: PASS" : "DETERMINISM: FAIL", title);

            var body = new GUIStyle { fontSize = 22 };
            body.normal.textColor = Color.white;
            GUI.Label(new Rect(40, 170, Screen.width - 80, 400),
                $"seed {GoldenReplay.Seed}\nevent-stream FNV-1a: {_result.Hash}\n{_result.Detail}", body);
        }
    }
}
