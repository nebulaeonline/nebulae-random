using System;
using System.Collections.Generic;
using System.Text;

namespace nebulae.rng
{
    public interface INebulaeRng
    {
        ulong NextRaw64();
        void Reseed();
        INebulaeRng Clone();
        void Jump();
        void LongJump();
        int Next();
        int Next(int max);
        int Next(int min, int max);
        double NextDouble();
        ulong Rand64(ulong max);
        ulong RangedRand64(ulong min, ulong max);
        long RangedRand64S(long min, long max);
        uint Rand32(uint max);
        uint RangedRand32(uint min, uint max);
        int RangedRand32S(int min, int max);
        ushort Rand16(ushort max);
        ushort RangedRand16(ushort min, ushort max);
        short RangedRand16S(short min, short max);
        byte Rand8(byte max);
        byte RangedRand8(byte min, byte max);
        sbyte RangedRand8S(sbyte min, sbyte max);
    }
}
