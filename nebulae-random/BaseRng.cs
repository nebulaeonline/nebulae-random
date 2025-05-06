using nebulae.dub;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace nebulae.rng
{
    public abstract class BaseRng : INebulaeRng
    {
        protected const ulong HIGH32 = 0xFFFF_FFFF_0000_0000;
        protected const ulong LOW32 = 0x0000_0000_FFFF_FFFF;
        protected const ulong LOW16 = 0x0000_0000_0000_FFFF;
        protected const ulong LOW8 = 0x0000_0000_0000_00FF;

        protected ConcurrentStack<uint> _banked32 = new ConcurrentStack<uint>();
        protected ConcurrentStack<ushort> _banked16 = new ConcurrentStack<ushort>();
        protected ConcurrentStack<byte> _banked8 = new ConcurrentStack<byte>();

        /// <summary>
        /// Jump() moves the RNG sequence ahead by a large number of steps.
        /// this varies by RNG implementation, but is typically 2^64 or more steps.
        /// Not all RNGs support this.
        /// </summary>
        public abstract void Jump();

        /// <summary>
        /// LongJump() moves the RNG sequence ahead by a very large number of steps.
        /// this varies by RNG implementation, but is typically 2^128 or more steps.
        /// Not all RNGs support this.
        /// </summary>
        public abstract void LongJump();

        /// <summary>
        /// NextRaw64() returns an unsigned 64-bit integer in the range [0, Max] (inclusive)
        /// Public, but not for general consumption although it is safe to do so
        /// Common interface to the nebulae.rng RNG api
        /// </summary>
        /// <returns>ulong</returns>        
        public abstract ulong NextRaw64();

        /// <summary>
        /// Clone() clones the internal state of the RNG and returns a new instance
        /// of the same RNG type with all the same internal state as this one.
        /// </summary>
        /// <returns>an RNG that implements the IRngCore interface</returns>
        public abstract INebulaeRng Clone();

        /// <summary>
        /// Reseeds the RNG with bytes from System.Security.Cryptography.RandomNumberGenerator
        /// </summary>
        /// <returns>an RNG that implements the IRngCore interface</returns>
        public abstract void Reseed();

        /// <summary>
        /// Interface that mimics System.Random
        /// Returns a non-negative random integer (32-bit).
        /// </summary>
        public int Next()
        {
            return (int)(Rand32() & 0x7FFFFFFF);
        }

        /// <summary>
        /// Interface that mimics System.Random
        /// Returns a non-negative random integer less than max.
        /// </summary>
        public int Next(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            return (int)(Rand32((uint)(max - 1)) & 0x7FFFFFFF);
        }

        /// <summary>
        /// Interface that mimics System.Random
        /// Returns a random integer between min (inclusive) and max (exclusive).
        /// </summary>
        public int Next(int min, int max)
        {
            if (min > max) (max, min) = (min, max);
            if (min == max) return min;
            return RangedRand32S(min, max - 1);
        }

        /// <summary>
        /// Interface that mimics System.Random
        /// Returns a random double between [0, 1).
        /// </summary>
        public double NextDouble()
        {
            return RandDouble();
        }

        /// <summary>
        /// Rand64() returns an unsigned 64-bit integer in the range [0, Max] (inclusive)
        /// </summary>
        /// <param name="Max">ulong Max - the maximum random number to return</param>
        /// <returns>ulong</returns>
        public ulong Rand64(ulong Max = 0)
        {
            if (Max == ulong.MaxValue) Max = 0;

            ulong ul = NextRaw64();
            return (Max == 0) ? ul : ul % ++Max;
        }

        /// <summary>
        /// Rand64S() returns a signed 64-bit integer in the range [long.MinValue, Max] (inclusive)
        /// </summary>
        /// <param name="Max">long Max - the maximum random number to return</param>
        /// <returns>ulong</returns>
        public long Rand64S(long Max = 0)
        {
            var maxVal = (Max == 0) ? long.MaxValue : Max;

            return RangedRand64S(long.MinValue, maxVal);
        }

        /// <summary>
        /// RangedRand64() returns an unsigned 64-bit integer in the range [Min, Max] (inclusive)
        /// </summary>
        /// <param name="Min">ulong Min - the minimum random number to return</param>
        /// <param name="Max">ulong Max - the maximum random number to return</param>
        /// <returns>ulong</returns>
        public ulong RangedRand64(ulong Min, ulong Max)
        {
            if (Min == Max) return Min;
            if (Min > Max) (Min, Max) = (Max, Min);

            ulong range = Max - Min + 1;
            if (range == 0) return Rand64();

            ulong threshold = ulong.MaxValue - (ulong.MaxValue % range);

            ulong r;
            do { r = Rand64(); } while (r >= threshold);

            return Min + (r % range);
        }

        /// <summary>
        /// RangedRand64S() returns a signed 64-bit integer in the range [Min, Max] (inclusive)
        /// </summary>
        /// <param name="Min">long Min - the minimum random number to return</param>
        /// <param name="Max">long Max - the maximum random number to return</param>
        /// <returns>long</returns>
        public long RangedRand64S(long Min, long Max)
        {
            if (Min == Max) return Min;
            if (Min > Max) (Min, Max) = (Max, Min);

            ulong range = (ulong)((ulong)Max - (ulong)Min) + 1;
            if (range == 0) return (long)Rand64();

            ulong threshold = ulong.MaxValue - (ulong.MaxValue % range);

            ulong r;
            do { r = Rand64(); } while (r >= threshold);

            return Min + (long)(r % range);
        }

        /// <summary>
        /// Rand32() returns an unsigned 32-bit integer in the range [0, Max] (inclusive)
        /// </summary>
        /// <param name="Max">uint Max - the maximum random number to return</param>
        /// <returns>uint</returns>
        public uint Rand32(uint Max = 0)
        {
            if (Max == uint.MaxValue) Max = 0;

            if (_banked32.TryPop(out uint ui))
                return (Max == 0) ? ui : ui % ++Max;

            ulong ul = NextRaw64();
            uint lo = (uint)(ul & 0xFFFFFFFF);
            _banked32.Push(lo);

            return (Max == 0) ? (uint)(ul >> 32) : (uint)(ul >> 32) % ++Max;
     
        }

        /// <summary>
        /// Rand32S() returns a signed 32-bit integer in the range [int.MinValue, Max] (inclusive)
        /// </summary>
        /// <param name="Max">int Max - the maximum random number to return</param>
        /// <returns>ulong</returns>
        public long Rand32S(int Max = 0)
        {
            var maxVal = (Max == 0) ? int.MaxValue : Max;

            return RangedRand32S(int.MinValue, maxVal);
        }

        /// <summary>
        /// RangedRand32() returns an unsigned 32-bit integer in the range [Min, Max] (inclusive)
        /// </summary>
        /// <param name="Min">uint Min - the minimum random number to return</param>
        /// <param name="Max">uint Max - the maximum random number to return</param>
        /// <returns>uint</returns>
        public uint RangedRand32(uint Min, uint Max)
        {
            if (Min == Max) return Min;
            if (Min > Max) (Min, Max) = (Max, Min);

            ulong range = (ulong)(Max - Min) + 1;
            if (range == 0) return Rand32();

            ulong threshold = (1UL << 32) - ((1UL << 32) % range);

            ulong r;
            do { r = Rand32(); } while (r >= threshold);

            return Min + (uint)(r % range);
        }

        /// <summary>
        /// RangedRand32S() returns a signed 32-bit integer in the range [Min, Max] (inclusive)
        /// </summary>
        /// <param name="Min">int Min - the minimum random number to return</param>
        /// <param name="Max">int Max - the maximum random number to return</param>
        /// <returns>int</returns>
        public int RangedRand32S(int Min, int Max)
        {
            if (Min == Max) return Min;
            if (Min > Max) (Min, Max) = (Max, Min);

            ulong range = (ulong)(Max - Min) + 1;
            if (range == 0) return (int)Rand32();

            ulong threshold = (1UL << 32) - ((1UL << 32) % range);

            ulong r;
            do { r = Rand32(); } while (r >= threshold);

            return Min + (int)(r % range);
        }

        /// <summary>
        /// Rand16() returns an unsigned 16-bit integer in the range [0, Max] (inclusive)
        /// </summary>
        /// <param name="Max">ushort Max - the maximum random number to return</param>
        /// <returns>ushort</returns>
        public ushort Rand16(ushort Max = 0)
        {
            if (Max == ushort.MaxValue) Max = 0;

            if (_banked16.TryPop(out ushort us))
                return (Max == 0) ? us : (ushort)(us % ++Max);

            var r64 = NextRaw64();

            // Push lower bits first (0–15), then (16–31), then (32–47)
            _banked16.Push((ushort)((r64 >> 0) & 0xFFFF));   // bits 0–15
            _banked16.Push((ushort)((r64 >> 16) & 0xFFFF));  // bits 16–31
            _banked16.Push((ushort)((r64 >> 32) & 0xFFFF));  // bits 32–47

            // Now take bits 48–63 immediately
            ushort us2 = (ushort)((r64 >> 48) & 0xFFFF);

            return (Max == 0) ? us2 : (ushort)((uint)us2 % ++Max);
        }

        /// <summary>
        /// Rand16S() returns a signed 16-bit integer in the range [short.MinValue, Max] (inclusive)
        /// </summary>
        /// <param name="Max">short Max - the maximum random number to return</param>
        /// <returns>ulong</returns>
        public long Rand16S(short Max = 0)
        {
            var maxVal = (Max == 0) ? short.MaxValue : Max;

            return RangedRand16S(short.MinValue, maxVal);
        }

        /// <summary>
        /// RangedRand16() returns an unsigned 16-bit integer in the range [Min, Max] (inclusive)
        /// </summary>
        /// <param name="Min">ushort Min - the minimum random number to return</param>
        /// <param name="Max">ushort Max - the maximum random number to return</param>
        /// <returns>ushort</returns>
        public ushort RangedRand16(ushort Min, ushort Max)
        {
            if (Min == Max) return Min;
            if (Min > Max) (Min, Max) = (Max, Min);

            uint range = (uint)(Max - Min) + 1;
            if (range == 0) return Rand16();

            uint threshold = (1U << 16) - ((1U << 16) % range);

            uint r;
            do { r = Rand16(); } while (r >= threshold);

            return (ushort)(Min + (r % range));
        }

        /// <summary>
        /// RangedRand16S() returns a signed 16-bit integer in the range [Min, Max] (inclusive)
        /// </summary>
        /// <param name="Min">short Min - the minimum random number to return</param>
        /// <param name="Max">short Max - the maximum random number to return</param>
        /// <returns>short</returns>
        public short RangedRand16S(short Min, short Max)
        {
            if (Min == Max) return Min;
            if (Min > Max) (Min, Max) = (Max, Min);

            uint range = (uint)(Max - Min) + 1;
            if (range == 0) return (short)Rand16();

            uint threshold = (1U << 16) - ((1U << 16) % range);

            uint r;
            do { r = Rand16(); } while (r >= threshold);

            return (short)(Min + (r % range));
        }

        /// <summary>
        /// Rand8() returns an unsigned 8-bit integer in the range [0, Max] (inclusive)
        /// </summary>
        /// <param name="Max">byte Max - the maximum random number to return</param>
        /// <returns>byte</returns>
        public byte Rand8(byte Max = 0)
        {
            if (Max == byte.MaxValue) Max = 0;

            if (_banked8.TryPop(out byte ub))
                return (Max == 0) ? ub : (byte)(ub % ++Max);

            var r64 = NextRaw64();

            // Push lower bytes first (0–7, 8–15, ..., 48–55)
            for (int i = 0; i < 7; i++)
            {
                _banked8.Push((byte)((r64 >> (i * 8)) & 0xFF));
            }

            // Now take highest 8 bits (56–63) immediately
            byte ub2 = (byte)((r64 >> 56) & 0xFF);

            return (Max == 0) ? ub2 : (byte)((uint)ub2 % ++Max);
        }

        /// <summary>
        /// Rand8S() returns a signed 8-bit integer in the range [sbyte.MinValue, Max] (inclusive)
        /// </summary>
        /// <param name="Max">sbyte Max - the maximum random number to return</param>
        /// <returns>ulong</returns>
        public long Rand8S(sbyte Max = 0)
        {
            var maxVal = (Max == 0) ? sbyte.MaxValue : Max;

            return RangedRand8S(sbyte.MinValue, maxVal);
        }

        /// <summary>
        /// RangedRand8() returns an unsigned 8-bit integer in the range [Min, Max] (inclusive)
        /// </summary>
        /// <param name="Min">byte Min - the minimum random number to return</param>
        /// <param name="Max">byte Max - the maximum random number to return</param>
        /// <returns>byte</returns>
        public byte RangedRand8(byte Min, byte Max)
        {
            if (Min == Max)
                return Min;

            if (Min > Max)
                (Min, Max) = (Max, Min);

            int range = Max - Min + 1;
            if (range <= 0 || range > 256) return Rand8();  // fallback for full range

            int threshold = 256 - (256 % range);

            byte r;
            do { r = Rand8(); } while (r >= threshold);

            return (byte)(Min + (r % range));
        }

        /// <summary>
        /// RangedRand8S() returns a signed 8-bit integer in the range [Min, Max] (inclusive)
        /// </summary>
        /// <param name="Min">sbyte Min - the minimum random number to return</param>
        /// <param name="Max">sbyte Max - the maximum random number to return</param>
        /// <returns>sbyte</returns>
        public sbyte RangedRand8S(sbyte Min, sbyte Max)
        {
            if (Min == Max)
                return Min;

            if (Min > Max)
                (Min, Max) = (Max, Min);

            int range = Max - Min + 1;
            if (range <= 0 || range > 256) return (sbyte)Rand8();

            int threshold = 256 - (256 % range);

            byte r;
            do { r = Rand8(); } while (r >= threshold);

            return (sbyte)(Min + (r % range));
        }


        /// <summary>
        /// RandAlphaNum() returns a char conforming to the specified options
        /// </summary>
        /// <param name="Upper">bool AlphaUpper - include upper case alphas?</param>
        /// <param name="Lower">bool AlphaLower - include lower case alphas?</param>
        /// <param name="Numeric">bool Numeric -  include numeric characters?</param>
        /// <param name="ExtraSymbols">char[]? ExtraSymbols - any additional symbols? (pass as char array)</param>
        /// <returns>char</returns>
        public char RandAlphaNum(bool Upper = true, bool Lower = true, bool Numeric = true, char[] ExtraSymbols = null)
        {
            List<char> charset = new List<char>();

            if (Numeric)
                charset.AddRange("0123456789");
            if (Upper)
                charset.AddRange("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            if (Lower)
                charset.AddRange("abcdefghijklmnopqrstuvwxyz");
            if (ExtraSymbols != null)
                charset.AddRange(ExtraSymbols);

            if (charset.Count == 0)
                throw new ArgumentException("You must enable at least one character group or pass custom symbols.");

            // Rejection sampling to remove bias
            byte rnd;
            int count = charset.Count;
            int max = 256 - (256 % count);

            do
            {
                rnd = Rand8();
            } while (rnd >= max);

            return charset[rnd % count];
        }

        /// <summary>
        /// RandDouble() returns a double between [0.0, 1.0)
        /// </summary>
        /// <param name="minzero">double minzero - the minimum value which will be considered 0.0
        /// to avoid range compression (default is 1.0e-4)</param>
        /// <returns>double</returns>
        public double RandDouble(double minzero = 1.0e-4)
        {
            double d = RandDoubleRaw(1.0, 2.0, minzero) - 1.0;
            return (d <= minzero) ? 0.0 : d;
        }

        // RandDoubleRaw() handles subnormals, but Min & Max must both
        // be subnormal or not subnormal.  You cannot generate randoms
        // between a subnormal and a regular double, we will throw.
        //
        // It is highly recommended to experiment with these ranges,
        // since ieee754 double precision floating point has some 
        // idiosyncracies.  Too small a zero, and the random range
        // becomes too large, and the results are poorly distributed,
        // tending to cluster around 0.
        //
        // Random subnormals only work well when their exponents
        // are the same or only 1 order of magnitude apart.
        //
        // For fp64 encoding info see:
        // https://en.wikipedia.org/wiki/Double-precision_floating-point_format
        //
        // Ranges (negative or positive):
        //
        //  +/- 2.2250738585072014 × 10^−308 (Min normal double) to
        //  +/- 1.7976931348623157 × 10^308  (Max normal double)
        //
        // and
        //
        //  +/- 4.9406564584124654 × 10^−324 (Min subnormal double) to
        //  +/- 2.2250738585072009 × 10^−308 (Max subnormal double)
        //
        // Other relevant info about ieee-754 double precision numbers:
        //
        //     For reference:
        //      18,446,744,073,709,551,615 UInt64.Max
        //      -9,223,372,036,854,775,808 Int64.Min
        //       9,223,372,036,854,775,807 Int64.Max
        //
        //     Doubles:
        //      −9,007,199,254,740,992  Double Min Integer Exactly Representable
        //       9,007,199,254,740,992  Double Max Integer Exactly Representable
        //  +/- 18,014,398,509,481,984  Double Max Integer Representable By 2x (i.e. n mod 2 == 0)
        //  +/- 36,028,797,018,963,968  Double Max Integer Representable By 4x (i.e. n mod 4 == 0)
        //  +/- Integers 2^n to 2^(n+1)                    Representable By 2n^(-52)x
        //
        // MinZero is the practical minimum when zero is part of the range
        // MinZero is +0 for subnormals
        // MinZero prevents range compression
        public double RandDoubleRaw(double Min, double Max, double MinZero = 1e-4)
        {
            // No NaNs or INF
            if (double.IsNaN(Min) || double.IsNaN(Max) ||
                double.IsInfinity(Min) || double.IsInfinity(Max))
                throw new ArgumentException("You cannot use infinities or NaNs for Min or Max! Use actual numeric double values instead.");

            // Both normal or both subnormal required
#if !NET5_0_OR_GREATER
            if (!(Dub.IsSubnormal(Min) == Dub.IsSubnormal(Max)))
#else
            if (!(double.IsSubnormal(Min) == double.IsSubnormal(Max)))
#endif
                throw new ArgumentException("You cannot mix subnormal and normal doubles for Min & Max! Choose both subnormals or both normals.");

            // Swap Min, Max if necessary
            // Easier to reason about if we know
            // d1 <= d2 ALWAYS
            if (Min > Max) (Min, Max) = (Max, Min);

            // Adjust MinZero if dealing with subnormals
#if !NET5_0_OR_GREATER
            bool sn = Dub.IsSubnormal(Min);
#else
            bool sn = double.IsSubnormal(Min);
#endif
            if (sn) MinZero = +0;
            else
            {
                // No subnormal, so see if the Abs(Min) is less
                // than MinZero or if Min == 0.0
                double absmin = Math.Abs(Min);

                if (absmin != 0.0 && absmin < MinZero)
                    MinZero = absmin;
                else if (Min == 0.0)
                    Min = MinZero;
            }

            Dub mz = new Dub(MinZero);
            Dub d1 = new Dub(Min);
            Dub d2 = new Dub(Max);

            // First, figure out what our sign is going to be
            // ->both ++ then +, both -- then -, one of each then random
            bool rnd_neg = (d1.IsNeg == d2.IsNeg) ? d1.IsNeg : (RangedRand16S(short.MinValue, short.MaxValue) < 0);
            bool same_sign = (d1.IsNeg == d2.IsNeg);

            // Default to 0s for subnormals (exp is always 0)
            uint exp_min = 0;
            uint exp_max = 0;
            uint new_exp = 0;

            // Generate a new exponent if necessary
            if (!sn)
            {
                // New Exponent Range
                exp_min = Dub.EXP_MIN;
                exp_max = Dub.EXP_MAX;

                if (same_sign)
                {
                    // if they are the same sign, we don't really care that
                    // min has a greater exp than max if they're negative,
                    // because the random functions adjust on the fly
                    exp_min = d1.Exp;
                    exp_max = d2.Exp;
                }
                else
                {
                    // they are not the same sign; the min exp will
                    // always be MinZero, however because Min and
                    // Max straddle the zero line we have to check
                    // which one we're randomizing toward
                    exp_min = mz.Exp;
                    exp_max = (rnd_neg) ? d1.Exp : d2.Exp;
                }

                new_exp = RangedRand32(exp_min, exp_max);
            }

            // Work on the fractional part
            ulong frac_min = (ulong)0;
            ulong frac_max = Dub.FRAC_BITS;

            // We only need to mess with the fraction ranges
            // if the exponents are equal
            if (new_exp == exp_min || new_exp == exp_max)
            {
                if (same_sign)
                {
                    if (exp_min == exp_max)
                    {
                        frac_min = d1.Frac;
                        frac_max = d2.Frac;
                    }
                    else
                    {
                        if (new_exp == exp_min)
                            frac_min = d1.Frac;
                        else
                            frac_max = d2.Frac;
                    }
                }
                else
                {
                    if (rnd_neg)
                        frac_max = d1.Frac;
                    else
                        frac_max = d2.Frac;
                }

                // For integer powers of 2^n
                if (frac_max == 0) { new_exp--; frac_min = (ulong)0; frac_max = Dub.FRAC_BITS; }
            }

            // Build the new double
            ulong new_frac = RangedRand64(frac_min, frac_max);
            ulong d = new_frac;
            if (rnd_neg) d |= Dub.SIGN_BIT;
            d |= ((ulong)new_exp << 52);
#if NET6_0_OR_GREATER
            return BitConverter.UInt64BitsToDouble(d);
#else
            return BitConverter.ToDouble(BitConverter.GetBytes(d), 0);
#endif
        }
    }

}
