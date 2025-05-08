using System;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;

namespace nebulae.rng
{
    public class GMWC256 : BaseRng
    {
        private const ulong GMWC_MINUSA0 = 0x54c3da46afb70f;
        private const ulong GMWC_A0INV = 0xbbf397e9a69da811;
        private const ulong GMWC_A3 = 0xff963a86efd088a2;

        public ulong _x;
        public ulong _y;
        public ulong _z;
        public ulong _c;

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
                GMWC256 clone = new GMWC256();
                clone._x = _x;
                clone._y = _y;
                clone._z = _z;
                clone._c = _c;
                return clone;
            }
        }

        /// <summary>
        /// GMWC256() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public GMWC256()
        {
            Reseed();
        }

        /// <summary>
        /// GMWC256() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seed_lo">ulong seed_lo - the lower 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="seed_mid">ulong seed_mid - the middle 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="seed_hi">ulong seed_hi - the upper 64-bits of the 128-bit seed to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        public GMWC256(ulong seed_lo, ulong seed_mid, ulong seed_hi, bool allowZeroSeed = false)
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
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if seeding with 0</param>
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
                BigInteger t = GMWC_A3 * (BigInteger)_x + _c;
                _x = _y;
                _y = _z;
                _z = GMWC_A0INV * (ulong)(t & ulong.MaxValue);
                _c = (ulong)(((t + GMWC_MINUSA0 * (BigInteger)_z) >> 64) & ulong.MaxValue);

                return _z;
            }
        }

        /// <summary>
        /// Jump() moves the RNG sequence ahead by 2^128 steps.
        /// </summary>
        public override void Jump()
        {
            lock (_lock)
            {
                InternalJump("7f37803efa1e1fa0b6e0bc8a24046dc4f12f5272b6224185e5daa67441bb11e8");                
            }
        }




        /// <summary>
        /// LongJump() moves the RNG sequence ahead by 2^192 steps.
        /// </summary>
        public override void LongJump()
        {
            lock (_lock)
            {
                InternalJump("4ac54a5962a2cc857261a61127b93d83a28e7b70072f6082ccb6a9c9ec62a812");
            }
        }
        
        private void InternalJump(string constant)
        {
            // Constants
            BigInteger b = BigInteger.One << 64;
            BigInteger b3 = b * b * b;

            BigInteger m = GMWC_A3 * b3 + GMWC_MINUSA0;

            BigInteger r = BigInteger.Parse(constant, NumberStyles.HexNumber);

            BigInteger xyzValue = _x + _y * b + _z * b * b;

            BigInteger s = ((m + _c * b3 - GMWC_MINUSA0 * xyzValue) * r) % m;
            if (s.Sign < 0) s += m;

            BigInteger multiplier = BigInteger.Parse(
                "ec9c73821433a23dc3641cd8c2367132bbf397e9a69da811",
                NumberStyles.HexNumber);

            BigInteger xyz = s * multiplier;

            _x = (ulong)(xyz & ulong.MaxValue);
            _y = (ulong)((xyz >> 64) & ulong.MaxValue);
            _z = (ulong)((xyz >> 128) & ulong.MaxValue);

            BigInteger mod_b3 = xyz % b3;

            ulong r0 = (ulong)(mod_b3 & ulong.MaxValue);
            ulong r1 = (ulong)((mod_b3 >> 64) & ulong.MaxValue);
            ulong r2 = (ulong)((mod_b3 >> 128) & ulong.MaxValue);

            BigInteger partial =
                (BigInteger)r0 +
                ((BigInteger)r1 << 64) +
                ((BigInteger)r2 << 128);

            BigInteger prod = GMWC_MINUSA0 * partial;
            BigInteger carry = s + prod;

            _c = (ulong)((carry >> 192) & ulong.MaxValue);
        }
    }
}