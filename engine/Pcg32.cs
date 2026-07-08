namespace SBR.Engine;

/// <summary>
/// PCG-XSH-RR 32-bit generator. Hand-rolled (not System.Random) so that a given
/// seed produces identical sequences on every runtime the engine ships on —
/// .NET, Unity/IL2CPP, WebGL. Determinism here underwrites replays, golden-seed
/// tests, and future daily challenges.
/// </summary>
public sealed class Pcg32
{
    private ulong _state;
    private readonly ulong _inc;

    public Pcg32(ulong seed, ulong sequence)
    {
        _inc = (sequence << 1) | 1UL;
        _state = 0;
        NextUInt();
        _state += seed;
        NextUInt();
    }

    public uint NextUInt()
    {
        ulong old = _state;
        _state = old * 6364136223846793005UL + _inc;
        uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
        int rot = (int)(old >> 59);
        return (xorshifted >> rot) | (xorshifted << (-rot & 31));
    }

    /// <summary>Uniform double in [0, 1).</summary>
    public double NextDouble() => NextUInt() * (1.0 / 4294967296.0);

    /// <summary>Uniform int in [minInclusive, maxExclusive).</summary>
    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
            throw new System.ArgumentException("maxExclusive must be greater than minInclusive");
        uint range = (uint)(maxExclusive - minInclusive);
        return minInclusive + (int)(NextUInt() % range);
    }
}
