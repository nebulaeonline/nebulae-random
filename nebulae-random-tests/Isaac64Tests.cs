using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nebulae.rng;

namespace nebulae.rng.tests
{
    public class Isaac64Tests
    {
        [Fact]
        public void Isaac64_Unseeded_ProducesCorrectReferenceSequence()
        {
            var rng = new Isaac64(true);
            ulong[] expected = new ulong[]
            {
                0xf67dfba498e4937c, 0x84a5066a9204f380,
                0xfee34bd5f5514dbb, 0x4d1664739b8f80d6,
                0x8607459ab52a14aa, 0x0e78bc5a98529e49,
                0xfe5332822ad13777, 0x556c27525e33d01a,
                0x08643ca615f3149f, 0xd0771faf3cb04714,
                0x30e86f68a37b008d, 0x3074ebc0488a3adf,
                0x270645ea7a2790bc, 0x5601a0a8d3763c6a,
                0x2f83071f53f325dd, 0xb9090f3d42d2d2ea
            };

            for (int i = 0; i < expected.Length; i++)
            {
                ulong actual = rng.Rand64();
                Assert.Equal(expected[i], actual);
            }
        }

        // Rust returns 32-bit values in opposite order to this implementation.
        // This is an inconsistency among implementations when returning
        // less than the full 64 bits of the state.
        // 32-bit values below are reversed for the test.
        [Fact]
        public void Isaac64_SeededByteArray_MatchesRustOutputsSwitched32()
        {
            byte[] seed = new byte[]
            {
                1,0,0,0,   0,0,0,0, 23,0,0,0,   0,0,0,0,
                200,1,0,0, 0,0,0,0, 210,30,0,0, 0,0,0,0
            };

            var rng = new Isaac64(seed);

            uint[] expected = new uint[]
            {
                3509106075, 3477963620, 1797495790, 687845478,
                2523132918, 227048253,  1260557630, 4044335064,
                3001306521, 4079741768,  3958365844, 69157722
            };

            for (int i = 0; i < expected.Length; i++)
            {
                uint actual = rng.Rand32();
                Assert.Equal(expected[i], actual);
            }
        }
    }
}
