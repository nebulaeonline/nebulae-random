using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace nebulae.rng
{
    public class Isaac64 : BaseRng
    {
        // size constants
        private const int ISAAC64_WORD_SZ = 8;
        private const int ISAAC64_SZ_64 = (int)(1 << ISAAC64_WORD_SZ);
        private const int ISAAC64_SZ_8 = (int)(ISAAC64_SZ_64 << 2);
        private const ulong IND_MASK = (ulong)(((ISAAC64_SZ_64) - 1) << 3);

        // concurrency lock
        private readonly object _lock = new object();

        // for mix
        private static readonly int[] MIX_SHIFT = { 9, 9, 23, 15, 14, 20, 17, 14 };

        // state & random data class
        public class Context
        {
            // randrsl
            internal ulong[] rng_buf = new ulong[ISAAC64_SZ_64];

            // mm
            internal ulong[] rng_state = new ulong[ISAAC64_SZ_64];

            // aa, bb, cc
            internal ulong aa, bb, cc;

            // randcnt
            internal int rngbuf_curptr;

            // allows to clone the context for simulation
            // and testing purposes
            internal Context Clone()
            {
                return new Context
                {
                    aa = this.aa,
                    bb = this.bb,
                    cc = this.cc,
                    rngbuf_curptr = this.rngbuf_curptr,
                    rng_buf = (ulong[])this.rng_buf.Clone(),
                    rng_state = (ulong[])this.rng_state.Clone()
                };
            }
        }

        // create a context
        private Context _ctx = new Context();

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
            Isaac64 copy;

            lock (_lock)
            {
                copy = new Isaac64(true); // testing constructor; does not reseed
                copy._ctx = this._ctx.Clone(); // manually assign copied context
                copy._banked8 = new ConcurrentStack<byte>(this._banked8);
                copy._banked16 = new ConcurrentStack<ushort>(this._banked16);
                copy._banked32 = new ConcurrentStack<uint>(this._banked32);
            }
            return copy;
        }

        /// <summary>
        /// Isaac64() constructs the rng object and seeds the rng
        /// This variant of the constructor uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 2048 bytes of random data to seed the RNG.
        /// </summary>
        /// <returns>the constructed & seeded rng</returns>
        public Isaac64()
        {         
            Reseed();
        }

        /// <summary>
        /// Isaac64() constructs the rng object and seeds the rng
        /// This variant of the constructor is for testing
        /// </summary>
        /// <param name="testing">bool Testing - for testing purposes; if false, this will throw</param>
        /// <returns>the constructed & seeded rng</returns>
        public Isaac64(bool testing = false)
        {
            Reseed(0, testing);
        }

        /// <summary>
        /// Isaac64() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of bytes, to use to seed the rng</param>
        /// <param name="ignoreZeroAndOverSZ8Bytes">bool IgnoreZeroAndOverSZ8Bytes - don't throw an exception if a zero sized or an oversized array is passed</param>
        /// <exception cref="ArgumentException">if the seed is zero or oversized</exception>"
        /// <returns>the constructed & seeded rng</returns>
        public Isaac64(byte[] seedbytes, bool ignoreZeroAndOverSZ8Bytes = false)
        {
            Reseed(seedbytes, ignoreZeroAndOverSZ8Bytes);
        }

        /// <summary>
        /// Isaac64() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="seedULongs">ulong[] SeedULongs - the seed, as an array of 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreZeroAndOverSZ64Longs">bool IgnoreZeroAndOverSZ64Longs - don't throw an exception if a zero size or an oversized array is passed</param>
        /// <returns>the constructed & seeded rng</returns>
        public Isaac64(ulong[] seedULongs, bool ignoreZeroAndOverSZ64Longs = false)
        {
            Reseed(seedULongs, ignoreZeroAndOverSZ64Longs);
        }

        /// <summary>
        /// Isaac64() constructs the rng object and seeds the rng
        /// </summary>
        /// <param name="numericSeed">ulong NumericSeed - the seed to use to seed the rng</param>
        /// <param name="ignoreZeroSeed">bool IgnoreZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>the constructed & seeded rng</returns>
        public Isaac64(ulong numericSeed, bool ignoreZeroSeed = false)
        {
            Reseed(numericSeed, ignoreZeroSeed);
        }

        /// <summary>
        /// Shuffle() mixes and re-shuffles the seed data and re-populates the rng array; it does not re-seed anything
        /// </summary>
        /// <returns>none</returns>
        public void Shuffle()
        {
            lock (_lock)
            {
                isaac64();
                reset_curptr();
            }
        }

        public override void Reseed()
        {
            var seed = new byte[ISAAC64_SZ_8];
#if NET6_0_OR_GREATER
            seed = System.Security.Cryptography.RandomNumberGenerator.GetBytes(ISAAC64_SZ_8);
#else
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(seed);
            }
#endif      
            Reseed(seed, false);
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// </summary>
        /// <param name="seedbytes">byte[] SeedBytes - the seed, as an array of bytes, to use to seed the rng</param>
        /// <param name="ignoreZeroAndOverSZ8Bytes">bool IgnoreZeroAndOverSZ8Bytes - don't throw an exception if a zero or oversized array is passed</param>
        /// <returns>none</returns>
        public void Reseed(byte[] seedbytes, bool ignoreZeroAndOverSZ8Bytes = false)
        {
            lock (_lock)
            {
                if (!ignoreZeroAndOverSZ8Bytes && (seedbytes.Length > ISAAC64_SZ_8 || seedbytes.Length == 0))
                    throw new ArgumentException($"Cannot seed ISAAC64 with zero or more than {ISAAC64_SZ_8} bytes! To pass a zero array size or an array size > {ISAAC64_SZ_8}, set IgnoreZeroAndOverSZ8Bytes to true.");

                if (seedbytes.Length == 0)
                {
                    Reseed(0, ignoreZeroAndOverSZ8Bytes);
                    return;
                }

                clear_state();

                for (int i = 0; i < seedbytes.Length; i++)
                {
                    if (i % 8 == 0)
                        _ctx.rng_buf[i / 8] = 0;

                    _ctx.rng_buf[i / 8] |= ((ulong)seedbytes[i] << ((i % 8) * 8));
                }
                init();
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// </summary>
        /// <param name="seedULongs">ulong[] SeedULongs - the seed, as an array of 64-bit unsigned integers, to use to seed the rng</param>
        /// <param name="ignoreZeroAndOverSZ64Longs">bool IgnoreZeroAndOverSZ64Longs - don't throw an exception if an oversized array is passed</param>
        /// <returns>none</returns>
        public void Reseed(ulong[] seedULongs, bool ignoreZeroAndOverSZ64Longs = false)
        {
            lock (_lock)
            {
                if (!ignoreZeroAndOverSZ64Longs && (seedULongs.Length > ISAAC64_SZ_64 || seedULongs.Length == 0))
                    throw new ArgumentException($"Cannot seed ISAAC64 with zero or more than {ISAAC64_SZ_64} ulongs! To pass a zero array size or an array size > {ISAAC64_SZ_64}, set IgnoreZeroAndOverSZ64Longs to true.");


                clear_state();

                int sl = (seedULongs.Length > ISAAC64_SZ_64) ? ISAAC64_SZ_64 : seedULongs.Length;
                for (int i = 0; i < sl; i++)
                    _ctx.rng_buf[i] = seedULongs[i];

                init();
            }
        }

        /// <summary>
        /// Reseed() reseeds the rng
        /// </summary>
        /// <param name="numericSeed">ulong NumericSeed - the seed to use to seed the rng</param>
        /// <param name="ignoreZeroSeed">bool IgnoreZeroSeed - don't throw an exception if seeding with 0</param>
        /// <returns>none</returns>
        public void Reseed(ulong numericSeed, bool ignoreZeroSeed = false)
        {
            lock (_lock)
            {
                clear_state();
                if (ignoreZeroSeed && numericSeed == 0)
                    init(true);
                else if (numericSeed == 0)
                    throw new ArgumentException("Rng seeded with 0 value. Set the IgnoreZeroSeed parameter if this behavior is desired.");
                else
                {
                    _ctx.rng_buf[0] = numericSeed;
                    init();
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
            lock (_lock)
            {
                dec_curptr();
                return _ctx.rng_buf[_ctx.rngbuf_curptr];
            }
        }

        /// <summary>
        /// Not supported in ISAAC64.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override void Jump()
        {
            throw new NotSupportedException("Jump() not supported in ISAAC64.");
        }

        /// <summary>
        /// Not supported in ISAAC64.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override void LongJump()
        {
            throw new NotSupportedException("LongJump() not supported in ISAAC64.");
        }

        // clear the rng state
        private void clear_state()
        {
            for (int i = 0; i < ISAAC64_SZ_64; i++) _ctx.rng_state[i] = (ulong)0;
        }

        // sets the curptr in the rng_buf back to max
        private void reset_curptr()
        {
            _ctx.rngbuf_curptr = ISAAC64_SZ_64;
        }

        // decrements the curptr if possible, if not, shuffles
        private void dec_curptr()
        {
            if (--_ctx.rngbuf_curptr < 0)
            {
                Shuffle();
                _ctx.rngbuf_curptr = ISAAC64_SZ_64 - 1; // explicit
            }
        }

        // Helper method for the isaac64() method
        // state_idx1 -> m  - first index into rng_state
        // state_idx2 -> m2 - second index into rng_state
        // rng_idx    -> r  - index into rng_buf
        // a, b, x, y -> isaac64() local vars (Context.aa, Context.bb, temporaries x & y)
        private void rng_step(ref int state_idx1, ref int state_idx2, ref int rng_idx, ref ulong a, ref ulong b, ref ulong x, ref ulong y)
        {
            x = _ctx.rng_state[state_idx1];

            switch (state_idx1 % 4)
            {
                case 0:
                    a = ~(a ^ (a << 21)) + _ctx.rng_state[state_idx2++];
                    break;
                case 1:
                    a = (a ^ (a >> 5)) + _ctx.rng_state[state_idx2++];
                    break;
                case 2:
                    a = (a ^ (a << 12)) + _ctx.rng_state[state_idx2++];
                    break;
                case 3:
                    a = (a ^ (a >> 33)) + _ctx.rng_state[state_idx2++];
                    break;
            }

            _ctx.rng_state[state_idx1++] = y = ind(x) + a + b;
            _ctx.rng_buf[rng_idx++] = b = ind(y >> ISAAC64_WORD_SZ) + x;
        }

        // Helper method for the isaac64() method
        private ulong ind(ulong x)
        {
            int index = (int)(x & IND_MASK) / 8;
            return _ctx.rng_state[index];
        }

        // Helper method for the isaac64() & init() methods
        private static void mix(ref ulong[] _x)
        {
            for (uint i = 0; i < 8; i++)
            {
                _x[i] -= _x[(i + 4) & 7];
                _x[(i + 5) & 7] ^= _x[(i + 7) & 7] >> MIX_SHIFT[i];
                _x[(i + 7) & 7] += _x[i];
                i++;
                _x[i] -= _x[(i + 4) & 7];
                _x[(i + 5) & 7] ^= _x[(i + 7) & 7] << MIX_SHIFT[i];
                _x[(i + 7) & 7] += _x[i];
            }
        }

        // internal shuffle
        private void isaac64()
        {
            ulong a, b, x, y;
            x = y = 0;

            int state_idx1, state_idx2, rng_idx, end_idx;
            rng_idx = 0;

            a = _ctx.aa;
            b = _ctx.bb + (++_ctx.cc);

            for (state_idx1 = 0, end_idx = state_idx2 = (ISAAC64_SZ_64 / 2); state_idx1 < end_idx;)
                for (int i = 0; i < 4; i++)
                    rng_step(ref state_idx1, ref state_idx2, ref rng_idx, ref a, ref b, ref x, ref y);

            for (state_idx2 = 0; state_idx2 < end_idx;)
                for (int i = 0; i < 4; i++)
                    rng_step(ref state_idx1, ref state_idx2, ref rng_idx, ref a, ref b, ref x, ref y);

            _ctx.bb = b;
            _ctx.aa = a;
        }

        // internal rng init
        private void init(bool Zero = false)
        {
            int i;

            //No need to waste the time on every update

            /*const ulong MAGIC = 0x9E3779B97F4A7C13;


           ulong[] x = { MAGIC, MAGIC,
                         MAGIC, MAGIC,
                         MAGIC, MAGIC,
                         MAGIC, MAGIC };



           for (i = 0; i < 4; i++)
               mix(ref x);*/

            // Save the 4 rounds of mix'ing MAGIC
            ulong[] x = { 0x647c4677a2884b7c, 0xb9f8b322c73ac862,
                          0x8c0ea5053d4712a0, 0xb29b2e824a595524,
                          0x82f053db8355e0ce, 0x48fe4a0fa5a09315,
                          0xae985bf2cbfc89ed, 0x98f5704f6c44c0ab };

            _ctx.aa = _ctx.bb = _ctx.cc = 0;

            for (i = 0; i < ISAAC64_SZ_64; i += 8)
            {
                if (!Zero)
                    for (int j = 0; j < 8; j++)
                        x[j] += _ctx.rng_buf[i + j];

                mix(ref x);

                for (int j = 0; j < 8; j++)
                    _ctx.rng_state[i + j] = x[j];
            }

            if (!Zero)
            {
                for (i = 0; i < ISAAC64_SZ_64; i += 8)
                {
                    for (int j = 0; j < 8; j++)
                        x[j] += _ctx.rng_state[i + j];

                    mix(ref x);

                    for (int j = 0; j < 8; j++)
                        _ctx.rng_state[i + j] = x[j];
                }
            }

            isaac64();
            reset_curptr();
        }
    }
}
