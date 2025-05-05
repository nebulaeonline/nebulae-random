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
    }
}