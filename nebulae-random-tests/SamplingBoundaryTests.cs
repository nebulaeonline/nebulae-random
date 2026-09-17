using nebulae.rng;

namespace nebulae.rng.tests;

public class SamplingBoundaryTests
{
    private sealed class ScriptedRng(params ulong[] values) : BaseRng
    {
        private readonly Queue<ulong> words = new(values);
        public override ulong NextRaw64() => words.Dequeue();
        public override void Reseed() => throw new NotSupportedException();
        public override INebulaeRng Clone() => throw new NotSupportedException();
        public override void Jump() => throw new NotSupportedException();
        public override void LongJump() => throw new NotSupportedException();
    }

    [Fact]
    public void BoundedUnsigned_RejectsIncompleteModuloBucket()
    {
        Assert.Equal(1UL, new ScriptedRng(ulong.MaxValue, 7).Rand64(5));
        Assert.Equal(1U, new ScriptedRng(0xFFFFFFFF00000007).Rand32(5));
        Assert.Equal((ushort)1, new ScriptedRng(0xFFFF000700000000).Rand16(5));
        Assert.Equal((byte)1, new ScriptedRng(0xFF07000000000000).Rand8(5));
    }

    [Fact]
    public void BoundedBytes_HaveEqualCountsOverAcceptedDomain()
    {
        // 252 accepted byte values: exactly 42 occurrences of each result in [0, 5].
        ulong[] words = Enumerable.Range(0, 32).Select(i =>
            Enumerable.Range(0, 8).Aggregate(0UL, (word, j) => (word << 8) | (byte)(i * 8 + j))).ToArray();
        var rng = new ScriptedRng(words);
        int[] counts = new int[6];
        for (int i = 0; i < 252; i++) counts[rng.Rand8(5)]++;
        Assert.All(counts, count => Assert.Equal(42, count));
    }

    [Fact]
    public void Signed32_WideRangeDoesNotOverflowOrLoop()
    {
        Assert.Equal(int.MinValue, new ScriptedRng(0).RangedRand32S(int.MinValue, 0));
        Assert.Equal(0, new ScriptedRng(0x8000000000000000).RangedRand32S(int.MinValue, 0));
        Assert.Equal(int.MaxValue, new ScriptedRng(ulong.MaxValue).RangedRand32S(int.MinValue, int.MaxValue));
    }

    [Fact]
    public void Next_HandlesZeroAndOneBounds()
    {
        Assert.Equal(0, new ScriptedRng().Next(0));
        Assert.Equal(0, new ScriptedRng(ulong.MaxValue).Next(1));
    }

    [Fact]
    public void Next_ExcludesIntMaxAndRejectsReversedBounds()
    {
        Assert.InRange(new ScriptedRng(ulong.MaxValue, 0).Next(), 0, int.MaxValue - 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ScriptedRng().Next(2, 1));
    }

    [Fact]
    public void CharacterSampling_SupportsMoreThan256Entries()
    {
        char[] symbols = Enumerable.Range(0, 300).Select(i => (char)i).ToArray();
        Assert.Equal(symbols[299], new ScriptedRng(299UL << 32)
            .RandAlphaNum(false, false, false, symbols));
    }

    [Fact]
    public void ExclusiveDouble_RejectsZeroAndUses53Bits()
    {
        Assert.Equal(1.0 / (1UL << 53), new ScriptedRng(0, 1UL << 11).RandDoubleExclusiveZero());
        Assert.Equal(1.0 - 1.0 / (1UL << 53), new ScriptedRng(ulong.MaxValue).RandDoubleExclusiveZero());
    }
}
