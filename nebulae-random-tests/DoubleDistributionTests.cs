using System;
using System.Collections.Generic;
using Xunit;
using nebulae.rng;

namespace nebulae.rng.tests
{
    public class DoubleDistributionTests
    {
        private const int NumSamples = 10_000_000;
        private const int NumBuckets = 100;
        private const double TolerancePercent = 0.05; // 5% wiggle room

        public static IEnumerable<object[]> AllNebulaeRngs()
        {
            yield return new object[] { "Xoshiro256++", new Xoshiro256plusplus() };
            yield return new object[] { "Xoshiro128++", new Xoshiro128plusplus() };
            yield return new object[] { "Xoshiro256**", new Xoshiro256starstar() };
            yield return new object[] { "Xoshiro256+", new Xoshiro256plus() };
            yield return new object[] { "Xoshiro128+", new Xoshiro128plus() };
            yield return new object[] { "Xoshiro128**", new Xoshiro128starstar() };
            yield return new object[] { "Xoshiro512++", new Xoshiro512plusplus() };
            yield return new object[] { "Xoshiro512+", new Xoshiro512plus() };
            yield return new object[] { "Xoshiro512**", new Xoshiro512starstar() };
            yield return new object[] { "Xoshiro1024++", new Xoshiro1024plusplus() };
            yield return new object[] { "Xoshiro1024*", new Xoshiro1024star() };
            yield return new object[] { "Xoshiro1024**", new Xoshiro1024starstar() };
            yield return new object[] { "PCG32", new PCG32() };
            yield return new object[] { "PCG64", new PCG64() };
            yield return new object[] { "ISAAC64", new Isaac64() };
            yield return new object[] { "MWC128", new MWC128() };
            yield return new object[] { "MWC192", new MWC192() };
            yield return new object[] { "MWC256", new MWC256() };
            yield return new object[] { "GMWC128", new GMWC128() };
            yield return new object[] { "GMWC128", new GMWC256() };
            yield return new object[] { "MT19937-32", new MT19937_32() };
            yield return new object[] { "MT19937-64", new MT19937_64() };
            yield return new object[] { "SplitMix64", new Splitmix() };
        }

        [Theory]
        [MemberData(nameof(AllNebulaeRngs))]
        public void RandDoubleExclusiveZero_ShouldDistributeEvenly(string name, BaseRng rng)
        {
            int[] buckets = new int[NumBuckets];

            for (int i = 0; i < NumSamples; i++)
            {
                double d = rng.RandDoubleExclusiveZero();
                int index = (int)(d * NumBuckets); // [0, NumBuckets - 1]
                if (index >= NumBuckets) index = NumBuckets - 1;
                buckets[index]++;
            }

            Console.WriteLine($"--- {name} ---");
            int expected = NumSamples / NumBuckets;
            int tolerance = (int)(expected * TolerancePercent);

            for (int i = 0; i < NumBuckets; i++)
            {
                Console.WriteLine($"Bucket {i:000}: {buckets[i]}");
                Assert.InRange(buckets[i], expected - tolerance, expected + tolerance);
            }
        }
    }
}