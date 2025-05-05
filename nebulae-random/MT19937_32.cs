using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace nebulae.rng
{
    public class MT19937_32 : BaseRng
    {
        private const int N = 624;
        private const int M = 397;
        private const ulong MATRIX_A = 0x9908B0DFUL; // constant vector a
        private const ulong UPPER_MASK = 0x80000000UL; // most significant w-r bits
        private const ulong LOWER_MASK = 0x7FFFFFFFUL; // least significant r bits

        private ulong[] mt = new ulong[N]; // the array for the state vector
        private ulong mti = N + 1; // mti==N+1 means mt[N] is not initialized
        private ulong[] mag01 = new ulong[2] { 0UL, MATRIX_A };

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
            MT19937_32 copy;

            lock (_lock)
            {
                copy = new MT19937_32(); // testing constructor; does not reseed

                copy.mt = mt;
                copy.mti = mti;
            }
            return copy;
        }

        /// <summary>
        /// MT19937_32() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 8 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public MT19937_32()
        {
            Reseed();
        }

        /// <summary>
        /// MT19937_32() constructs the rng object and seeds the rng with the given 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 64-bit unsigned integers, to use to seed the rng</param>
        /// <returns>the constructed & seeded rng</returns>
        public MT19937_32(ulong[] seeds)
        {
            Reseed(seeds);
        }

        /// <summary>
        /// MT19937() constructs the rng object and seeds the rng object with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seed">ulong[] seed - the seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws via Reseed() if the seed is 0, unless allowZeroSeed is set</exception>"
        public MT19937_32(ulong seed, bool allowZeroSeed = false)
        {
            Reseed(seed, allowZeroSeed);
        }

        /// <summary>
        /// Reseed() reseeds the rng object
        /// This variant uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 8 bytes of random data to seed the RNG.
        /// </summary>
        public override void Reseed()
        {
            ulong seed;

            byte[] bytes = new byte[8];
#if NET6_0_OR_GREATER
            bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
#else
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
#endif
            lock (_lock)
            {
                var bytes_array = MemoryMarshal.Cast<byte, ulong>(bytes);
                
                seed = bytes_array[0];
            }

            Reseed(seed);
        }

        /// <summary>
        /// Reseed() reseeds the rng object with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 4 64-bit unsigned integers, to use to seed the rng</param>
         public void Reseed(ulong[] seeds)
        {
            lock (_lock)
            {
                int i, j, k;
                Reseed(19650218UL);
                
                i = 1; j = 0;
                k = (N > seeds.Length) ? N : seeds.Length;

                for (; k > 0; k--)
                {
                    mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525UL)) + seeds[j] + (ulong)j;
                    mt[i] &= 0xFFFFFFFFUL;
                    i++; j++;
                    if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                    if (j >= seeds.Length) j = 0;
                }

                for (k = N - 1; k > 0; k--)
                {
                    mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941UL)) - (ulong)i;
                    mt[i] &= 0xFFFFFFFFUL;
                    i++;
                    if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                }

                mt[0] = 0x80000000UL; // Set MSB to 1 indicating a non-zero initial array
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng object with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seed">ulong[] seed - the seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        public void Reseed(ulong seed, bool allowZeroSeed = false)
        {
            if (seed == 0 && !allowZeroSeed)
                throw new ArgumentOutOfRangeException(nameof(seed), "Seed cannot be 0 unless allowZeroSeed is set to true.");

            lock (_lock)
            {
                mt[0] = seed & 0xFFFFFFFFUL;

                for (mti = 1; mti < N; mti++)
                {
                    mt[mti] = (1812433253UL * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                    mt[mti] &= 0xFFFFFFFFUL;
                }
            }
        }

        /// <summary>
        /// NextRaw64() returns an unsigned 64-bit integer in the range [0, Max] (inclusive)
        /// Public, but not for general consumption although it is safe to do so
        /// </summary>
        /// <returns>ulong</returns>
        public override ulong NextRaw64()
        {
            ulong raw64;

            lock (_lock)
            {
                raw64 = (ulong)GenerateRand32() << 32;
                raw64 |= (ulong)GenerateRand32();
            }

            return raw64;
        }

        /// <summary>
        /// Not supported in this RNG
        /// </summary>
        public override void Jump()
        {
            throw new NotSupportedException("Jump() is not supported in this RNG.");
        }

        /// <summary>
        /// Not supported in this RNG
        /// </summary>
        public override void LongJump()
        {
            throw new NotSupportedException("LongJump() is not supported in this RNG.");
        }

        // MT19937_32 is a 32-bit generator, so we need to do this internally
        // so the external interface can generate 2 x 32-bit for its 64-bit
        // requirement. The base class banks integers smaller than 32-bit,
        // so generating 32-bit numbers from this will generate the same
        // sequence.
        private ulong GenerateRand32()
        {
            ulong y;

            if (mti >= N)
            {
                int kk;

                if (mti == N + 1) Reseed(5489UL);

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 1UL];
                }

                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 1UL];
                }

                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 1UL];

                mti = 0;
            }

            y = mt[mti++];

            // tempering
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680UL;
            y ^= (y << 15) & 0xefc60000UL;
            y ^= (y >> 18);

            return y;
        }
    }
}
