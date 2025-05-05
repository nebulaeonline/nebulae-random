using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace nebulae.rng
{
    public class Xoshiro128starstar : BaseRng
    {
        private readonly static ulong[] _jump_seeds = new ulong[]
        {
            0xdf900294d8f554a5,
            0x170865df4b3201fc,
        };

        private readonly static ulong[] _long_jump_seeds = new ulong[]
        {
            0xd2a98b26625eee7b,
            0xdddf9b1090aa7ac1,
        };

        private ulong[] _state = new ulong[2];

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
            Xoshiro128starstar copy;

            lock (_lock)
            {
                copy = new Xoshiro128starstar(); // testing constructor; does not reseed
                copy._state[0] = this._state[0];
                copy._state[1] = this._state[1];

                copy._banked8 = new ConcurrentStack<byte>(this._banked8);
                copy._banked16 = new ConcurrentStack<ushort>(this._banked16);
                copy._banked32 = new ConcurrentStack<uint>(this._banked32);
            }
            return copy;
        }

        /// <summary>
        /// Xoshiro128starstar() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 16 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public Xoshiro128starstar()
        {
            Reseed();
        }

        /// <summary>
        /// Xoshiro128starstar() constructs the rng object and seeds the rng with the given 2 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 2 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreNot2ULongs">bool ignoreNot2ULongs - don't throw an exception if an undersized or oversized array is passed</param>
        /// <returns>the constructed & seeded rng</returns>
        public Xoshiro128starstar(ulong[] seeds, bool ignoreNot2ULongs = false)
        {
            Reseed(seeds, ignoreNot2ULongs);
        }

        /// <summary>
        /// Xoshiro128starstar() constructs the rng object and seeds the rng with the given 16 bytes. This constructor
        /// will throw if the given array is not 16 bytes long.
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of 16 bytes, to use to seed the rng</param>
        /// <returns>none</returns>
        public Xoshiro128starstar(byte[] seed)
        {
            Reseed(seed);
        }

        /// <summary>
        /// Xoshiro128starstar() constructs the rng object and seeds the rng object with the given 2 64-bit unsigned integers
        /// </summary>
        /// <param name="seed1">ulong[] seed1 - the first seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="seed2">ulong[] seed2 - the second seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        public Xoshiro128starstar(ulong seed1, ulong seed2)
        {
            Reseed(seed1, seed2);
        }

        /// <summary>
        /// Reseed() reseeds the rng object
        /// This variant uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 16 bytes of random data to seed the RNG.
        /// </summary>
        public override void Reseed()
        {
            byte[] bytes = new byte[16];
#if NET6_0_OR_GREATER
            bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
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
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng object with the given 2 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 2 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreNot2ULongs">bool ignoreNot2ULongs - don't throw an exception if an undersized or oversized array is passed</param>
        public void Reseed(ulong[] seeds, bool ignoreNot2ULongs = false)
        {
            if (seeds.Length != 2 && !ignoreNot2ULongs)
                throw new ArgumentOutOfRangeException(nameof(seeds));

            lock (_lock)
            {
                for (int i = 0; i < Math.Min(seeds.Length, 2); ++i)
                {
                    _state[i] = seeds[i];
                }
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng with the given 16 bytes; this method will throw an exception if the array is not 16 bytes long
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of bytes, to use to seed the rng</param>
        public void Reseed(byte[] seed)
        {
            if (seed.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(seed));

            lock (_lock)
            {
                var bytes_array = MemoryMarshal.Cast<byte, ulong>(seed);
                _state[0] = bytes_array[0];
                _state[1] = bytes_array[1];
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng object with the given 2 64-bit unsigned integers
        /// </summary>
        /// <param name="seed1">ulong[] seed1 - the first seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="seed2">ulong[] seed2 - the second seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        public void Reseed(ulong seed1, ulong seed2)
        {
            lock (_lock)
            {
                _state[0] = seed1;
                _state[1] = seed2;
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
                var s0 = _state[0];
                var s1 = _state[1];
                var result = rotl(s0 * 5, 7) * 9;

                s1 ^= s0;
                _state[0] = rotl(s0, 24) ^ s1 ^ (s1 << 16); // a, b
                _state[1] = rotl(s1, 37); // c

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
                ulong s0, s1;
                s0 = s1 = 0;

                for (int i = 0; i < _jump_seeds.Length; ++i)
                {
                    for (int j = 0; j < 64; ++j)
                    {
                        if ((_jump_seeds[i] & (1UL << j)) > 0)
                        {
                            s0 ^= _state[0];
                            s1 ^= _state[1];
                        }
                        NextRaw64();
                    }
                }

                _state[0] = s0;
                _state[1] = s1;
            }
        }

        /// <summary>
        /// LongJump() moves the RNG sequence ahead by 2^96 steps.
        /// </summary>
        public override void LongJump()
        {
            lock (_lock)
            {
                ulong s0, s1;
                s0 = s1 = 0;

                for (int i = 0; i < _long_jump_seeds.Length; ++i)
                {
                    for (int j = 0; j < 64; ++j)
                    {
                        if ((_long_jump_seeds[i] & (1UL << j)) > 0)
                        {
                            s0 ^= _state[0];
                            s1 ^= _state[1];
                        }
                        NextRaw64();
                    }
                }

                _state[0] = s0;
                _state[1] = s1;
            }
        }
    }
}
