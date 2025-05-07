using System;
using System.IO;
using nebulae.rng;

namespace nebulae.rng.dump
{
    class DumpRng
    {
        const int BytesPerGB = 1_073_741_824;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: DumpRng <RngName> <OutputFile>");
                return;
            }

            string rngName = args[0];
            string outputPath = args[1];

            BaseRng rng = rngName switch
            {
                "Xoshiro128plus" => new Xoshiro128plus(),
                "Xoshiro128plusplus" => new Xoshiro128plusplus(),
                "Xoshiro128starstar" => new Xoshiro128starstar(),
                "Xoshiro256plus" => new Xoshiro256plus(),
                "Xoshiro256plusplus" => new Xoshiro256plusplus(),
                "Xoshiro256starstar" => new Xoshiro256starstar(),
                "Xoshiro512plus" => new Xoshiro512plus(),
                "Xoshiro512plusplus" => new Xoshiro512plusplus(),
                "Xoshiro512starstar" => new Xoshiro512starstar(),
                "Xoshiro1024plusplus" => new Xoshiro1024plusplus(),
                "Xoshiro1024star" => new Xoshiro1024star(),
                "Xoshiro1024starstar" => new Xoshiro1024starstar(),
                "PCG32" => new PCG32(),
                "ISAAC64" => new Isaac64(),
                "SplitMix64" => new Splitmix(),
                "MT19937_32" => new MT19937_32(),
                "MT19937_64" => new MT19937_64(),
                "MWC128" => new MWC128(),
                "MWC192" => new MWC192(),
                "MWC256" => new MWC256(),
                "GMWC128" => new GMWC128(),
                "GMWC256" => new GMWC256(),
                _ => throw new ArgumentException($"Unknown RNG: {rngName}")
            };

            using FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter writer = new BinaryWriter(fs);

            Console.WriteLine($"Generating 1GB of output from {rngName}...");

            int numUInts = BytesPerGB / sizeof(uint);
            for (int i = 0; i < numUInts; i++)
            {
                writer.Write(rng.Rand32());
            }

            Console.WriteLine($"Done: {outputPath}");
        }
    }
}