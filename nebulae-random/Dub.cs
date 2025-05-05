using System;
using System.Collections.Generic;
using System.Text;

namespace nebulae.dub
{
    public class Dub
    {
        public static readonly int EXP_BIAS = 0x3ff;
        public static readonly uint EXP_MIN = 1;
        public static readonly uint EXP_MAX = 0x7fe;
        public static readonly ulong SIGN_BIT = ((ulong)1 << 63);
        public static readonly ulong FRAC_BITS = (ulong)0xF_FFFF_FFFF_FFFF;

        private bool _neg;
        private uint _exp;
        private ulong _frac;

        public bool IsNeg { get { return _neg; } }
        public uint Exp { get { return _exp; } }
        public bool HasNegExp { get { return (_exp < EXP_BIAS); } }
        public int UnbiasedExp { get { return (int)_exp - EXP_BIAS; } }
        public ulong Frac { get { return _frac; } }

#if !NET5_0_OR_GREATER
        public static bool IsSubnormal(double value)
        {
            if (value == 0.0) return false;

            ulong bits = BitConverter.ToUInt64(BitConverter.GetBytes(value), 0);
            uint exponent = (uint)((bits >> 52) & 0x7FF);   // Extract exponent bits
            ulong mantissa = bits & 0xFFFFFFFFFFFFF;        // Extract mantissa bits

            return exponent == 0 && mantissa != 0;
        }
#endif
        public Dub(double InDub)
        {
#if NET6_0_OR_GREATER
                ulong db = BitConverter.DoubleToUInt64Bits(InDub);
#else
            ulong db = BitConverter.ToUInt64(BitConverter.GetBytes(InDub), 0);
#endif

            _neg = (db & SIGN_BIT) != 0;
            _exp = (uint)(((db & ~SIGN_BIT) & ~FRAC_BITS) >> 52);
            _frac = (db & FRAC_BITS);
        }
    }
}
