using System;
using System.Security.Cryptography;

using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace nebulae.rng
{
    public class MWC128 : BaseRng
    {
        private const ulong MWC_A1 = 0xffebb71d94fcdaf9;

        private ulong _x;
        private ulong _c;

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
                MWC128 clone = new MWC128();
                clone._x = _x;
                clone._c = _c;
                return clone;
            }
        }

        /// <summary>
        /// MWC128() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public MWC128()
        {
            Reseed();
        }

        /// <summary>
        /// MWC128() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seed">ulong seed - the seed to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        public MWC128(ulong seed, bool allowZeroSeed = false)
        {
            Reseed(seed);
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// This variant System.Security.Cryptography.RandomNumberGenerator
        /// component to get 8 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public override void Reseed()
        {
            lock (_lock)
            {
                byte[] seedBytes = new byte[8];

                using (var rng = RandomNumberGenerator.Create())
                    rng.GetBytes(seedBytes);

                var seed = BitConverter.ToUInt64(seedBytes, 0);


                Reseed(seed);
            }
            
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// </summary>
        /// <param name="seed">ulong seed - the seed to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        /// <exception cref="ArgumentException">if allowZeroSeed is false and seed is 0</exception>"
        public void Reseed(ulong seed, bool allowZeroSeed = false)
        {
            if (!allowZeroSeed && seed == 0)
                throw new ArgumentException("Seed cannot be zero unless allowZeroSeed is true.");

            lock (_lock)
            {
                _x = seed;
                _c = 1;
            }
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
                ulong result = _x ^ (_x << 32);

                MulAdd64(MWC_A1, _x, _c, out ulong low, out ulong high);

                _x = low;
                _c = high;

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
                // Constants
                BigInteger b = BigInteger.One << 64;
                BigInteger m = (BigInteger)MWC_A1 * b - 1;

                BigInteger r = BigInteger.Parse("2f65fed2e8400983a72f9a3547208003", System.Globalization.NumberStyles.HexNumber);
                BigInteger state = _x + _c * b;

                // Ensure unsigned modular result
                BigInteger s = (state * r) % m;
                if (s.Sign < 0)
                    s += m;

                _x = (ulong)(s & ulong.MaxValue);
                _c = (ulong)((s >> 64) & ulong.MaxValue);
            }
        }

        /// <summary>
        /// LongJump() moves the RNG sequence ahead by 2^96 steps.
        /// </summary>
        public override void LongJump()
        {
            lock (_lock)
            {
                BigInteger b = BigInteger.One << 64;
                BigInteger m = (BigInteger)MWC_A1 * b - 1;

                // NOTE: parse without 0x and with HexNumber
                BigInteger r = BigInteger.Parse("394649cfd6769c91e6f7814467f3fcdd", System.Globalization.NumberStyles.HexNumber);
                BigInteger state = _x + _c * b;

                BigInteger s = (state * r) % m;
                if (s.Sign < 0) s += m;

                _x = (ulong)(s & ulong.MaxValue);
                _c = (ulong)((s >> 64) & ulong.MaxValue);
            }
        }

        private static void MulAdd64(ulong a, ulong b, ulong c, out ulong lo, out ulong hi)
        {
            // Computes (a * b + c) as a 128-bit result (lo, hi)
            // Emulates unsigned __uint128_t behavior in C

            ulong a_lo = (uint)a;
            ulong a_hi = a >> 32;
            ulong b_lo = (uint)b;
            ulong b_hi = b >> 32;

            ulong p0 = a_lo * b_lo;
            ulong p1 = a_lo * b_hi;
            ulong p2 = a_hi * b_lo;
            ulong p3 = a_hi * b_hi;

            ulong mid = p1 + p2;
            ulong carry1 = (mid < p1) ? 1UL << 32 : 0;

            ulong mid_lo = mid << 32;
            ulong mid_hi = mid >> 32;

            ulong sum_lo = p0 + mid_lo;
            ulong carry2 = (sum_lo < p0) ? 1UL : 0;

            lo = sum_lo + c;
            ulong carry3 = (lo < sum_lo) ? 1UL : 0;

            hi = p3 + mid_hi + carry1 + carry2 + carry3;
        }

    }
}