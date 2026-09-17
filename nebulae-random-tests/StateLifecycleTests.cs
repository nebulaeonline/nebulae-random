using System.Numerics;
using System.Reflection;
using nebulae.rng;

namespace nebulae.rng.tests;

public class StateLifecycleTests
{
    public static IEnumerable<object[]> Generators() => typeof(BaseRng).Assembly.GetTypes()
        .Where(t => t.IsSubclassOf(typeof(BaseRng)) && !t.IsAbstract)
        .Select(t => new object[] { t.Name });

    public static IEnumerable<object[]> JumpGenerators() => Generators()
        .Where(row => ((string)row[0]).StartsWith("Xoshiro")
            || ((string)row[0]).StartsWith("MWC")
            || ((string)row[0]).StartsWith("GMWC") || (string)row[0] == "PCG64")
        .SelectMany(row => (string)row[0] == "GMWC128"
            ? new[] { new object[] { row[0], false } }
            : new[] { new object[] { row[0], false }, new object[] { row[0], true } });

    private static (BaseRng Rng, Action Reseed) Create(string name)
    {
        Type type = typeof(BaseRng).Assembly.GetType("nebulae.rng." + name)!;
        MethodInfo reseed = type.GetMethods().First(m => m.Name == "Reseed"
            && m.GetParameters().Length > 0 && m.GetParameters()[0].ParameterType != typeof(byte[]));
        int count = name.StartsWith("Xoshiro")
            ? int.Parse(new string(name.Skip(7).TakeWhile(char.IsDigit).ToArray())) / 64 : 16;
        object[] args = reseed.GetParameters().Select(p => p.ParameterType == typeof(ulong[])
            ? (object)Enumerable.Range(1, count).Select(i => (ulong)i).ToArray()
            : p.ParameterType == typeof(bool) ? false
            : p.ParameterType == typeof(BigInteger) ? new BigInteger(42)
            : (object)42UL).ToArray();
        BaseRng rng = (BaseRng)Activator.CreateInstance(type, args)!;
        return (rng, () => reseed.Invoke(rng, args));
    }

    private static void FillBanks(BaseRng rng)
    {
        rng.Rand8();
        rng.Rand16();
        rng.Rand32();
    }

    private static void CompareMixed(BaseRng expected, BaseRng actual)
    {
        for (int i = 0; i < 40; i++)
        {
            Assert.Equal(expected.Rand8(), actual.Rand8());
            Assert.Equal(expected.Rand16(), actual.Rand16());
            Assert.Equal(expected.Rand32(), actual.Rand32());
            Assert.Equal(expected.Rand64(), actual.Rand64());
        }
    }

    [Theory]
    [MemberData(nameof(Generators))]
    public void Clone_PreservesCachedValuesAndOrder(string name)
    {
        var (rng, _) = Create(name);
        FillBanks(rng);
        CompareMixed(rng, (BaseRng)rng.Clone());
    }

    [Theory]
    [MemberData(nameof(Generators))]
    public void Clone_HasIndependentStateAcrossRefills(string name)
    {
        var (rng, _) = Create(name);
        var clone = rng.Clone();
        for (int i = 0; i < 700; i++)
            Assert.Equal(rng.NextRaw64(), clone.NextRaw64());
    }

    [Theory]
    [MemberData(nameof(Generators))]
    public void Reseed_ReplaysFreshInstanceIncludingCachedSizes(string name)
    {
        var (rng, reseed) = Create(name);
        var (fresh, _) = Create(name);
        for (int i = 0; i < 19; i++) rng.NextRaw64();
        FillBanks(rng);
        reseed();
        CompareMixed(fresh, rng);
    }

    [Theory]
    [MemberData(nameof(JumpGenerators))]
    public void Jump_DiscardsPreJumpCachedValues(string name, bool longJump)
    {
        var (rng, _) = Create(name);
        var (fresh, _) = Create(name);
        FillBanks(rng);
        // Match the three core draws used to populate the separate banks.
        for (int i = 0; i < 3; i++) fresh.NextRaw64();
        if (longJump) { rng.LongJump(); fresh.LongJump(); }
        else { rng.Jump(); fresh.Jump(); }
        CompareMixed(fresh, rng);
    }

    [Fact]
    public void Constructors_HonorAllowZeroSeed()
    {
        BaseRng[] rngs = { new PCG32(0, 0, true), new MWC128(0, true),
            new MWC192(0, 0, true), new MWC256(0, 0, 0, true),
            new GMWC128(0, true), new GMWC256(0, 0, 0, true) };
        foreach (var rng in rngs) rng.NextRaw64();
        new Splitmix(0, true).Clone();
    }
}
