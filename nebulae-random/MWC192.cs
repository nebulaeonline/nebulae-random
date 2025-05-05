using System;
using System.Security.Cryptography;

using System.Numerics;

namespace nebulae.rng
{
    public class MWC192 : BaseRng
    {
        private const ulong MWC_A2 = 0xffa04e67b3c95d86;

        private ulong _x;
        private ulong _y;
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
                MWC192 clone = new MWC192();
                clone._x = _x;
                clone._y = _y;
                clone._c = _c;
                return clone;
            }
        }

        /// <summary>
        /// MWC192() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public MWC192()
        {
            Reseed();
        }

        /// <summary>
        /// MWC192() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seed_lo">ulong seed_lo - the lower 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="seed_hi">ulong seed_hi - the upper 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="ignoreZeroSeed">bool IgnoreZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        public MWC192(ulong seed_lo, ulong seed_hi, bool allowZeroSeed = false)
        {
            Reseed(seed_lo, seed_hi);
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
        /// <param name="seed_lo">ulong seed_lo - the lower 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="seed_hi">ulong seed_hi - the upper 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="ignoreZeroSeed">bool IgnoreZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        /// <exception cref="ArgumentException">if allowZeroSeed is false and seed is 0</exception>"
        public void Reseed(ulong seed_lo, ulong seed_hi, bool allowZeroSeed = false)
        {
            if (!allowZeroSeed && (seed_lo == 0 && seed_hi == 0))
                throw new ArgumentException("Seed cannot be zero unless allowZeroSeed is true.");

            lock (_lock)
            {
                _x = seed_lo;
                _y = seed_hi;
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
                ulong result = _y;

                // Compute t = MWC_A2 * _x + _c as a 128-bit result
                MulAdd64(MWC_A2, _x, _c, out ulong lo, out ulong hi);

                _x = _y;
                _y = lo;
                _c = hi;

                return result;
            }
        }

        /// <summary>
        /// Jump() moves the RNG sequence ahead by 2^96 steps.
        /// </summary>
        public override void Jump()
        {
            lock (_lock)
            {
                const string R_HEX = "0dc2be36e4bd21a2afc217e3b9edf985d94fb8d87c7c6437";
                ApplyMWC192Jump(R_HEX);
            }
        }



        /// <summary>
        /// LongJump() moves the RNG sequence ahead by 2^144 steps.
        /// </summary>
        public override void LongJump()
        {
            lock (_lock)
            {
                const string R_HEX = "3c6528aaead6bbddec956c3909137b2dd0e7cedd16a0758e";
                ApplyMWC192Jump(R_HEX);
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

        private void ApplyMWC192Jump(string rHex)
        {
            const ulong MWC_A2 = 0xffa04e67b3c95d86UL;

            BigInteger m = ((BigInteger)MWC_A2 << 128) - BigInteger.One;

            BigInteger r = BigInteger.Parse(rHex, System.Globalization.NumberStyles.HexNumber);

            BigInteger state = _x + ((BigInteger)_y << 64) + ((BigInteger)_c << 128);

            BigInteger s = (state * r) % m;
            if (s.Sign < 0) s += m;

            _x = (ulong)(s & ulong.MaxValue);
            _y = (ulong)((s >> 64) & ulong.MaxValue);
            _c = (ulong)((s >> 128) & ulong.MaxValue);
        }

    }
}