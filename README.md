# nebulae-random

### A collection of random number generators for most every need.

[![NuGet](https://img.shields.io/nuget/v/nebulae.rng.svg)](https://www.nuget.org/packages/nebulae.rng/)

#### [Nuget Package](https://www.nuget.org/packages/nebulae.rng/)

---

Implements a common interface for random number generation, similar to our older ISAAC64 library. The goal is to provide a simple and consistent interface for generating random numbers, no matter what the underlying generator is. With this library, you can try several different generators by simply switching up the constructor.

---

install with:

$ dotnet add package nebulae.rng

---

Note that these are not cryptographically secure RNGs (although ISAAC64 *is* a CSPRNG, the C# code here is not guaranteed to be constant time, so it could be vulnerable to side-channel attacks). 

These RNGs provide default constructors that are seeded via a crytographically secure system RNG (/dev/random or /dev/urandom on Unix/Linux or Microsoft's Windows Crypto Provider), so the predictability should be near zero, especially if the initial seed and the RNG state are not shared.

That means these RNGs are not suitable for key derivation or authenication / authorization purposes (such as salt generation); that being said, they are secure for the purposes of gaming & simulation and most other general purposes (i.e. they are not predictable unless you know the seed).

If you need a CSPRNG, use the system provider from the .NET framework you are using, or look into something like ChaCha20, PRNGs based on ECDSA curves, a constant time version of ISAAC64, or RC4 (among others).

Further reading:

[Wikipedia on CSPRNGs](https://en.wikipedia.org/wiki/Cryptographically_secure_pseudorandom_number_generator)

[Practical Cryptography for Developers](https://cryptobook.nakov.com/secure-random-generators/secure-random-generators-csprng)

---

#### This library implements the following generators:

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
20. GMWC128(*)
21. GMWC256
22. Splitmix64

Each generator is seeded slightly differntly, but there is a common interface for all of them. Most can be seeded with 1, 2 or 3 64-bit unsigned integers, and all provide a default constructor that will seed them using the system's cryptographic rng.

Every generator has been tested against the default reference implementations provided by their respective authors. This ensures the behavior of this implementation does not deviate from the original specifications. All tests are included in the GitHub repository.

#### (*) GMWC128 does not support LongJump() in this implementation- there was a disagreement over the sequence generated between the original C code, the C++ code using the Boost MPC library and the C# code, so I left that functionality out. Jump() is still supported, however.

---

Constructors will throw exceptions if used unseeded (0 or empty arrays). The flags are provided to allow overriding this behavior should your use case require it.

### Methods:

#### Namespace is nebulae.rng

The RangedRandN{S} functions support bias elimination via modulo sampling; more than one random may be burned in these functions if the generated number falls outside the mod range.

1. `RandN(_size_ Max)`, where N is 64/32/16/8.  These methods return unsigned integers of the corresponding size in the range [0, Max]
2. `RandNS(_size_ Max)` methods return signed integers in the range of [_size_.MinValue, _size_ Max] (i.e. between the signed type's minimum value and the specified Max value, inclusive).
3. `RangedRandN(_size_ Min, _size_ Max)` methods return unsigned integers in the range [Min, Max]; the `RangedRandNS(_size_ Min, _size_ Max)` variants return signed integers instead
4. `RandAlphaNum(bool Upper, bool Lower, bool Numeric, char[] symbols = null)` generates a char using the range(s) specified, optionally also using the symbols array if provided (bias is eliminated in all cases)
5. `RandDouble()` returns a 64-bit double-precision float in the range [0.0, 1.0)
6. `RandDoubleRaw(double Min, double Max, double MinZero = 1e-4)` generates a double in the range (Min, Max) using the MinZero parameter as the defacto smallest number (see below for info)
7. `Reseed()` reseeds the RNG from ground zero at any time; has variants mirroring the class constructors
8. `Clone()` returns a new instance of the Rng with a complete clone of the current RNG's state; this allows you to "fork" the RNG and run multiple independent RNGs, all of which will start with identical state from the point of Clone(). Useful for using the same RNG state in multiple functions or threads.
9. `Jump()` jumps the rng sequence ahead (usually by 2^64 or more) for parallel simulations using the same seed. Not supported by all generators.
10. `LongJump()` jumps the rng sequence ahead (usually by 2^128 or more) for parallel simulations using the same seed. Not supported by all generators.

### Mimic of System.Random API for 32-bit Ints:

1. `Next()`: returns a 32-bit unsigned integer in the range [0, 2^32)
2. `Next(int Max)`: returns a 32-bit unsigned integer in the range [0, Max)
3. `Next(int Min, int Max)`: returns a 32-bit unsigned integer in the range [Min, Max)
4. `NextDouble()`: returns a 64-bit double precision floating point value in the range [0.0, 1.0)

#### When pulling a data type smaller than 64-bits, the remaining bytes of the 8-byte chunk are banked until you request that same type size again.

### Notes on Random Doubles:

All doubles pull a 64-bit integer for the mantissa (fraction). Regular doubles may also pull a 16-bit integer for the sign bit and a 32-bit integer for the exponent. If the specified Min & Max are the same sign, no integer is pulled; likewise, if Min & Max share a common exponent, no integer will be pulled. Subnormal doubles (extremely small < +/- 10^-308) will never pull an integer for their exponent, and may or may not pull an integer for their sign, exactly the same as regular doubles. Because of the range compression inherent in the ieee754 encoding of floating point doubles (there are many more values close to zero), you can specify a minimum value (MinZero) that is the cutoff for what should be considered *zero*. The library defaults to 1e-4, but RandomDoubleRaw() will let you specify.

---

### Example Usage:

```csharp
Xoshiro256starstar rng = new Xoshiro256starstar(0x987654321UL)
var random64 = rng.Rand64(); // [0, ulong.MaxValue]
var random32 = rng.Rand32(); // [0, uint.MaxValue]
var random8 = rng.RangedRand8(0, 100); // [0, 100]
var random8_signed = rng.RangedRand8S(-10, 10) // [-10, 10]
var signed_short = rng.Rand16S(); // [short.MinValue, short.MaxValue]

var cloned_rng = rng.Clone();

var val1 = cloned_rng.Rand64();
var val2 = rng.Rand64();

if (val1 == val2)
    Console.WriteLine("Clone() call was successful!");

// New rng getting seed from system crypto provider
PCG32 rng2 = new PCG32();

var random_double = rng2.NextDouble();

// Using System.Random API
var next = rng2.Next();       // [0, uint.MaxValue)
var next2 = rng2.Next(0x5555) // [0, 0x5555)

```
---

### Etc:

If there are any bugs or diversions from the specs, please reach out.

### N