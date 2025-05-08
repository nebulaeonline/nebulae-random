using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace nebulae.rng
{
    public class PCG64 : BaseRng
    {
        private readonly BigInteger PCG_DEFAULT_MULTIPLIER_128 = ((BigInteger)2549297995355413924UL << 64) | 4865540595714422341UL;
        private readonly BigInteger PCG_DEFAULT_INCREMENT_128 = ((BigInteger)6364136223846793005UL << 64) | 1442695040888963407UL;

        private BigInteger _state;
        private BigInteger _inc;

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
                PCG64 clone = new PCG64();
                clone._state = _state;
                clone._inc = _inc;
                return clone;
            }
        }

        /// <summary>
        /// PCG64() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 128 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public PCG64()
        {
            Reseed();
        }

        /// <summary>
        /// Reseed() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seed_hi">ulong seed_hi - the high 64-bits of the seed to use to seed the rng</param>
        /// <param name="seed_lo">ulong seed_lo - the low 64-bits of the seed to use to seed the rng</param>
        /// <param name="seq_hi">ulong seq_hi - the high 64-bits of the sequence selector to use to seed the rng</param>
        /// <param name="seq_lo">ulong seq_lo - the low 64-bits of the sequence selector to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if seeding with 0</param>
        /// <exception cref="ArgumentException">if allowZeroSeed is false and seed is 0</exception>"
        public PCG64(ulong seed_hi, ulong seed_lo, ulong seq_hi, ulong seq_lo, bool allowZeroSeed = false)
        {
            BigInteger seed = ((BigInteger)seed_hi << 64) | seed_lo;
            BigInteger seq = ((BigInteger)seq_hi << 64) | seq_lo;

            Reseed(seed, seq, allowZeroSeed);
        }

        /// <summary>
        /// PCG64() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seed">ulong seed - the 64-bit seed to use to seed the rng</param>
        /// <param name="seq">ulong seq - the 64-bits sequence selector to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        public PCG64(BigInteger seed, BigInteger seq, bool allowZeroSeed = false)
        {
            Reseed(seed, seq, allowZeroSeed);
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// This variant System.Security.Cryptography.RandomNumberGenerator
        /// component to get 32 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public override void Reseed()
        {
            ulong[] seeds = new ulong[4];

            lock (_lock)
            {
                for (int i = 0; i < 4; ++i)
                {
                    byte[] seedBytes = new byte[32];

                    using (var rng = RandomNumberGenerator.Create())
                        rng.GetBytes(seedBytes);

                    seeds[i] = BitConverter.ToUInt64(seedBytes, 0);
                }

                BigInteger seed = ((BigInteger)seeds[0] << 64) | seeds[1];
                BigInteger seq = ((BigInteger)seeds[2] << 64) | seeds[3];

                Reseed(seed, seq);
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// </summary>
        /// <param name="seed">BigInteger seed - the 64-bit seed to use to seed the rng</param>
        /// <param name="seq">BigInteger seq - the 64-bits sequence selector to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if seeding with 0</param>
        /// <exception cref="ArgumentException">if allowZeroSeed is false and seed is 0</exception>"
        public void Reseed(BigInteger seed, BigInteger seq, bool allowZeroSeed = false)
        {
            if (!allowZeroSeed && (seed == 0 && seq == 0))
                throw new ArgumentException("Seed cannot be zero unless allowZeroSeed is true.");

            lock (_lock)
            {
                PcgSetSeq128SRandomR(seed, seq);
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
                return PcgSetSeq128XslRotateRight64RandomR();
            }            
        }

        /// <summary>
        /// Jump ahead by 2^64 states.
        /// </summary>
        public override void Jump()
        {
            Advance(BigInteger.One, PCG_DEFAULT_MULTIPLIER_128, PCG_DEFAULT_INCREMENT_128);
        }

        /// <summary>
        /// Jump ahead by 2^96 states.
        /// </summary>
        public override void LongJump()
        {
            Advance(BigInteger.One << 32, PCG_DEFAULT_MULTIPLIER_128, PCG_DEFAULT_INCREMENT_128);
        }

        public static ulong PcgRotateRight64(ulong value, int rot)
        {
            rot &= 63; // mask to [0, 63]
            return (value >> rot) | (value << ((64 - rot) & 63));
        }

        private void PcgSetSeq128StepR()
        {
            _state = (_state * PCG_DEFAULT_MULTIPLIER_128) + _inc;
        }

        private ulong PcgOutputXslRotateRight128_64()
        {
            ulong hi = (ulong)(_state >> 64 & 0xFFFFFFFFFFFFFFFF);
            ulong lo = (ulong)(_state & 0xFFFFFFFFFFFFFFFF);
            int rot = (int)((_state >> 122) & 0x3F);

            return PcgRotateRight64(hi ^ lo, rot);
        }

        private void PcgSetSeq128SRandomR(BigInteger initstate, BigInteger initseq)
        {
            _state = 0;
            _inc = (initseq << 1) | 1; // the sequence selector must be odd
            PcgSetSeq128StepR();
            _state += initstate;
            PcgSetSeq128StepR();

        }

        private ulong PcgSetSeq128XslRotateRight64RandomR()
        {
            PcgSetSeq128StepR();
            ulong output = PcgOutputXslRotateRight128_64();

            return output;
        }

        public void Advance(BigInteger delta, BigInteger multiplier, BigInteger increment)
        {
            BigInteger acc_mult = BigInteger.One; // a^0 == 1
            BigInteger acc_plus = 0;

            while (!delta.Equals(0))
            {
                if ((delta & 1) != 0)
                {
                    acc_mult = acc_mult * multiplier;
                    acc_plus = acc_plus * multiplier + increment;
                }

                increment = (multiplier + BigInteger.One) * increment;
                multiplier = multiplier * multiplier;
                delta >>= 1;
            }

            _state = acc_mult * _state + acc_plus;
        }
    }
}