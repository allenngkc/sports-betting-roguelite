using System;
using System.Globalization;
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
///   • IDLE (no input for <see cref="IdleWindow"/>): cores − 2. The two reserved cores are not
///     superstition — they are what keeps the shell, the editor and the OS answering if Allen comes
///     back mid-batch, which is the moment the cap exists to protect.
///
/// MEASURED, AND NOT ACTED ON. cores − 2 is Allen's ruled idle number, and it is left alone — but it
/// is NOT this machine's fastest. Measured on the 22-core box, one 2,000-run batch, Release:
/// wall bottoms out at 16 workers and REGRESSES above it (skilled 3.11 s at 16 → 4.28 s at 22 with
/// server GC; 5.65 s → 6.58 s with the default workstation GC), while CPU burned keeps climbing —
/// the extra threads spend their time in the allocator, not on runs. So the ruled idle tier of 20
/// is roughly a third slower than the knee at ~16, and `--workers 16` beats auto-idle today. That is
/// a number to re-rule, not a number to quietly change here.
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

    public static int ActiveWorkers => Math.Max(1, Cores / 4);
    public static int CoolingWorkers => Math.Max(1, Cores / 2);
    public static int IdleWorkers => Math.Max(1, Cores - 2);

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
            return $"unused (no batches ran); {Cores} logical cores";

        string span = _min == _max
            ? _min.ToString(CultureInfo.InvariantCulture)
            : $"{_min.ToString(CultureInfo.InvariantCulture)}–{_max.ToString(CultureInfo.InvariantCulture)}";
        string how = _manual > 0
            ? "manual, --workers"
            : _min == _max
                ? $"auto — {_lastReason}"
                : $"auto, re-checked per batch over {_batches.ToString("N0", CultureInfo.InvariantCulture)} "
                  + $"batches; last {_lastReason}";
        return $"{span} ({how}); {Cores.ToString(CultureInfo.InvariantCulture)} logical cores";
    }

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
