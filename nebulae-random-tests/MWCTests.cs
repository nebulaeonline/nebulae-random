using nebulae.rng;
using System.Diagnostics.Metrics;

namespace nebulae.rng.tests
{
    public class MWCTests
    {
        [Fact]
        public void MWC128_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                1311768465173141112, 12166362336649169593,  3949029528295382307,  5472733716351113021,
                5379016874051839003,  1982326862696084190,  5482295573486842119,  5337654010745106096,
                6135862836935752226,  4458998714689406838, 13048659348736460409,  7118826115067147959,
                10450767410111843800,  1131896256380239952,  1589014920458935972,  4189216939855360452,
                12617002536535744125,  3055206049927487239,  5490834897945824335, 10802102197168152769
            };

            ulong seed = 0x12345678;

            MWC128 rng = new MWC128(seed);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }

            rng.Jump();
            ulong resultAfterJump = rng.Rand64();
            Assert.Equal((ulong)3587780927188566940, resultAfterJump);

            rng.LongJump();
            ulong resultAfterLongJump = rng.Rand64();
            Assert.Equal((ulong)6273574218713948847, resultAfterLongJump);
        }

        [Fact]
        public void MWC192_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                2557891634, 16514775134394309329, 4437670264166532246, 4804456986524175594,
                15077121723280790481, 7234228866838282699, 11044702191231888499, 3263177286270032477,
                15031385415279136345, 2169084651050194966, 18231406271267056249, 10158124348709651043,
                186695642781502540, 8123591154666173055, 9330651374171667693, 11780771869901694595,
                15060517721161687592, 1586606093336966093, 2643691478544008935, 16295828131054806409
            };

            ulong seed_lo = 0x12345678;
            ulong seed_hi = 0x98765432;

            MWC192 rng = new MWC192(seed_lo, seed_hi);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }

            rng.Jump();
            ulong resultAfterJump = rng.Rand64();
            Assert.Equal((ulong)3329848198669835316, resultAfterJump);

            rng.LongJump();
            ulong resultAfterLongJump = rng.Rand64();
            Assert.Equal((ulong)9596496225976159432, resultAfterLongJump);
        }

        [Fact]
        public void MWC256_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                1412569479, 16300154572754990089, 11984445407397886924,  8633588703993858208,
                7204641541114602881,  5892165249881301570,  5471404443569471188, 11394308040579544152,
                3441937441101395703, 16876631080940656862,  5679844440163987509, 14465521718254593449,
                13964815562053663553,  3093173600497978328, 17322286554016672504,  8964837844043810983,
                14085062776019137086,  4854538681330747028,  1981384948678516883,  5625232156809145292
            };

            ulong seed_lo = 0x12345678;
            ulong seed_mid = 0x98765432;
            ulong seed_hi = 0x54321987;

            MWC256 rng = new MWC256(seed_lo, seed_mid, seed_hi);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }

            rng.Jump();
            ulong resultAfterJump = rng.Rand64();
            Assert.Equal((ulong)6457902794947341525, resultAfterJump);

            rng.LongJump();
            ulong resultAfterLongJump = rng.Rand64();
            Assert.Equal((ulong)14980215816572406489, resultAfterLongJump);
        }
    }
}