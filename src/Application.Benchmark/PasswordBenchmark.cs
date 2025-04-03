using Application.Core.tools;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using tools;

namespace Application.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 100)]
    public class PasswordBenchmark
    {
        [Benchmark()]
        public void UseBcrypt()
        {
            var inputString = "admin";

            var r1 = BCrypt.hashpw(inputString, BCrypt.gensalt(12));
        }

        [Benchmark()]
        public void UseHash1()
        {
            var inputString = "admin";

            var r1 = HashDigest.HashByType("SHA-1", inputString);
        }

        [Benchmark()]
        public void UseHash512()
        {
            var inputString = "admin";

            var r1 = HashDigest.HashByType("SHA-512", inputString);
        }
    }
}
