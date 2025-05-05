using nebulae.rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nebulae.rng.tests
{
    public class GMWCTests
    {
        [Fact]
        public void GMWC128_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                4643040298910166833,  7438332832496527520,   537366322116350360, 12848019198184289481,
                15229605214807319001,  7815959442627237391,  9449115187610888370, 15368796834182872676,
                5899347731080822994, 17469704121265295232, 10251852947099714824,  6714564810959967705,
                7023558440727288789,    70580064213383996,  5406206288792803838,  4147532334982581860,
                13752329129153632705, 15233191864294447380,  6624685910780834298,  1435920653476504756
            };

            ulong seed = 0x12345678;

            GMWC128 rng = new GMWC128(seed);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }

            var ref_x = rng._x;
            var ref_c = rng._c;

            System.Diagnostics.Debug.WriteLine($"ref_x: 0x{ref_x:x}");
            System.Diagnostics.Debug.WriteLine($"ref_c: 0x{ref_c:x}");

            rng.Jump();
            ulong resultAfterJump = rng.Rand64();
            Assert.Equal(5556877514885060804UL, resultAfterJump);
        }

        [Fact]
        public void GMWC256_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                18416368405943230209, 18200753386591570122,  7828470588716469053, 10830402628629503432,
                9139302112296402245, 16886049655607986583, 15782671085417760542, 12261638243217425953,
                2775806068115598751, 14294808206951352177, 16701393622935691683, 10971953737262735386,
                12995709771114314677,  2362831617110781048,  3524990048390783180,  1650577526131218751,
                1814850751766907509,  8614432416301633630,  9905735967135498181, 15185014011040189189
            };

            ulong seed_lo = 0x12345678;
            ulong seed_mid = 0x98765432;
            ulong seed_hi = 0x54321987;

            GMWC256 rng = new GMWC256(seed_lo, seed_mid, seed_hi);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }

            var ref_x = rng._x;
            var ref_y = rng._y;
            var ref_z = rng._z;
            var ref_c = rng._c;

            System.Diagnostics.Debug.WriteLine($"ref_x: 0x{ref_x:x}");
            System.Diagnostics.Debug.WriteLine($"ref_c: 0x{ref_c:x}");

            rng.Jump();
            ulong resultAfterJump = rng.Rand64();
            Assert.Equal(10301111929018620435UL, resultAfterJump);

            rng._x = ref_x;
            rng._y = ref_y;
            rng._z = ref_z;
            rng._c = ref_c;

            rng.LongJump();
            ulong resultAfterLongJump = rng.Rand64();
            Assert.Equal(5070362387458137609UL, resultAfterLongJump);
        }
    }
}
