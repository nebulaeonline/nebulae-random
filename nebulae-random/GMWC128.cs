using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace nebulae.rng
{
    public class GMWC128 : BaseRng
    {
        private const ulong GMWC_MINUSA0 = 0x7d084a4d80885f;
        private const ulong GMWC_A0INV = 0x9b1eea3792a42c61;
        private const ulong GMWC_A1 = 0xff002aae7d81a646;

        public ulong _x;
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
                GMWC128 clone = new GMWC128();
                clone._x = _x;
                clone._c = _c;
                return clone;
            }
        }

        /// <summary>
        /// GMWC128() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public GMWC128()
        {
            Reseed();
        }

        /// <summary>
        /// GMWC128() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seed">ulong seed - the seed to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        public GMWC128(ulong seed, bool allowZeroSeed = false)
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
                // t = GMWC_A1 * x + c (128-bit)
                BigInteger t = (BigInteger)GMWC_A1 * _x + _c;

                // Extract low 64 bits of t
                ulong t_lo = (ulong)(t & 0xFFFFFFFFFFFFFFFFUL);

                // x = A0INV * low64(t) (wrap to ulong)
                ulong x_new = unchecked(GMWC_A0INV * t_lo);

                // c = high64(t + MINUSA0 * x)
                BigInteger carry_term = t + (BigInteger)GMWC_MINUSA0 * x_new;
                ulong c_new = (ulong)(carry_term >> 64);

                _x = x_new;
                _c = c_new;

                return _x;
            }
        }


        /// <summary>
        /// Jump() moves the RNG sequence ahead by 2^64 steps.
        /// </summary>
        public override void Jump()
        {
            lock (_lock)
            {
                BigInteger b = BigInteger.One << 64;
                BigInteger m = (BigInteger)GMWC_A1 * b + GMWC_MINUSA0;
                BigInteger r = BigInteger.Parse("0EFF1CF6268DB2DD61F03B9690DC51B2F", NumberStyles.HexNumber);

                BigInteger t = ((m + ((BigInteger)_c * b) - (BigInteger)GMWC_MINUSA0 * _x) * r) % m;
                if (t.Sign < 0) t += m;

                ulong t_lo = (ulong)(t & ulong.MaxValue);
                ulong x_new = unchecked(GMWC_A0INV * t_lo);

                BigInteger carry = t + (BigInteger)GMWC_MINUSA0 * x_new;
                ulong c_new = (ulong)(carry >> 64);

                _x = x_new;
                _c = c_new;
            }
        }

        /// <summary>
        /// LongJump() moves the RNG sequence ahead by 2^96 steps.
        /// I cannot duplicate either the C or C++ boost implementation of this function.
        /// Values do not match the reference implementation, so for now, it has been disabled.
        /// </summary>
        public override void LongJump()
        {
            throw new NotImplementedException("LongJump() is not implemented yet.");
        }
    }
}