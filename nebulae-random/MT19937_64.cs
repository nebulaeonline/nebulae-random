using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace nebulae.rng
{
    public class MT19937_64 : BaseRng
    {
        private const int NN = 312;
        private const int MM = 156;
        private const ulong MATRIX_A = 0xB5026F5AA96619E9UL; // constant vector a
        private const ulong UM = 0xFFFFFFFF80000000UL; // most significant w-r bits
        private const ulong LM = 0x7FFFFFFFUL; // least significant r bits

        private ulong[] mt = new ulong[NN]; // the array for the state vector
        private ulong mti = NN + 1; // mti==N+1 means mt[N] is not initialized
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
            MT19937_64 copy;

            lock (_lock)
            {
                copy = new MT19937_64(); // testing constructor; does not reseed

                copy.mt = mt;
                copy.mti = mti;
            }
            return copy;
        }

        /// <summary>
        /// MT19937_64() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 8 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public MT19937_64()
        {
            Reseed();
        }

        /// <summary>
        /// MT19937_64() constructs the rng object and seeds the rng with the given 64-bit unsigned integers
        /// </summary>
        /// <param name="seeds">ulong[] seeds - the seeds, as an array of 64-bit unsigned integers, to use to seed the rng</param>
        /// <returns>the constructed & seeded rng</returns>
        public MT19937_64(ulong[] seeds)
        {
            Reseed(seeds);
        }

        /// <summary>
        /// MT19937() constructs the rng object and seeds the rng object with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seed">ulong[] seed - the seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws via Reseed() if the seed is 0, unless allowZeroSeed is set</exception>""
        public MT19937_64(ulong seed, bool allowZeroSeed = false)
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
                k = (NN > seeds.Length) ? NN : seeds.Length;

                for (; k > 0; k--)
                {
                    mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 62)) * 3935559000370003845UL)) + seeds[j] + (ulong)j;
                    i++; j++;
                    if (i >= NN) { mt[0] = mt[NN - 1]; i = 1; }
                    if (j >= seeds.Length) j = 0;
                }

                for (k = NN - 1; k > 0; k--)
                {
                    mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 62)) * 2862933555777941757UL)) - (ulong)i;
                    i++;
                    if (i >= NN) { mt[0] = mt[NN - 1]; i = 1; }
            }

                mt[0] = 1UL << 63; // Set MSB to 1 indicating a non-zero initial array
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng object with the given 4 64-bit unsigned integers
        /// </summary>
        /// <param name="seed">ulong[] seed - the seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if the seed is 0, unless allowZeroSeed is set</exception>
        public void Reseed(ulong seed, bool allowZeroSeed = false)
        {
            if (seed == 0 && !allowZeroSeed)
                throw new ArgumentOutOfRangeException(nameof(seed), "Seed cannot be 0 unless allowZeroSeed is set to true.");

            lock (_lock)
            {
                mt[0] = seed;

                for (mti = 1; mti < NN; mti++)
                {
                    mt[mti] = (6364136223846793005UL * (mt[mti - 1] ^ (mt[mti - 1] >> 62)) + mti);
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
            ulong x;
            int i;

            lock (_lock)
            {
                if (mti >= NN)
                { 

                    /* if Reseed() or the constructor were called with zero as a seed, */
                    /* a default initial seed is used     */
                    if (mti == NN + 1)
                        Reseed(5489UL);

                    for (i = 0; i < NN - MM; i++)
                    {
                        x = (mt[i] & UM) | (mt[i + 1] & LM);
                        mt[i] = mt[i + MM] ^ (x >> 1) ^ mag01[(int)(x & 1UL)];
                    }
                    for (; i < NN - 1; i++)
                    {
                        x = (mt[i] & UM) | (mt[i + 1] & LM);
                        mt[i] = mt[i + (MM - NN)] ^ (x >> 1) ^ mag01[(int)(x & 1UL)];
                    }
                    x = (mt[NN - 1] & UM) | (mt[0] & LM);
                    mt[NN - 1] = mt[MM - 1] ^ (x >> 1) ^ mag01[(int)(x & 1UL)];

                    mti = 0;
                }

                x = mt[mti++];

                x ^= (x >> 29) & 0x5555555555555555UL;
                x ^= (x << 17) & 0x71D67FFFEDA60000UL;
                x ^= (x << 37) & 0xFFF7EEE000000000UL;
                x ^= (x >> 43);

                return x;
            }
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
    }
}
