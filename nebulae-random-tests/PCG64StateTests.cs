using System.Numerics;
using System.Reflection;
using nebulae.rng;

namespace nebulae.rng.tests;

public class PCG64StateTests
{
    private static readonly BigInteger Multiplier = ((BigInteger)2549297995355413924UL << 64) | 4865540595714422341UL;

    // Generated with the official pcg-c pcg_setseq_128_srandom_r(42, seq),
    // pcg_setseq_128_advance_r(1 << power), then four xsl_rr_64 outputs.
    // Compiled with clang -O2, linking src/pcg-advance-128.c.
    // https://github.com/imneme/pcg-c
    [Theory]
    [InlineData(54, 64, 0xc4ebffdcfe29bbacUL, 0x2ef2cf381d9b37c5UL, 0xe00beef5bf53ce59UL, 0x1f6d6a43a9dc7b1fUL)]
    [InlineData(54, 96, 0x2b68828ae1a76206UL, 0xb3050f2cc12a91b1UL, 0xe0e9e0dd550d8535UL, 0x7dae009ade42719aUL)]
    [InlineData(55, 64, 0xad9790f40567a0c8UL, 0xff06bea3bc61d318UL, 0xb689a50bc8cddb67UL, 0xa9d156052a031e33UL)]
    [InlineData(55, 96, 0x090b0de9719c6b8cUL, 0x75ae1582b9609cf6UL, 0xa360aa029ecdd16aUL, 0x1d9666b951d3b0b1UL)]
    public void Jumps_MatchOfficialCForDifferentStreams(int seq, int power, params ulong[] expected)
    {
        var rng = new PCG64(new BigInteger(42), new BigInteger(seq));
        if (power == 64) rng.Jump(); else rng.LongJump();
        foreach (ulong value in expected) Assert.Equal(value, rng.NextRaw64());
    }

    [Fact]
    public void State_Remains128BitsDuringGenerationAndAdvancement()
    {
        var rng = new PCG64(BigInteger.One << 200, BigInteger.One << 180);
        var state = typeof(PCG64).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var increment = typeof(PCG64).GetField("_inc", BindingFlags.Instance | BindingFlags.NonPublic)!;
        BigInteger max = (BigInteger.One << 128) - 1;
        Assert.InRange((BigInteger)increment.GetValue(rng)!, BigInteger.Zero, max);
        for (int i = 0; i < 1000; i++)
        {
            rng.NextRaw64();
            Assert.InRange((BigInteger)state.GetValue(rng)!, BigInteger.Zero, max);
        }
        rng.Jump();
        Assert.InRange((BigInteger)state.GetValue(rng)!, BigInteger.Zero, max);
        rng.LongJump();
        Assert.InRange((BigInteger)state.GetValue(rng)!, BigInteger.Zero, max);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(127)]
    [InlineData(1000)]
    public void Advance_EqualsRepeatedSteps(int steps)
    {
        var expected = new PCG64(42, 54);
        var actual = new PCG64(42, 54);
        for (int i = 0; i < steps; i++) expected.NextRaw64();
        actual.Advance(steps, Multiplier, 109);
        for (int i = 0; i < 20; i++) Assert.Equal(expected.NextRaw64(), actual.NextRaw64());
    }

    [Fact]
    public void Advance_RejectsNegativeDistanceWithoutChangingState()
    {
        var rng = new PCG64(42, 54);
        var expected = rng.Clone();
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Advance(-1, Multiplier, 109));
        Assert.Equal(expected.NextRaw64(), rng.NextRaw64());
    }
}
