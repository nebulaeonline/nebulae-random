using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace nebulae.rng
{
    public class Xoshiro1024star : BaseRng
    {
        private readonly static ulong[] _jump_seeds = new ulong[]
        {
            0x931197d8e3177f17,
            0xb59422e0b9138c5f,
            0xf06a6afb49d668bb,
            0xacb8a6412c8a1401,
            0x12304ec85f0b3468,
            0xb7dfe7079209891e,
            0x405b7eec77d9eb14,
            0x34ead68280c44e4a,
            0xe0e4ba3e0ac9e366,
            0x8f46eda8348905b7,
            0x328bf4dbad90d6ff,
            0xc8fd6fb31c9effc3,
            0xe899d452d4b67652,
            0x45f387286ade3205,
            0x03864f454a8920bd,
            0xa68fa28725b1b384
        };

        private readonly static ulong[] _long_jump_seeds = new ulong[]
        {
            0x7374156360bbf00f,
            0x4630c2efa3b3c1f6,
            0x6654183a892786b1,
            0x94f7bfcbfb0f1661,
            0x27d8243d3d13eb2d,
            0x9701730f3dfb300f,
            0x2f293baae6f604ad,
            0xa661831cb60cd8b6,
            0x68280c77d9fe008c,
            0x50554160f5ba9459,
            0x2fc20b17ec7b2a9a,
            0x49189bbdc8ec9f8f,
            0x92a65bca41852cc1,
            0xf46820dd0509c12a,
            0x52b00c35fbf92185,
            0x1e5b3b7f589e03c1
        };

        private ulong[] _state = new ulong[16];
        private int _p = 0;

        // concurrency lock
        private readonly object _lock = new object();

        /// <summary>
        /// Clone() clones the internal context of the rng object and returns a new rng object
        /// This is useful to split the same rng object into multiple rng objects to take
        /// two different paths, but maintain the same repeatability if using known
        /// seeds. This is useful for testing & simulation purposes.
        /// </summary>
        /// <returns>A newly constructed rng with the same internal state as the instance
        /// Clone() was called on</returns>
        public override INebulaeRng Clone()
        {
            Xoshiro1024star copy;

            lock (_lock)
            {
                copy = new Xoshiro1024star(); // testing constructor; does not reseed

                copy._p = _p;
                for (int i = 0; i < _state.Length; ++i)
                {
                    copy._state[i] = this._state[i];
                }

                copy._banked8 = new ConcurrentStack<byte>(this._banked8);
                copy._banked16 = new ConcurrentStack<ushort>(this._banked16);
                copy._banked32 = new ConcurrentStack<uint>(this._banked32);
            }
            return copy;
        }

        /// <summary>
        /// Xoshiro1024star() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public Xoshiro1024star()
        {
            Reseed();
        }

        /// <summary>
        /// Xoshiro1024star() constructs the rng object and seeds the rng with the given 16 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 16 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreNot8ULongs">bool ignoreNot4ULongs - don't throw an exception if an undersized or oversized array is passed</param>
        /// <returns>the constructed & seeded rng</returns>
        public Xoshiro1024star(ulong[] seeds, bool ignoreNot8ULongs = false)
        {
            Reseed(seeds, ignoreNot8ULongs);
        }

        /// <summary>
        /// Xoshiro1024star() constructs the rng object and seeds the rng with the given 128 bytes. This constructor
        /// will throw if the given array is not 128 bytes long.
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of 128 bytes, to use to seed the rng</param>
        /// <returns>none</returns>
        public Xoshiro1024star(byte[] seed)
        {
            Reseed(seed);
        }

        /// <summary>
        /// Reseed() reseeds the rng object
        /// This variant uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        public override void Reseed()
        {
            byte[] bytes = new byte[128];
#if NET6_0_OR_GREATER
            bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(64);
#else
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
#endif
            lock (_lock)
            {
                var bytes_array = MemoryMarshal.Cast<byte, ulong>(bytes);

                for (int i = 0; i < _state.Length; ++i)
                {
                    _state[i] = bytes_array[i];
                }
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng object with the given 16 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 16 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreNot16ULongs">bool ignoreNot4ULongs - don't throw an exception if an undersized or oversized array is passed</param>
        public void Reseed(ulong[] seeds, bool ignoreNot16ULongs = false)
        {
            if (seeds.Length != 16 && !ignoreNot16ULongs)
                throw new ArgumentOutOfRangeException(nameof(seeds));

            lock (_lock)
            {
                for (int i = 0; i < Math.Min(seeds.Length, 16); ++i)
                {
                    _state[i] = seeds[i];
                }
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng with the given 128 bytes; this method will throw an exception if the array is not 128 bytes long
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of bytes, to use to seed the rng</param>
        public void Reseed(byte[] seed)
        {
            if (seed.Length != 128)
                throw new ArgumentOutOfRangeException(nameof(seed));

            lock (_lock)
            {
                var bytes_array = MemoryMarshal.Cast<byte, ulong>(seed);
                for (int i = 0; i < _state.Length; ++i)
                {
                    _state[i] = bytes_array[i];
                }
            }
        }

        private ulong rotl(ulong x, int k)
        {
            return (x << k) | (x >> (64 - k));
        }

        /// <summary>
        /// NextRaw64() returns an unsigned 64-bit integer in the range [0, Max] (inclusive)
        /// Public, but not for general consumption although it is safe to do so
        /// </summary>
        /// <returns>ulong</returns>
        public override ulong NextRaw64()
        {
            lock (_lock)
            {
                int q = _p;
                _p = (_p + 1) & 15;
                ulong s0 = _state[_p];
                ulong s15 = _state[q];

                var result = s0 * 0x9e3779b97f4a7c13;

                s15 ^= s0;
                _state[q] = rotl(s0, 25) ^ s15 ^ (s15 << 27);
                _state[_p] = rotl(s15, 36);

                return result;
            }
        }

        /// <summary>
        /// Jump() moves the RNG sequence ahead by 2^256 steps.
        /// </summary>
        public override void Jump()
        {

            lock (_lock)
            {
                ulong[] t = new ulong[8];

                for (int i = 0; i < _jump_seeds.Length; ++i)
                {
                    for (int j = 0; j < 64; ++j)
                    {
                        if ((_jump_seeds[i] & (1UL << j)) > 0)
                        {
                            for (int k = 0; k < _state.Length; ++k)
                                t[k] ^= _state[(k + _p) & 15];
                        }
                        NextRaw64();
                    }
                }

                for (int i = 0; i < _state.Length; ++i)
                    _state[(i + _p) & 15] = t[i];
            }
        }

        /// <summary>
        /// LongJump() moves the RNG sequence ahead by 2^384 steps.
        /// </summary>
        public override void LongJump()
        {
            lock (_lock)
            {
                ulong[] t = new ulong[8];

                for (int i = 0; i < _long_jump_seeds.Length; ++i)
                {
                    for (int j = 0; j < 64; ++j)
                    {
                        if ((_long_jump_seeds[i] & (1UL << j)) > 0)
                        {
                            for (int k = 0; k < _state.Length; ++k)
                                t[k] ^= _state[(k + _p) & 15];
                        }
                        NextRaw64();
                    }
                }

                for (int i = 0; i < _state.Length; ++i)
                    _state[(i + _p) & 15] = t[i];
            }
        }
    }
}
