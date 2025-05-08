using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace nebulae.rng
{
    public class Splitmix : BaseRng
    {
        private ulong _state;

        private readonly object _lock = new object();

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
            return new Splitmix(_state);
        }

        /// <summary>
        /// Split() clones the internal context of the rng object and returns a new rng object
        /// This is useful to split the same rng object into multiple rng objects to take
        /// two different paths, but maintain the same repeatability if using known
        /// seeds. This is useful for testing & simulation purposes. Calls Clone() internally.
        /// </summary>
        /// <returns>A newly constructed rng with the same internal state as the instance
        /// Split() was called on</returns>
        public INebulaeRng Split()
        {
            return Clone();
        }

        /// Splitmix() creates a new RNG and seeds the rng object
        /// This variant uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 8 bytes of random data to seed the RNG.
        /// </summary>
        public Splitmix()
        {
            Reseed();
        }

        /// <summary>
        /// Splitmix() constructs the rng object and seeds the rng with the given 64-bit unsigned integer
        /// </summary>
        /// <param name="seed">ulong seed - the seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if 0 is passed as a seed</param>
        /// <returns>the constructed & seeded rng</returns>
        public Splitmix(ulong seed, bool allowZeroSeed = false)
        {
            Reseed(seed, allowZeroSeed);
        }

        /// <summary>
        /// Reseed() re-seeds the rng with the given 64-bit unsigned integer
        /// </summary>
        /// <param name="seed">ulong seed - the seed, as a 64-bit unsigned integer, to use to seed the rng</param>
        /// <param name="allowZeroSeed">bool allowZeroSeed - don't throw an exception if 0 is passed as a seed</param>
        public void Reseed(ulong seed, bool allowZeroSeed)
        {
            if (seed == 0 && !allowZeroSeed)
                throw new ArgumentException("Seed cannot be zero unless allowZeroSeed is true.");

            _state = seed;
        }

        /// Reseed() reseeds the rng object
        /// This variant uses the System.Security.Cryptography.RandomNumberGenerator
        /// component to get 8 bytes of random data to seed the RNG.
        /// </summary>
        public override void Reseed()
        {
            byte[] bytes = new byte[8];
#if NET6_0_OR_GREATER
            bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
#else
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
#endif
            lock (_lock)
            {
                var bytes_array = MemoryMarshal.Cast<byte, ulong>(bytes);
                _state = bytes_array[0];
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
                ulong z = (_state += 0x9e3779b97f4a7c15);
                z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
                z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
                return z ^ (z >> 31);
            }
        }

        /// <summary>
        /// Jump() is not supported in this RNG.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override void Jump()
        {
            throw new NotSupportedException("Jump not supported in Splitmix RNG.");
        }

        /// <summary>
        /// LongJump() is not supported in this RNG.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override void LongJump()
        {
            throw new NotSupportedException("LongJump not supported in Splitmix RNG.");
        }
    }
}
