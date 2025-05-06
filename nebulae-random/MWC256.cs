using System;
using System.Security.Cryptography;

using System.Numerics;

namespace nebulae.rng
{
    public class MWC256 : BaseRng
    {
        private const ulong MWC_A3 = 0xfff62cf2ccc0cdaf;

        private ulong _x;
        private ulong _y;
        private ulong _z;
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
                MWC256 clone = new MWC256();
                clone._x = _x;
                clone._y = _y;
                clone._z = _z;
                clone._c = _c;
                return clone;
            }
        }

        /// <summary>
        /// MWC256() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public MWC256()
        {
            Reseed();
        }

        /// <summary>
        /// MWC256() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seed_lo">ulong seed_lo - the lower 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="seed_mid">ulong seed_mid - the middle 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="seed_hi">ulong seed_hi - the upper 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="ignoreZeroSeed">bool IgnoreZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        public MWC256(ulong seed_lo, ulong seed_mid, ulong seed_hi, bool allowZeroSeed = false)
        {
            Reseed(seed_lo, seed_mid, seed_hi);
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// This variant System.Security.Cryptography.RandomNumberGenerator
        /// component to get 24 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public override void Reseed()
        {
            ulong[] seed = new ulong[3];

            lock (_lock)
            {
                for (int i = 0; i < 3; ++i)
                {
                    byte[] seedBytes = new byte[8];

                    using (var rng = RandomNumberGenerator.Create())
                        rng.GetBytes(seedBytes);

                    seed[i] = BitConverter.ToUInt64(seedBytes, 0);
                }

                Reseed(seed[0], seed[1], seed[2]);
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// </summary>
        /// <param name="seed_lo">ulong seed_lo - the lower 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="seed_mid">ulong seed_mid - the middle 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="seed_hi">ulong seed_hi - the upper 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="ignoreZeroSeed">bool IgnoreZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        /// <exception cref="ArgumentException">if allowZeroSeed is false and seed is 0</exception>"
        public void Reseed(ulong seed_lo, ulong seed_mid, ulong seed_hi, bool allowZeroSeed = false)
        {
            if (!allowZeroSeed && (seed_lo == 0 && seed_mid == 0 && seed_hi == 0))
                throw new ArgumentException("Seed cannot be zero unless allowZeroSeed is true.");

            lock (_lock)
            {
                _x = seed_lo;
                _y = seed_mid;
                _z = seed_hi;
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
                ulong result = _z;

                // Compute t = MWC_A3 * _x + _c as a 128-bit result
                MulAdd64(MWC_A3, _x, _c, out ulong lo, out ulong hi);

                _x = _y;
                _y = _z;
                _z = lo;
                _c = hi;

                return result;
            }
        }

        /// <summary>
        /// Jump() moves the RNG sequence ahead by 2^128 steps.
        /// </summary>
        public override void Jump()
        {
            lock (_lock)
            {
                const string R_HEX = "4b89aa2cd51c37b9f6f8c3fd02ec98fbfe88c291203b225428c3ff11313847eb";
                ApplyMWC256Jump(R_HEX);
            }
        }



        /// <summary>
        /// LongJump() moves the RNG sequence ahead by 2^192 steps.
        /// </summary>
        public override void LongJump()
        {
            lock (_lock)
            {
                const string R_HEX = "0af5ca22408cdc8306c40ce860e0d702f95382f758ac987764c6e39cf92f77a4";
                ApplyMWC256Jump(R_HEX);
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

        public void ApplyMWC256Jump(string rHex)
        {
            // Modulus = MWC_A3 × 2^192 − 1
            BigInteger m = ((BigInteger)MWC_A3 << 192) - BigInteger.One;

            BigInteger r = BigInteger.Parse(rHex, System.Globalization.NumberStyles.HexNumber);

            // Compose 256-bit state
            BigInteger state =
                ((BigInteger)_c << 192) |
                ((BigInteger)_z << 128) |
                ((BigInteger)_y << 64) |
                _x;

            // Apply jump
            BigInteger s = (state * r) % m;
            if (s.Sign < 0) s += m;

            // Extract new 64-bit parts
            _x = (ulong)(s & ulong.MaxValue);
            _y = (ulong)((s >> 64) & ulong.MaxValue);
            _z = (ulong)((s >> 128) & ulong.MaxValue);
            _c = (ulong)((s >> 192) & ulong.MaxValue);
        }
    }
}