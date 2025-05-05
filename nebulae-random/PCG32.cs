using System;
using System.Security.Cryptography;

using System.Numerics;

namespace nebulae.rng
{
    public class PCG32 : BaseRng
    {
        private const ulong PCG_STATE_MULT = 6364136223846793005UL;

        private ulong _state;
        private ulong _inc;

        private static readonly object _lock = new object();

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
            lock (_lock)
            {
                PCG32 clone = new PCG32();
                clone._state = _state;
                clone._inc = _inc;
                return clone;
            }
        }

        /// <summary>
        /// PCG32() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public PCG32()
        {
            Reseed();
        }

        /// <summary>
        /// PCG32() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seed">ulong seed - the 64-bit seed to use to seed the rng</param>
        /// <param name="seq">ulong seq - the 64-bits sequence selector to use to seed the rng</param>
        /// <param name="ignoreZeroSeed">bool IgnoreZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        public PCG32(ulong seed, ulong seq, bool allowZeroSeed = false)
        {
            Reseed(seed, seq);
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// This variant System.Security.Cryptography.RandomNumberGenerator
        /// component to get 16 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public override void Reseed()
        {
            ulong[] seed = new ulong[2];

            lock (_lock)
            {
                for (int i = 0; i < 2; ++i)
                {
                    byte[] seedBytes = new byte[8];

                    using (var rng = RandomNumberGenerator.Create())
                        rng.GetBytes(seedBytes);

                    seed[i] = BitConverter.ToUInt64(seedBytes, 0);
                }

                Reseed(seed[0], seed[1]);
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// </summary>
        /// <param name="seed">ulong seed - the 64-bit seed to use to seed the rng</param>
        /// <param name="seq">ulong seq - the 64-bits sequence selector to use to seed the rng</param>
        /// <param name="ignoreZeroSeed">bool IgnoreZeroSeed - don't throw an exception if seeding with 0</param>
        /// <exception cref="ArgumentException">if allowZeroSeed is false and seed is 0</exception>"
        public void Reseed(ulong seed, ulong seq, bool allowZeroSeed = false)
        {
            if (!allowZeroSeed && (seed == 0 && seq == 0))
                throw new ArgumentException("Seed cannot be zero unless allowZeroSeed is true.");

            lock (_lock)
            {
                _state = 0;
                _inc = (seq << 1) | 1;
                NextRaw32();
                _state += seed;
                NextRaw32();
            }
        }

        /// <summary>
        /// NextRaw64() returns an unsigned 64-bit integer in the range [0, Max] (inclusive)
        /// Public, but not for general consumption although it is safe to do so
        /// </summary>
        /// <returns>ulong</returns>
        public override ulong NextRaw64()
        {
            ulong raw32_1, raw32_2;

            lock (_lock)
            {
                raw32_1 = (ulong)NextRaw32() << 32;
                raw32_2 = NextRaw32();
            }

            return raw32_1 | raw32_2;
        }

        private uint NextRaw32()
        {
            lock (_lock)
            {
                ulong oldstate = _state;
                _state = oldstate * PCG_STATE_MULT + _inc;
                uint xorshifted = (uint)(((oldstate >> 18) ^ oldstate) >> 27);
                uint rot = (uint)(oldstate >> 59);
                return (xorshifted >> (int)rot) | (xorshifted << (int)((32 - rot) & 31));
            }
        }
        /// <summary>
        /// PCG32 does not support the Jump() method.
        /// </summary>
        public override void Jump()
        {
            throw new NotSupportedException("Jump() is not supported for PCG32.");
        }



        /// <summary>
        /// PCG32 does not support the LongJump() method.
        /// </summary>
        public override void LongJump()
        {
            throw new NotSupportedException("LongJump() is not supported for PCG32.");
        }        
    }
}