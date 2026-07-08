using SBR.Engine;

namespace SBR.Engine.Tests;

public class RngTests
{
    [Fact]
    public void Same_seed_and_sequence_produce_identical_streams()
    {
        var a = new Pcg32(42, 7);
        var b = new Pcg32(42, 7);
        for (int i = 0; i < 100; i++)
            Assert.Equal(a.NextUInt(), b.NextUInt());
    }

    [Fact]
    public void Different_sequences_diverge_for_same_seed()
    {
        var a = new Pcg32(42, 1);
        var b = new Pcg32(42, 2);
        bool anyDifferent = false;
        for (int i = 0; i < 20; i++)
            anyDifferent |= a.NextUInt() != b.NextUInt();
        Assert.True(anyDifferent);
    }

    [Fact]
    public void NextDouble_stays_in_unit_interval()
    {
        var rng = new Pcg32(123, 456);
        for (int i = 0; i < 10_000; i++)
        {
            double d = rng.NextDouble();
            Assert.InRange(d, 0.0, 0.9999999999);
        }
    }

    [Fact]
    public void NextInt_respects_bounds()
    {
        var rng = new Pcg32(9, 9);
        for (int i = 0; i < 10_000; i++)
            Assert.InRange(rng.NextInt(3, 17), 3, 16);
    }

    [Fact]
    public void NextInt_rejects_empty_range()
        => Assert.Throws<ArgumentException>(() => new Pcg32(1, 1).NextInt(5, 5));

    [Fact]
    public void Hub_streams_are_independent_and_reproducible()
    {
        var hub1 = new RngHub("8F3K-22");
        var hub2 = new RngHub("8F3K-22");

        Assert.Equal(hub1.Outcomes.NextUInt(), hub2.Outcomes.NextUInt());
        Assert.Equal(hub1.Slate.NextUInt(), hub2.Slate.NextUInt());

        // Draining one stream must not perturb another.
        var hub3 = new RngHub("8F3K-22");
        var hub4 = new RngHub("8F3K-22");
        for (int i = 0; i < 1000; i++) hub3.Shop.NextUInt();
        Assert.Equal(hub4.Outcomes.NextUInt(), hub3.Outcomes.NextUInt());
    }

    [Fact]
    public void Different_run_seeds_produce_different_outcomes()
    {
        var a = new RngHub("SEED-A");
        var b = new RngHub("SEED-B");
        bool anyDifferent = false;
        for (int i = 0; i < 20; i++)
            anyDifferent |= a.Outcomes.NextUInt() != b.Outcomes.NextUInt();
        Assert.True(anyDifferent);
    }
}
