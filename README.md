# nebulae-random

### A collection of random number generators for most every need.

---

Implements a common interface for random number generation, similar to the older ISAAC64 library. The goal is to provide a simple and consistent interface for generating random numbers, no matter what the underlying generator is.

This library implements the following generators:

1.  ISAAC64
2.  Xoshiro128+
3.  Xoshiro128++
4.  Xoshiro128**
5.  Xoshiro256+
6.  Xoshiro256++
7.  Xoshiro256**
8.  Xoshiro512+
9.  Xoshiro512++
10. Xoshiro512**
11. Xoshiro1024++
12. Xoshiro1024*
13. Xoshiro1024**
14. Mersenne Twister (MT19937) 32-bit
15. Mersenne Twister (MT19937) 64-bit
16. PCG32
17. MWC128
18. MWC192
19. MWC256
20. GMWC128
21. GMWC256
22. Splitmix64

Each generator is seeded slightly differntly, but there is a common interface for all of them. Most can be seeded with 1, 2 or 3 64-bit unsigned integers, and all provide a default constructor that will seed them using the system's cryptographic rng.

---

Constructors will throw exceptions if used unseeded (0 or empty arrays), or if the passed arrays exceed the prescribed size limits (no silent ignore). The flags are provided to allow overriding this behavior should your use case require it.

### Methods:

1. `RandN(_size_ Max)`, where N is 64/32/16/8.  These methods return unsigned integers of the corresponding size in the range [0, Max]
2. `RangedRandN(_size_ Min, _size_ Max)` methods return unsigned integers in the range [Min, Max]; the `RangedRandNS(_size_ Min, _size_ Max)` variants return signed integers instead
3. `RandAlphaNum(bool Upper, bool Lower, bool Numeric, char[]? symbols)` generates a char using the range(s) specified, optionally also using the symbols array if provided (bias is eliminated in all cases)
4. `RandDouble()` returns a 64-bit double-precision float in the range (0.0, 1.0)
5. `RandDoubleRaw(double Min, double Max, double MinZero = 1e-3)` generates a double in the range (Min, Max) using the MinZero parameter as the defacto smallest number (see source for info)
6. `Shuffle()` mixes & rotates the data and refills the RNG buffer (occurs automatically at mod 256 runs)
7. `Reseed()` reseeds the RNG from ground zero at any time; has variants mirroring the class constructors
8. `Clone()` returns a new instance of the Rng with a complete clone of the current RNG's state; this allows you to "fork" the RNG and run multiple independent RNGs, all of which will start with identical state from the point of Clone(). Useful for using the same RNG state in multiple functions or threads.

### Mimic of System.Random for 32-bit Ints:

1. `Next()`: returns a 32-bit unsigned integer in the range [0, 2^32)
2. `Next(int Max)`: returns a 32-bit unsigned integer in the range [0, Max)
3. `Next(int Min, int Max)`: returns a 32-bit unsigned integer in the range [Min, Max)

When pulling a data type smaller than 64-bits, the remaining bytes of the 8-byte chunk are banked until you request that same type size again.

### Notes on Random Doubles:

All doubles pull a 64-bit integer for the mantissa/fraction. Regular doubles may also pull a 16-bit integer for the sign bit and one 32-bit integer for the exponent. If the specified Min & Max are the same sign, no integer is pulled.  Likewise, if Min & Max share a common exponent, no integer will be pulled. Subnormal doubles (extremely small < 10^-308) will never pull an integer for their exponent, and may or may not pull an integer for their sign, exactly the same as regular doubles.

### Etc:

If there are any bugs or diversions from the specs, please reach out.

### N