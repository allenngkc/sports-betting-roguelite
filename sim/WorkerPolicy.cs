using System;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SBR.Sim;

/// <summary>
/// How many runs the harness executes at once — the single source of
/// <see cref="System.Threading.Tasks.ParallelOptions.MaxDegreeOfParallelism"/> for the campaign.
///
/// WHY THIS IS AMBIENT RATHER THAN A PARAMETER. Every parallel loop in the sim lives in exactly one
/// place (<see cref="Harness.RunBatch"/>); the batches themselves are driven from four unrelated
/// call sites (Program's strategy roster, Program's martyr-worst batch, AuditData.Compute,
/// ComboData.Compute). Threading a worker count through those signatures would touch four APIs to
/// deliver one number that is a property of the MACHINE, not of the measurement. So the count is
/// resolved by this class and read by RunBatch. Nothing about a run's numbers can depend on it —
/// see RunBatch's own note — so it is ambient state that provably cannot leak into a result.
///
/// WHY IT RE-CHECKS. A campaign is ~85 minutes. Allen is not necessarily at the keyboard for all of
/// it, and whether he is changes the right answer. <see cref="NextBatch"/> is called once per batch,
/// so a campaign that starts while he is working ramps up when he walks away, and backs off again
/// when he sits down — at the next batch boundary, never mid-batch (a batch is the only unit at
/// which changing the degree costs nothing).
///
/// THE TIERS. All are derived from <see cref="Cores"/> so the policy travels to other machines.
///   • ACTIVE (input inside <see cref="ActiveWindow"/>): cores/4, floor 1. On Allen's 22-core box
///     that is 5. Chosen so ~three quarters of the machine stays free for whatever he is doing —
///     and it costs the campaign almost nothing today, because the measured utilisation of a
///     samematch batch is ~6 cores even when the degree is unbounded (thread-pool ramp, not
///     missing parallelism). A low mode that sits just under the level the workload actually
///     reaches is a cap in name only, which is exactly what "stay responsive" should cost.
///   • COOLING (5–10 min since input): cores/2. The windows in the brief leave a band between
///     "just typed" and "definitely gone"; stepping through it beats picking a side, because both
///     mistakes are cheap here and neither is worth a stall.
///   • IDLE (no input for <see cref="IdleWindow"/>): min(cores − 2, 16). The two reserved cores are
///     not superstition — they are what keeps the shell, the editor and the OS answering if Allen
///     comes back mid-batch, which is the moment the cap exists to protect. The 16 is measured; see
///     below.
///
/// THE 16 IS EMPIRICAL, AND IT IS RULED ONLY FOR THE IDLE TIER. Measured on the 22-core box, one
/// 2,000-run batch, Release: wall bottoms out at 16 workers and REGRESSES above it (skilled 3.11 s
/// at 16 → 4.28 s at 22 with server GC; 5.65 s → 6.58 s with the default workstation GC), while CPU
/// burned keeps climbing — the extra threads spend their time queueing in the allocator, not on
/// runs. An uncapped cores − 2 was therefore ~35% slower here than the knee. Allen ruled the ceiling
/// on 2026-08-15; it is a measurement of THIS hardware, not a law.
///
/// OPEN QUESTION — DO NOT ANSWER IT BY GENERALISING. Whether ACTIVE and COOLING want the same
/// ceiling is UNMEASURED AND UNRULED. It cannot be observed on this box: at 22 cores they resolve to
/// 5 and 11, both already under 16, so the cap would be dead code here. On a 64-core machine cooling
/// would ask for 32 and active for exactly 16, and the cooling tier is where the question first
/// becomes real. Do not assume the same 16 transfers: contention scales with heap and core topology,
/// and under server GC (now the shipped default — see SBR.Sim.csproj) each core gets its own heap,
/// so the knee that produced this number may itself move on bigger hardware. The honest way to
/// settle it is to measure the knee on the box in question and have it ruled — not to copy 16 down
/// from a line written about a 22-core desktop.
///
/// NON-WINDOWS. The idle probe is a user32 P/Invoke and there is no portable equivalent. Off
/// Windows the class never calls it (<see cref="OperatingSystem.IsWindows"/> guard) and reports the
/// machine as IDLE. That is the right default rather than a cop-out: a box running this harness
/// without a Windows session is a CI runner or a server, where there is no interactive user whose
/// responsiveness we would be protecting. A manual --workers still overrides, everywhere.
/// </summary>
public static class WorkerPolicy
{
    /// <summary>Logical cores visible to this process (honours affinity / container limits).</summary>
    public static readonly int Cores = Environment.ProcessorCount;

    public static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan IdleWindow = TimeSpan.FromMinutes(10);

    /// <summary>
    /// The empirical ceiling on useful concurrency for THIS workload on THIS box — the knee where
    /// added workers start queueing in the allocator instead of running runs, measured, not guessed
    /// (see the class note for the numbers). Applied to <see cref="IdleWorkers"/> ONLY: that is the
    /// tier Allen ruled, and the only tier that reaches it at 22 cores.
    /// </summary>
    private const int MeasuredKnee = 16;

    // ACTIVE and COOLING are deliberately UNCAPPED. Not an oversight and not a pending TODO: on this
    // hardware they resolve to 5 and 11, so a ceiling would never bind, and no one has measured
    // whether the knee is even in the same place on a machine large enough for them to exceed it.
    // See the class note's OPEN QUESTION before adding MeasuredKnee to either of these.
    public static int ActiveWorkers => Math.Max(1, Cores / 4);
    public static int CoolingWorkers => Math.Max(1, Cores / 2);

