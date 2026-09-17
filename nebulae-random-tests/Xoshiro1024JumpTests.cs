using nebulae.rng;

namespace nebulae.rng.tests;

public class Xoshiro1024JumpTests
{
    // Expected values generated with clang from the authors' C implementations:
    // https://prng.di.unimi.it/xoroshiro1024plusplus.c
    // https://prng.di.unimi.it/xoroshiro1024star.c
    // https://prng.di.unimi.it/xoroshiro1024starstar.c
    // Set p = 0 and s[i] = i + 1; call next() warmup times, then jump()
    // or long_jump(), then record 20 next() outputs (crossing a ring wrap).
    public static IEnumerable<object[]> ReferenceCases()
    {
            yield return new object[] { "plusplus", 0, false, new ulong[]
            {
                0xbb1cbe470fb29842UL, 0x853906315344b3bfUL, 0xf5888eaa0d8c9556UL, 0x9a3f0b8a011e1ac1UL,
                0xfbd81f620c0290f2UL, 0x019f06bf0aabde7bUL, 0x8f5b9da43ac58628UL, 0xfe47db7bd65fc395UL,
                0x4eb12096c34a0256UL, 0x1c17626595ecd293UL, 0xbe6a68b1d3f51386UL, 0x163c173e492755b2UL,
                0x11767e5a31e98ebdUL, 0x61906d797d25c426UL, 0xaf0d6589d3dc7d60UL, 0xd79e182ed40766e7UL,
                0xc3a6399487741444UL, 0x4f2002e92cc5e6eeUL, 0x21a00a2f8440ac82UL, 0xbc9fb1c0486e4db1UL,
            } };
            yield return new object[] { "plusplus", 0, true, new ulong[]
            {
                0x0f128418d5ea7a35UL, 0x32129e812c0d8a39UL, 0xe628802b292a34e4UL, 0x3087eb7b309859b6UL,
                0xdcc080661956d64fUL, 0x3c9526a150c82674UL, 0x452c0e1016b18aa7UL, 0xadce3cde0362d509UL,
                0x65cbfaf549b2af6aUL, 0xf6842d375e0dbecfUL, 0x9265037587b3636cUL, 0x59d0c572f921ab48UL,
                0xa7814a386a576973UL, 0x26c7709982b56284UL, 0x362e5217951c844aUL, 0x0cd351da8c03e3b3UL,
                0xbe190af25625a020UL, 0xeb47e7577dc622b5UL, 0x171d7b31e3e341a3UL, 0xc77351e2f5f89557UL,
            } };
            yield return new object[] { "plusplus", 7, false, new ulong[]
            {
                0xfe47db7bd65fc395UL, 0x4eb12096c34a0256UL, 0x1c17626595ecd293UL, 0xbe6a68b1d3f51386UL,
                0x163c173e492755b2UL, 0x11767e5a31e98ebdUL, 0x61906d797d25c426UL, 0xaf0d6589d3dc7d60UL,
                0xd79e182ed40766e7UL, 0xc3a6399487741444UL, 0x4f2002e92cc5e6eeUL, 0x21a00a2f8440ac82UL,
                0xbc9fb1c0486e4db1UL, 0x968d42fa040d2becUL, 0xc87a76858630c0f2UL, 0xa33c47a644420cc9UL,
                0x09c3b0dada452d73UL, 0xbc6b36bf06563c57UL, 0x99b1b6cf23793abfUL, 0x5c7faa6fd2864dc8UL,
            } };
            yield return new object[] { "plusplus", 7, true, new ulong[]
            {
                0xadce3cde0362d509UL, 0x65cbfaf549b2af6aUL, 0xf6842d375e0dbecfUL, 0x9265037587b3636cUL,
                0x59d0c572f921ab48UL, 0xa7814a386a576973UL, 0x26c7709982b56284UL, 0x362e5217951c844aUL,
                0x0cd351da8c03e3b3UL, 0xbe190af25625a020UL, 0xeb47e7577dc622b5UL, 0x171d7b31e3e341a3UL,
                0xc77351e2f5f89557UL, 0xda3dc267103cff10UL, 0x897d74a8846432e6UL, 0x6831243494f4f2cfUL,
                0x979529e71d6247b4UL, 0x784076f4892dc1f3UL, 0x53e6acf10b9302c8UL, 0x780e1d8def3d4efdUL,
            } };
            yield return new object[] { "star", 0, false, new ulong[]
            {
                0x40e0d395abaa1eeaUL, 0xe33d4ff744105c15UL, 0x91b8ac05a3d37b80UL, 0x8a5c62064011cdb9UL,
                0x871bc4e5b6956c66UL, 0x0da86f6f393ff0b8UL, 0x683f5bb88bd3a74fUL, 0xc5f87f1bcaee66f5UL,
                0x12e285d262191bf3UL, 0x009f04ad50bf81c7UL, 0x462db7d6baa4e931UL, 0xe3be658bc9b33588UL,
                0xbfc8095f019b7b90UL, 0x76ea602ddae7f1b3UL, 0x6b19f42d360be356UL, 0x0881f552404c6980UL,
                0xc7ef479903bc6176UL, 0x3d960dde6f3261caUL, 0x1b56a998084bd1eaUL, 0x5e9e8e63d9f65cd1UL,
            } };
            yield return new object[] { "star", 0, true, new ulong[]
            {
                0xdfbfdc8548267c12UL, 0x3167e3fbde440155UL, 0x20201d1c445346e7UL, 0x3144e77bd2451dc8UL,
                0x365d94b75c063308UL, 0xbc2196e41bc48fbbUL, 0xaf85376b907fd09cUL, 0xaa19216f0c39c8b6UL,
                0x8db5caa9db9ae0f9UL, 0x00f30f7d362c884eUL, 0x17f97e50942c65f2UL, 0x6d96481dc34b855fUL,
                0xad563eb6a2019affUL, 0xdfdfd3b35ce4726fUL, 0x6bececadc67c80d2UL, 0x8285a944c3aca0a8UL,
                0xc2251db60966debbUL, 0xde62b87a3820b51bUL, 0xbbe0f5e0cd063d27UL, 0x76d5f7ea9755c45bUL,
            } };
            yield return new object[] { "star", 7, false, new ulong[]
            {
                0xc5f87f1bcaee66f5UL, 0x12e285d262191bf3UL, 0x009f04ad50bf81c7UL, 0x462db7d6baa4e931UL,
                0xe3be658bc9b33588UL, 0xbfc8095f019b7b90UL, 0x76ea602ddae7f1b3UL, 0x6b19f42d360be356UL,
                0x0881f552404c6980UL, 0xc7ef479903bc6176UL, 0x3d960dde6f3261caUL, 0x1b56a998084bd1eaUL,
                0x5e9e8e63d9f65cd1UL, 0x341d9c0b0af9df0aUL, 0x78b8ab9e5706ae69UL, 0x673851201971b9f1UL,
                0x284d5360a0f2dac0UL, 0x7333192839e8c41eUL, 0xb29cdd0c20fd9a65UL, 0xc01a6426a9aec13dUL,
            } };
            yield return new object[] { "star", 7, true, new ulong[]
            {
                0xaa19216f0c39c8b6UL, 0x8db5caa9db9ae0f9UL, 0x00f30f7d362c884eUL, 0x17f97e50942c65f2UL,
                0x6d96481dc34b855fUL, 0xad563eb6a2019affUL, 0xdfdfd3b35ce4726fUL, 0x6bececadc67c80d2UL,
                0x8285a944c3aca0a8UL, 0xc2251db60966debbUL, 0xde62b87a3820b51bUL, 0xbbe0f5e0cd063d27UL,
                0x76d5f7ea9755c45bUL, 0xadc0de74dc48c16eUL, 0xc35b3776ac2b5f6dUL, 0xf1e22cdf39e70eafUL,
                0x34a21bbdb494af65UL, 0xf47eaa0649e98782UL, 0xea0b10f8bc51f5cdUL, 0xc965e3f5127cbb64UL,
            } };
            yield return new object[] { "starstar", 0, false, new ulong[]
            {
                0x06a136c7e8ea4f53UL, 0x4bad8bd57faad931UL, 0x79d3b4ca0a124024UL, 0xdcf7137933e383f5UL,
                0xb7ebc89ce29e0e68UL, 0x2d5055fec9a4a6f4UL, 0xa1e5021350acfc5eUL, 0x0732dd86ba19686bUL,
                0x4e77068d4b042aeeUL, 0xfcf430de4ac4bd07UL, 0x04fb1b3d9fbaca62UL, 0x6b4cafb12c603fa8UL,
                0xa73b1ab89ae83bf0UL, 0xd1d0f8c4fe814797UL, 0xd3ab6ca1ea2c967aUL, 0xe32f34cabcdb427fUL,
                0x35c6a0ba10d98509UL, 0x1f47b6d57b3cdc0eUL, 0x45919cbe1230ccefUL, 0xf18e50d9a97cf927UL,
            } };
            yield return new object[] { "starstar", 0, true, new ulong[]
            {
                0xe7ff95756ab2b97fUL, 0x775012b138103739UL, 0xdcbfb646156e3031UL, 0x82dba64835c41e2eUL,
                0xabb53d1be1717d4dUL, 0x21d88c8a0bdd42e3UL, 0x7fa515df313e3677UL, 0x0e8059f7a71de5bdUL,
                0x210484c860d1e47cUL, 0xd6bb5cfd7e4f1b76UL, 0x158dc92a17d1ca69UL, 0xf6fabf6ec2d3f0d1UL,
                0x0f0855e391052362UL, 0xf22c3a200d13693dUL, 0x2675f88f2f385940UL, 0xf4ede658d076b014UL,
                0x37a2540adb55c5fbUL, 0x487531b749ae92f5UL, 0x40fd04bca98b0e8aUL, 0x12178223eb1ef52cUL,
            } };
            yield return new object[] { "starstar", 7, false, new ulong[]
            {
                0x0732dd86ba19686bUL, 0x4e77068d4b042aeeUL, 0xfcf430de4ac4bd07UL, 0x04fb1b3d9fbaca62UL,
                0x6b4cafb12c603fa8UL, 0xa73b1ab89ae83bf0UL, 0xd1d0f8c4fe814797UL, 0xd3ab6ca1ea2c967aUL,
                0xe32f34cabcdb427fUL, 0x35c6a0ba10d98509UL, 0x1f47b6d57b3cdc0eUL, 0x45919cbe1230ccefUL,
                0xf18e50d9a97cf927UL, 0x236675a7d2b63d7fUL, 0xedb42f03f2a52c3dUL, 0xb106f4e3115a68d6UL,
                0xa6a2a4c9a45aa1a7UL, 0x89055cc4e42f31d8UL, 0x27d22e81ee79afd2UL, 0x285c087f9b6f42c4UL,
            } };
            yield return new object[] { "starstar", 7, true, new ulong[]
            {
                0x0e8059f7a71de5bdUL, 0x210484c860d1e47cUL, 0xd6bb5cfd7e4f1b76UL, 0x158dc92a17d1ca69UL,
                0xf6fabf6ec2d3f0d1UL, 0x0f0855e391052362UL, 0xf22c3a200d13693dUL, 0x2675f88f2f385940UL,
                0xf4ede658d076b014UL, 0x37a2540adb55c5fbUL, 0x487531b749ae92f5UL, 0x40fd04bca98b0e8aUL,
                0x12178223eb1ef52cUL, 0xb5fbe7b6143e8b6dUL, 0x8d1c4ad9fce22a97UL, 0x5b0bbdc9459d4946UL,
                0x805b54351bcf2fdbUL, 0x525db57e2e27036eUL, 0x1b4cbd143cdafcb3UL, 0x8a60ec3820fbd106UL,
            } };
    }

    [Theory]
    [MemberData(nameof(ReferenceCases))]
    public void Jump_MatchesOriginalC(string variant, int warmup, bool longJump, ulong[] expected)
    {
        ulong[] seeds = Enumerable.Range(1, 16).Select(i => (ulong)i).ToArray();
        BaseRng rng = variant switch
        {
            "plusplus" => new Xoshiro1024plusplus(seeds),
            "star" => new Xoshiro1024star(seeds),
            "starstar" => new Xoshiro1024starstar(seeds),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };

        for (int i = 0; i < warmup; i++) rng.NextRaw64();
        if (longJump) rng.LongJump(); else rng.Jump();

        foreach (ulong value in expected)
            Assert.Equal(value, rng.NextRaw64());
    }
}
