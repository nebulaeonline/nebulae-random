using nebulae.rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nebulae.rng.tests
{
    public class XoshiroTests
    {
        [Fact]
        public void Xoshiro128plus_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x579bdf,
                0xaea8e43058853df,
                0xb1776039e6a4842d,
                0xf9a292a6a78b0683,
                0x38c596ac8aaed739,
                0x909e7885c5166a95,
                0x830270c2d14a549d,
                0xc9564021a258f29e,
                0x10a53adc31d66a2c,
                0x9d193c5c435fba2a,
                0xe8a9ce27bd9eaebb,
                0xd802a64d0f875490,
                0xd0e0715034a19fda,
                0x6ef2255d702edfd8,
                0xab7fc44169cbc5b5,
                0x18f93024fcc2cbc4,
                0xc15d68390985e7,
                0xc17e350061caa420,
                0x31529d43ce0106a5,
                0xf666ce671b2998ea,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL };

            Xoshiro128plus rng = new Xoshiro128plus(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro128plusplus_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0xaf37d03456,
                0x68fce25923ae255e,
                0xd0fa07825a290169,
                0x4425554a22cb9b4a,
                0x30ba98eb4a927577,
                0x7034777520e506e9,
                0xde68fc4a9db7e75a,
                0x53ce18f86823f37c,
                0x44b277bc5dfb718c,
                0xc70e3aec0aae1cfe,
                0xeaf99afe6c3c5acc,
                0x210dfdfc8baf0a58,
                0x623abfccf54b690a,
                0x4ea9a3ce086f91f6,
                0x2a6cad79160e8591,
                0x9275657a55e468dc,
                0x1f4a428d0ebb51b,
                0x4adebf404253f74,
                0xd4bc047885fff713,
                0xb948582201ecde1,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL };

            Xoshiro128plusplus rng = new Xoshiro128plusplus(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro128starstar_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x199998f00,
                0x19db3fc7b5f1980,
                0x9211c8122cdd8489,
                0x918b22c916d7ebdf,
                0x62f808d615d4c1fe,
                0xb2e5edc8ae93f573,
                0x7c057e4a28a1ba84,
                0x7ccde16a418da2b6,
                0x4c8772e42cb70d7d,
                0xc447fa4f7a3452db,
                0x44cb9535a4b8576f,
                0x6926145e2b4d5b6a,
                0x8fa8813504bbf88a,
                0xaab91d70275e1be4,
                0x8f9412af8c0a11de,
                0x31ad4cfdf060af8,
                0xda544c72cfad1bf7,
                0xf3390c3136a6bb06,
                0xb744637968475ae6,
                0x4bee09d881b8b4ac,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL };

            Xoshiro128starstar rng = new Xoshiro128starstar(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro256plus_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x777777,
                0x495000000321102,
                0xe946e092a0fd358a,
                0x8f12bcaeab9b500b,
                0x2dff125f34606ded,
                0x90411659c2704f3b,
                0xf6f45de3cf5342a1,
                0x3dcf05f781414e5f,
                0xfa321f1049879311,
                0xc1ac625d95b9ae13,
                0x743f7bf265edbbee,
                0x93e3d1a9a14c131,
                0x6c9576c0834fbf3d,
                0x7f158f94251659db,
                0x78d125b21a510903,
                0x1702f6ee728ba56a,
                0xc84b85bfbf19a1a5,
                0x94129ac3ad9a023c,
                0x1211c3c7fd4848d6,
                0x9eb6cf267d211f9c,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro256plus rng = new Xoshiro256plus(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro256plusplus_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x3bbbbb923456,
                0x190881345b7e,
                0x4de57e9ac671d8e1,
                0x377aadc0755fcb7d,
                0x79f66d470f565e92,
                0x75b0ce2eead58510,
                0x343366349afc3bee,
                0x9f6e0a61d63af84e,
                0xebe2cad764adbc7b,
                0x1aa6ae27e4cee419,
                0xdbc3f1a6f7b4a059,
                0xa797c85fe9b29625,
                0x1fd9e9e5c206dba9,
                0xb677f0416df39036,
                0x64b6213ce857d4c0,
                0xc9eda19d9328cfd5,
                0x676897e8e7e36e0b,
                0x20b4809a34d3eb3b,
                0x4eefcd66d4f5e1cb,
                0x2216233173f4017a,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro256plusplus rng = new Xoshiro256plusplus(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro256starstar_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x619998a80,
                0x1234ccb780,
                0xc333a6afff580,
                0x18a4699aab7df963,
                0xd99ac2dcb031a5c2,
                0x1782710ec886ddd2,
                0x653cabcca233577d,
                0x7a51463996ff7ee9,
                0x90a36309f9b96923,
                0xafe620f85d7cdadf,
                0xc369be1530bfaabf,
                0x71d3e226aec5a593,
                0x81ba5a33b0019f65,
                0x8ca81153c9e0780d,
                0x9e9db87c0c412692,
                0xad6cb6cf551ec503,
                0xc01065cd932e1127,
                0xa711c2ee4ab90112,
                0x9ef7f5b2d0059df,
                0xeca1e23cb58493a8,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro256starstar rng = new Xoshiro256starstar(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro512plus_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0xaaaaaa,
                0x1148404,
                0x22b2e7c56,
                0x8a4ffe9cb,
                0xeef0f86789a,
                0xddddd0e006004ddc,
                0xf2ea7d480b1e11c8,
                0x85b2dfcf072afb6b,
                0xf4d431cecfbc879c,
                0x2c5bf8128d67950d,
                0x8006da671e1df567,
                0x64210605fa52ee75,
                0xc3ecf3e8bcd053e2,
                0xeda1456f615e7066,
                0x82a7b73c865e211,
                0x5920055cd3653f30,
                0x3b1d32ec7a6fb23d,
                0x543c5f8c60f4b4e2,
                0x59740dff195487a3,
                0xb3d3258cd5b7b0c1,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro512plus rng = new Xoshiro512plus(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro512plusplus_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x15555ec7654,
                0x22908924202,
                0x4565cf8ac0000,
                0x114a01fec47c56,
                0x1dde1f1144331123,
                0xa1c01aef8a32320f,
                0xd86de55c1d9187b0,
                0x886f6f8ddfb8d995,
                0xd7cdbf1a05e2ccb3,
                0xe4b9285c59d5a051,
                0x782223445ce60af6,
                0x8bed08fcbafca8d9,
                0x83b05f9a6a167522,
                0x3eafaad418fd6712,
                0x80e8461ad5237f9a,
                0xfee31a3624c5cfcd,
                0xf6b8d779efdfb268,
                0xf9fb74b4ce33f299,
                0x3fb950b4a830d3c8,
                0x611d9d677a03adc9,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro512plusplus rng = new Xoshiro512plusplus(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro512starstar_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x619998a80,
                0x1234ccb780,
                0x1234ccb780,
                0x30db4e65ec80,
                0x91a5296a5300,
                0x14f714e7c81f100,
                0xfd8c3f388083e07a,
                0x9c20ebd19324a8d4,
                0xd6d1f9ea57abc88b,
                0x53b9dce2fd1b69a6,
                0x170ca47f7c6ef621,
                0xcec650e517485e16,
                0x48708553f1d8b22e,
                0x48b62d4c1fa6b812,
                0x57e4171dad796233,
                0xe8abb33d600d91ae,
                0x6637f82a30683b8f,
                0x932c8b297111f2b4,
                0x80bd2537b72e45a8,
                0x30bbd1e071645739,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro512starstar rng = new Xoshiro512starstar(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro1024star_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0xcca9158f87e70b2b,
                0xc27305c3c4e9783c,
                0xf031759cda91f773,
                0xfa6785689d8f8a62,
                0xcca9158f87e70b2b,
                0xc27305c3c4e9783c,
                0xf031759cda91f773,
                0xfa6785689d8f8a62,
                0xcca9158f87e70b2b,
                0xc27305c3c4e9783c,
                0xf031759cda91f773,
                0xfa6785689d8f8a62,
                0xcca9158f87e70b2b,
                0xc27305c3c4e9783c,
                0xf031759cda91f773,
                0xf129569790913d8d,
                0x2a34f88e5ce9783c,
                0xe8ec241f379273,
                0xfc358b636f195a29,
                0xcc4f347db463b80,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro1024star rng = new Xoshiro1024star(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro1024starstar_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x619998a80,
                0xd66666200,
                0x8e6666680,
                0x199998f00,
                0x619998a80,
                0xd66666200,
                0x8e6666680,
                0x199998f00,
                0x619998a80,
                0xd66666200,
                0x8e6666680,
                0x199998f00,
                0x619998a80,
                0xd66666200,
                0x8e6666680,
                0x313f3318acdf1980,
                0x6a2a4bc1666664b5,
                0xf562f0114cb8e77c,
                0x825b4826d2774f43,
                0x9e22713d18b24117,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro1024starstar rng = new Xoshiro1024starstar(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }

        [Fact]
        public void Xoshiro1024plusplus_GeneratesCorrectReferenceSequence()
        {
            ulong[] expected = new ulong[]
            {
                0x2bcdef923456,
                0xfd758a2b2a02ba9e,
                0xa9b341d167d8a2b2,
                0x7bb60654c63008e6,
                0x21e175fb7fdf440b,
                0x71c6e46d48171da7,
                0x8aef2858acee41af,
                0xccb85622d9ee458a,
                0x6003384090055fe8,
                0x6c87e2412a2f7198,
                0xaa786f6858fd20f1,
                0x7e1e0a24d282f265,
                0x29520c6ba660a20b,
                0xe9bca48dbf207499,
                0xe98219802c414276,
                0xdab56118879d4508,
                0x744b6d34bed6c5bb,
                0xf83947cb1e275835,
                0x6f3291f05baca401,
                0x695003ab7e2ca7b9,
            };

            ulong[] seeds = new ulong[] { 0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL,
                                          0x123456UL, 0x456789UL,
                                          0x987654UL, 0x654321UL };

            Xoshiro1024plusplus rng = new Xoshiro1024plusplus(seeds);

            for (int i = 0; i < expected.Length; i++)
            {
                ulong result = rng.Rand64();
                Assert.Equal(expected[i], result);
            }
        }
    }
}
