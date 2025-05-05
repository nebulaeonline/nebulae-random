using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace nebulae.rng
{
    public class Xoshiro256plusplus : BaseRng
    {
        private readonly static ulong[] _jump_seeds = new ulong[]
        {
            0x180ec6d33cfd0aba,
            0xd5a61266f0c9392c,
            0xa9582618e03fc9aa,
            0x39abdc4529b1661c,
        };
        
        private readonly static ulong[] _long_jump_seeds = new ulong[]
        {
            0x76e15d3efefdcbbf,
            0xc5004e441c522fb3,
            0x77710069854ee241,
            0x39109bb02acbe635,
        };

        private ulong[] _state = new ulong[4];

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
            Xoshiro256plusplus copy;

            lock (_lock)
            {
                copy = new Xoshiro256plusplus(); // testing constructor; does not reseed
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
        /// Xoshiro256plusplus() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 32 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public Xoshiro256plusplus()
        {
            Reseed();
        }

        /// <summary>
        /// Xoshiro256plusplus() constructs the rng object and seeds the rng with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 4 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreNot4ULongs">bool ignoreNot4ULongs - don't throw an exception if an undersized or oversized array is passed</param>
        /// <returns>the constructed & seeded rng</returns>
        public Xoshiro256plusplus(ulong[] seeds, bool ignoreNot4ULongs = false)
        {
            Reseed(seeds, ignoreNot4ULongs);
        }

        /// <summary>
        /// Xoshiro256plusplus() constructs the rng object and seeds the rng with the given 32 bytes. This constructor
        /// will throw if the given array is not 32 bytes long.
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of 32 bytes, to use to seed the rng</param>
        /// <returns>none</returns>
        public Xoshiro256plusplus(byte[] seed)
        {
            Reseed(seed);
        }

        /// <summary>
        /// Xoshiro256plusplus() constructs the rng object and seeds the rng object with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seed1">ulong[] seed1 - the first seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="seed2">ulong[] seed2 - the second seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="seed3">ulong[] seed3 - the third seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="seed4">ulong[] seed4 - the fourth seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        public Xoshiro256plusplus(ulong seed1, ulong seed2, ulong seed3, ulong seed4)
        {
            Reseed(seed1, seed2, seed3, seed4);
        }

        /// <summary>
        /// Reseed() reseeds the rng object
        /// This variant uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 32 bytes of random data to seed the RNG.
        /// </summary>
        public override void Reseed()
        {
            byte[] bytes = new byte[32];
#if NET6_0_OR_GREATER
            bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
#else
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
#endif
            lock (_lock)
            {
                var bytes_array = MemoryMarshal.Cast<byte, ulong>(bytes);
                _state[0] = bytes_array[0];
                _state[1] = bytes_array[1];
                _state[2] = bytes_array[2];
                _state[3] = bytes_array[3];
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng object with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 4 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreNot4ULongs">bool ignoreNot4ULongs - don't throw an exception if an undersized or oversized array is passed</param>
        public void Reseed(ulong[] seeds, bool ignoreNot4ULongs = false)
        {
            if (seeds.Length != 4 && !ignoreNot4ULongs)
                throw new ArgumentOutOfRangeException(nameof(seeds));

            lock (_lock)
            {
                for (int i = 0; i < Math.Min(seeds.Length, 4); ++i)
                {
                    _state[i] = seeds[i];
                }
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng with the given 32 bytes; this method will throw an exception if the array is not 32 bytes long
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of bytes, to use to seed the rng</param>
        public void Reseed(byte[] seed)
        {
            if (seed.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(seed));

            lock (_lock)
            {
                var bytes_array = MemoryMarshal.Cast<byte, ulong>(seed);
                _state[0] = bytes_array[0];
                _state[1] = bytes_array[1];
                _state[2] = bytes_array[2];
                _state[3] = bytes_array[3];
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng object with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seed1">ulong[] seed1 - the first seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="seed2">ulong[] seed2 - the second seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="seed3">ulong[] seed3 - the third seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="seed4">ulong[] seed4 - the fourth seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        public void Reseed(ulong seed1, ulong seed2, ulong seed3, ulong seed4)
        {
            lock (_lock)
            {
                _state[0] = seed1;
                _state[1] = seed2;
                _state[2] = seed3;
                _state[3] = seed4;
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
                var result = rotl(_state[0] + _state[3], 23) + _state[0];

                var t = _state[1] << 17;

                _state[2] ^= _state[0];
                _state[3] ^= _state[1];
                _state[1] ^= _state[2];
                _state[0] ^= _state[3];

                _state[2] ^= t;

                _state[3] = rotl(_state[3], 45);

                return result;
            }
        }

        /// <summary>
        /// Jump() moves the RNG sequence ahead by 2^64 steps.
        /// </summary>
        public override void Jump()
        {
            lock (_lock)
            {
                ulong s0, s1, s2, s3;
                s0 = s1 = s2 = s3 = 0;

                for (int i = 0; i < _jump_seeds.Length; ++i)
                {
                    for (int j = 0; j < 64; ++j)
                    {
                        if ((_jump_seeds[i] & (1UL << j)) > 0)
                        {
                            s0 ^= _state[0];
                            s1 ^= _state[1];
                            s2 ^= _state[2];
                            s3 ^= _state[3];
                        }
                        NextRaw64();
                    }
                }

                _state[0] = s0;
                _state[1] = s1;
                _state[2] = s2;
                _state[3] = s3;
            }
        }

        /// <summary>
        /// LongJump() moves the RNG sequence ahead by 2^192 steps.
        /// </summary>
        public override void LongJump()
        {
            lock (_lock)
            {
                ulong s0, s1, s2, s3;
                s0 = s1 = s2 = s3 = 0;

                for (int i = 0; i < _long_jump_seeds.Length; ++i)
                {
                    for (int j = 0; j < 64; ++j)
                    {
                        if ((_long_jump_seeds[i] & (1UL << j)) > 0)
                        {
                            s0 ^= _state[0];
                            s1 ^= _state[1];
                            s2 ^= _state[2];
                            s3 ^= _state[3];
                        }
                        NextRaw64();
                    }
                }

                _state[0] = s0;
                _state[1] = s1;
                _state[2] = s2;
                _state[3] = s3;
            }
        }
    }
}
