# nebulae-random

### A collection of random number generators for most every need.

All RNGs in nebulae-random pass a 10 million-sample bucket uniformity test across 100 bins with <1.2% max deviation (most are generally <1.0%). This includes 64-bit integer sampling via bias-free rejection methods and high-resolution floating point generation using full 53-bit precision.

[![NuGet](https://img.shields.io/nuget/v/nebulae.rng.svg)](https://www.nuget.org/packages/nebulae.rng/)

#### [Nuget Package](https://www.nuget.org/packages/nebulae.rng/)

---

nebaule-random implements a common interface for random number generation, similar to our older ISAAC64 library. The goal is to provide a simple and consistent interface for generating random numbers, no matter what the underlying generator is. With this library, you can try several different generators by simply switching up the constructor.

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
17. PCG64 
18. MWC128
19. MWC192
20. MWC256
21. GMWC128(*)
22. GMWC256
23. Splitmix64

Each generator is seeded slightly differntly, but there is a common interface for all of them. Most can be seeded with 1, 2 or 3 64-bit unsigned integers, and all provide a default constructor that will seed them using the system's cryptographic rng.

Every generator has been tested against the default reference implementations provided by their respective authors. This ensures the behavior of this implementation does not deviate from the original specifications. All tests are included in the GitHub repository.

#### (*) GMWC128 does not support LongJump() in this implementation- there was a disagreement over the sequence generated between the original C code, the C++ code using the Boost MPC library and the C# code, so I left that functionality out. Jump() is still supported, however.

---

Constructors will throw exceptions if used unseeded (0 or empty arrays). The flags are provided to allow overriding this behavior should your use case require it.

## Methods:

### Namespace is nebulae.rng

### Integer RNG Methods:

All integer functions support bias elimination using modulo rejection sampling. Multiple random numbers may be consumed (burned) if the generated value falls outside the acceptable modular range.

1. `RandN(_size_ Max)`, where N is 64/32/16/8. Returns an unsigned integer in the range [0, Max] (inclusive).
2. `RandNS(_size_ Max)` Returns a signed integer in the range [T.MinValue, Max], where T is the signed integer type of _size_.
3. `RangedRandN(_size_ Min, _size_ Max)` Returns an unsigned integer in the range [Min, Max].
4. `RangedRandNS(_size_ Min, _size_ Max)` Returns a signed integer in the range [Min, Max].
5. `RandAlphaNum(bool Upper, bool Lower, bool Numeric, char[] symbols = null)` Returns a random character based on the selected ranges (uppercase, lowercase, digits, or custom symbols). Bias is eliminated regardless of which options are selected.

### Double RNG Methods:

1. `RandDoubleExclusiveZero()` Uses full 53-bit precision. Excludes 0.0 to prevent edge cases in logarithmic or exponential sampling. Minzero defines the minimum representable nonzero value, defaulting to 2^-53 \~1.11e-16.
2. `RandDoubleInclusiveZero()` Returns a double in the range [0.0, 1.0). This matches the behavior of System.Random.NextDouble() and includes 0.0.
3. `RandDoubleLinear(double Min, double Max)` Returns a double in [Min, Max) using linear interpolation. Safe for general-purpose simulations and float-based algorithms.
4. `RandDoubleRaw(double Min, double Max, double MinZero = MINZERO_DEFAULT)` Constructs a raw IEEE-754 double between min and max. Gives full control over sign, exponent, and mantissa layout. Both min and max must be either normal or subnormal doubles. Mixing types throws. Not intended for casual use; this is a precision tool.
5. `RandDouble53()` Equivalent to RandDoubleInclusiveZero(). Returns a 53-bit mantissa float in [0.0, 1.0).

MINZERO_DEFAULT is available as a public constant in each RNG and is equal to 1.0 / (1 << 53) or \~1.11e-16.

### RNG State Control Methods:

1. `Reseed()` Reseeds the generator from scratch, optionally with a user-specified seed or entropy source.
2. `Clone()` Returns a new RNG instance with a cloned internal state. Useful for deterministic replay or parallel simulations that must diverge from a common state.
3. `Jump()` Advances the generator's state (usually by 2^64 or more). Ideal for creating independent substreams in parallel simulations. Not supported by all generators.
4. `LongJump()` Advances the generator's state (usually by 2^128 or more). Not supported by all generators.

### System.Random Compatibility:

1. `Next()`: returns a 32-bit unsigned integer in the range [0, 2^32)
2. `Next(int Max)`: returns a 32-bit unsigned integer in the range [0, Max)
3. `Next(int Min, int Max)`: returns a 32-bit unsigned integer in the range [Min, Max)
4. `NextDouble()`: Returns a double in [0.0, 1.0). Internally calls RandDoubleInclusiveZero().

#### When pulling a data type smaller than 64-bits, the remaining bytes of the 8-byte chunk are banked until you request that same type size again.

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

// Jump functions
Xoshiro1024plusplus rng = new Xoshiro1024plusplus();
var rng2 = rng.Clone();
var rng3 = rng.Clone();
var rng4 = rng.Clone();

// Demonstration
rng.LongJump(); // advance 2^384 iterations
rng2.Jump(); // advance 2^256 iterations
rng3.Jump(); rng3.Jump(); // etc.
rng4.Jump(); rng4.Jump(); rng4.Jump(); // etc.

// now rng2, rng3 & rng4 are equally spaced by 2^256 steps each,
// and rng is spaced out to 2^384 steps from the start, all with the same seed.

```
---

### Etc:

If there are any bugs or diversions from the specs, please reach out.

### N