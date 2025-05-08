using nebulae.rng;
using System;
using System.Collections.Generic;
using Xunit;

namespace nebulae.rng.tests
{
    public class IntegerDistributionTests
    {
        private const int NumSamples = 10_000_000;
        private const int NumBuckets = 100;
        private const long MinValue = -5000;
        private const long MaxValue = 4999;
        private const double TolerancePercent = 1.2;

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
            yield return new object[] { "GMWC256", new GMWC256() };
            yield return new object[] { "MT19937-32", new MT19937_32() };
            yield return new object[] { "MT19937-64", new MT19937_64() };
            yield return new object[] { "SplitMix64", new Splitmix() };
        }

        [Theory]
        [MemberData(nameof(AllNebulaeRngs))]
        public void IntegerRangeBucketTest_IsEvenlyDistributed(string name, BaseRng rng)
        {
            int[] buckets = new int[NumBuckets];
            long rangeSize = MaxValue - MinValue + 1;
            long bucketSize = rangeSize / NumBuckets;

            for (int i = 0; i < NumSamples; i++)
            {
                long val = rng.RangedRand64S(MinValue, MaxValue);
                int index = (int)((val - MinValue) / bucketSize);
                if (index >= NumBuckets) index = NumBuckets - 1;
                buckets[index]++;
            }

            Console.WriteLine($"--- {name} ---");
            int expected = NumSamples / NumBuckets;
            int tolerance = (int)(expected * TolerancePercent / 100.0);

            int maxDeviation = 0;

            for (int i = 0; i < NumBuckets; i++)
            {
                int deviation = Math.Abs(buckets[i] - expected);
                if (deviation > maxDeviation) maxDeviation = deviation;

                Console.WriteLine($"Bucket {i:000}: {buckets[i]}");
                Assert.InRange(buckets[i], expected - tolerance, expected + tolerance);
            }

            double pct = 100.0 * maxDeviation / expected;
            Console.WriteLine($"Max deviation for {name}: {maxDeviation} samples ({pct:F2}%)");
        }
    }
}