    /// <summary>Ruled: min(cores − 2, 16). Still derived from <see cref="Cores"/>, so a small box
    /// scales down rather than inheriting a desktop's number.</summary>
    public static int IdleWorkers => Math.Max(1, Math.Min(Cores - 2, MeasuredKnee));

    private static int _manual;              // 0 = auto; else the --workers override
    private static int _min = int.MaxValue;
    private static int _max;
    private static int _last;
    private static int _batches;
    private static string _lastReason = "not resolved";

    /// <summary>The --workers override. MANUAL BEATS AUTO — the brief's rule, and the reason the
    /// determinism proof can pin a run at 1 worker on a machine that is otherwise idle.</summary>
    public static void SetManual(int workers)
    {
        if (workers < 1) throw new ArgumentOutOfRangeException(nameof(workers), "workers must be ≥ 1");
        _manual = workers;
    }

    public static bool IsManual => _manual > 0;

    /// <summary>
    /// Resolve the degree for the batch about to start, re-probing idle state. Called from the batch
    /// driver thread only (RunBatch's caller side, before the Parallel.For), so the bookkeeping below
    /// needs no lock: batches never overlap.
    /// </summary>
    public static int NextBatch()
    {
        int n;
        if (_manual > 0)
        {
            n = _manual;
            _lastReason = "manual --workers";
        }
        else
        {
            TimeSpan? idle = IdleFor();
            if (idle is null)
            {
                n = IdleWorkers;
                _lastReason = OperatingSystem.IsWindows()
                    ? "idle probe unavailable, assumed idle"
                    : "non-Windows, assumed idle";
            }
            else if (idle.Value < ActiveWindow)
            {
                n = ActiveWorkers;
                _lastReason = $"active, input {Mins(idle.Value)} min ago";
            }
            else if (idle.Value < IdleWindow)
            {
                n = CoolingWorkers;
                _lastReason = $"cooling, input {Mins(idle.Value)} min ago";
            }
            else
            {
                n = IdleWorkers;
                _lastReason = $"idle {Mins(idle.Value)} min";
            }
        }

        _last = n;
        _batches++;
        if (n < _min) _min = n;
        if (n > _max) _max = n;
        return n;
    }

    /// <summary>
    /// One header line. It names the count the report was PRODUCED at, which is the whole point:
    /// an artifact that claims its numbers are worker-count-independent and does not say what count
    /// it ran at cannot be falsified after the fact. When auto re-checked its way to more than one
    /// value across the campaign, the range and the batch count are printed rather than a tidy
    /// single number that would be a lie.
    /// </summary>
    public static string Describe()
    {
        if (_batches == 0)
            return $"unused (no batches ran); {Cores} logical cores; {GcMode}";

        string span = _min == _max
            ? _min.ToString(CultureInfo.InvariantCulture)
            : $"{_min.ToString(CultureInfo.InvariantCulture)}–{_max.ToString(CultureInfo.InvariantCulture)}";
        string how = _manual > 0
            ? "manual, --workers"
            : _min == _max
                ? $"auto — {_lastReason}"
                : $"auto, re-checked per batch over {_batches.ToString("N0", CultureInfo.InvariantCulture)} "
                  + $"batches; last {_lastReason}";
        return $"{span} ({how}); {Cores.ToString(CultureInfo.InvariantCulture)} logical cores; {GcMode}";
    }

    /// <summary>
    /// Which collector this process actually got, observed at runtime rather than inferred from the
    /// project file. It is recorded in the header for the same reason the worker count is: server GC
    /// is the shipped default (SBR.Sim.csproj) but an explicit DOTNET_gcServer beats the
    /// runtimeconfig, so the setting a reader can see in source is not proof of the setting a report
    /// was produced under. This line closes that gap — and it is what makes the GC half of the
    /// byte-identity proof checkable: two headers naming DIFFERENT collectors above one identical
    /// body is the whole claim, and neither half of it is legible if the mode goes unrecorded.
    /// Header-only, like the worker count — never in Body().
    /// </summary>
    public static string GcMode => GCSettings.IsServerGC ? "server GC" : "workstation GC";

    /// <summary>Time since the last keyboard/mouse input, or null when the machine cannot be asked
    /// (non-Windows, or the call failed). Null means "treat as idle" — see the class note.</summary>
    public static TimeSpan? IdleFor()
    {
        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            var info = new LastInputInfo { cbSize = (uint)Marshal.SizeOf<LastInputInfo>() };
            if (!GetLastInputInfo(ref info)) return null;
            // dwTime and Environment.TickCount are the SAME 32-bit millisecond counter, which wraps
            // roughly every 49.7 days. Unsigned subtraction is correct across the wrap; the widened
            // 64-bit TickCount64 is not, because dwTime never leaves 32 bits.
            uint elapsed = unchecked((uint)Environment.TickCount - info.dwTime);
            return TimeSpan.FromMilliseconds(elapsed);
        }
        catch (DllNotFoundException) { return null; }
        catch (EntryPointNotFoundException) { return null; }
    }

    private static string Mins(TimeSpan t) =>
        ((int)t.TotalMinutes).ToString(CultureInfo.InvariantCulture);

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint cbSize;
        public uint dwTime;
    }

    // SYSLIB1054 asks for the LibraryImport source generator. Its generated stub pins the struct
    // with `fixed`, which would force <AllowUnsafeBlocks> on the whole sim project for one call —
    // a worse trade than the classic marshaller for a two-field blittable struct.
#pragma warning disable SYSLIB1054
    [SupportedOSPlatform("windows")]
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetLastInputInfo(ref LastInputInfo plii);
#pragma warning restore SYSLIB1054
}
