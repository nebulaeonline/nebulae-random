using nebulae.rng;

namespace nebulae.rng.tests
{
    public class PCGTests
    {
        [Fact]
        public void PCG32_GeneratesCorrectReferenceSequence()
        {
            uint[] expected = new uint[]
            {
                2098444299, 3146305294, 724141107, 3646777727, 4146451631,
                786350529, 1390359870, 470195731, 3999409732, 4100632749,
                2848297225, 1330528224, 1965167708, 2732630254, 2670843380,
                1016216922, 953070094, 77203014, 2081414551, 1418079917
            };

            PCG32 rng = new PCG32(0x12345678, 0x98765432);

            for (int i = 0; i < expected.Length; i++)
            {
                uint result = rng.Rand32();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void PCG64_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                5328383251916423251, 596178232259590883, 8764121873367028933, 9492206962425681309,
                9153906665090117815, 13974427569661460936, 18322412226341010040, 18333359210874473496,
                6090299682901593761, 3906321649278259414, 8325968342092680202, 10870654155381920833,
                5507867331972246793, 309940337170307040, 7301322697712292963, 10076312753340422991,
                9568986605778981995, 17813311259234958731, 11262602361483463160, 13237019671311812050
            };

            PCG64 rng = new PCG64(0, 42UL, 0, 0x8000000000000054UL);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }
    }
}