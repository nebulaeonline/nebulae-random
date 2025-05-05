using nebulae.rng;

namespace nebulae.rng.tests
{
    public class MT19937Tests
    {
        [Fact]
        public void MT19937_32_GeneratesCorrectReferenceSequence()
        {
            uint[] expected = new uint[]
            {
                1067595299,  955945823,  477289528, 4107218783, 4228976476,
                3344332714, 3355579695,  227628506,  810200273, 2591290167,
                2560260675, 3242736208,  646746669, 1479517882, 4245472273,
                1143372638, 3863670494, 3221021970, 1773610557, 1138697238
            };

            ulong[] seeds = new ulong[] { 0x123, 0x234, 0x345, 0x456 };

            MT19937_32 rng = new MT19937_32(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                uint result = rng.Rand32();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void MT19937_64_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                7266447313870364031, 4946485549665804864, 16945909448695747420, 16394063075524226720,
                4873882236456199058, 14877448043947020171, 6740343660852211943, 13857871200353263164,
                5249110015610582907, 10205081126064480383, 1235879089597390050, 17320312680810499042,
                16489141110565194782, 8942268601720066061, 13520575722002588570, 14226945236717732373,
                9383926873555417063, 15690281668532552105, 11510704754157191257, 15864264574919463609
            };

            ulong[] seeds = new ulong[] { 0x12345UL, 0x23456UL, 0x34567UL, 0x45678UL };

            MT19937_64 rng = new MT19937_64(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }
    }
}