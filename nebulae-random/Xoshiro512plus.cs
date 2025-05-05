using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace nebulae.rng
{
    public class Xoshiro512plus : BaseRng
    {
        private readonly static ulong[] _jump_seeds = new ulong[]
        {
            0x33ed89b6e7a353f9,
            0x760083d7955323be,
            0x2837f2fbb5f22fae,
            0x4b8c5674d309511c,
            0xb11ac47a7ba28c25,
            0xf1be7667092bcc1c,
            0x53851efdb6df0aaf,
            0x1ebbc8b23eaf25db
        };

        private readonly static ulong[] _long_jump_seeds = new ulong[]
        {
            0x11467fef8f921d28,
            0xa2a819f2e79c8ea8,
            0xa8299fc284b3959a,
            0xb4d347340ca63ee1,
            0x1cb0940bedbff6ce,
            0xd956c5c4fa1f8e17,
            0x915e38fd4eda93bc,
            0x5b3ccdfa5d7daca5
        };

        private ulong[] _state = new ulong[8];

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
            Xoshiro512plus copy;

            lock (_lock)
            {
                copy = new Xoshiro512plus(); // testing constructor; does not reseed
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
        /// Xoshiro512plus() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 64 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public Xoshiro512plus()
        {
            Reseed();
        }

        /// <summary>
        /// Xoshiro512plus() constructs the rng object and seeds the rng with the given 8 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seed, as an array of 8 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreNot8ULongs">bool ignoreNot4ULongs - don't throw an exception if an undersized or oversized array is passed</param>
        /// <returns>the constructed & seeded rng</returns>
        public Xoshiro512plus(ulong[] seeds, bool ignoreNot8ULongs = false)
        {
            Reseed(seeds, ignoreNot8ULongs);
        }

        /// <summary>
        /// Xoshiro512plus() constructs the rng object and seeds the rng with the given 64 bytes. This constructor
        /// will throw if the given array is not 64 bytes long.
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of 64 bytes, to use to seed the rng</param>
        /// <returns>none</returns>
        public Xoshiro512plus(byte[] seed)
        {
            Reseed(seed);
        }

        /// <summary>
        /// Reseed() reseeds the rng object
        /// This variant uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 64 bytes of random data to seed the RNG.
        /// </summary>
        public override void Reseed()
        {
            byte[] bytes = new byte[64];
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
        /// Reseed() reseeds the rng object with the given 8 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 8 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreNot8ULongs">bool ignoreNot4ULongs - don't throw an exception if an undersized or oversized array is passed</param>
        public void Reseed(ulong[] seeds, bool ignoreNot8ULongs)
        {
            if (seeds.Length != 8 && !ignoreNot8ULongs)
                throw new ArgumentOutOfRangeException(nameof(seeds));

            lock (_lock)
            {
                for (int i = 0; i < Math.Min(seeds.Length, 8); ++i)
                {
                    _state[i] = seeds[i];
                }
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng with the given 64 bytes; this method will throw an exception if the array is not 64 bytes long
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of bytes, to use to seed the rng</param>
        public void Reseed(byte[] seed)
        {
            if (seed.Length != 64)
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
                var result = _state[0] + _state[2];

                var t = _state[1] << 11;

                _state[2] ^= _state[0];
                _state[5] ^= _state[1];
                _state[1] ^= _state[2];
                _state[7] ^= _state[3];
                _state[3] ^= _state[4];
                _state[4] ^= _state[5];
                _state[0] ^= _state[6];
                _state[6] ^= _state[7];

                _state[6] ^= t;

                _state[7] = rotl(_state[7], 21);

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
                                t[k] ^= _state[k];
                        }
                        NextRaw64();
                    }
                }

                for (int i = 0; i < _state.Length; ++i)
                    _state[i] = t[i];
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
                                t[k] ^= _state[k];
                        }
                        NextRaw64();
                    }
                }

                for (int i = 0; i < _state.Length; ++i)
                    _state[i] = t[i];
            }
        }
    }
}
