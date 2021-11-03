using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SharedMemoryIPC.Tests
{
    [TestFixture]
    public class PipeBenchmarkTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public async Task Test01Async()
        {
            await using var pipeBenchmark = new PipeBenchmark(
                "Test 01:",
                Sizes._1_MB,
                1024,
                1024,
                Sizes._10_MB
            );
            pipeBenchmark.Run();
            await pipeBenchmark.OnTestComplete;
            Console.WriteLine(pipeBenchmark.PrintReport());
        }

        [Test]
        public async Task Test02Async()
        {
            await using var pipeBenchmark = new PipeBenchmark(
                "Test 01:",
                Sizes._10_MB,
                100,
                100,
                Sizes._100_MB
            );
            pipeBenchmark.Run();
            await pipeBenchmark.OnTestComplete;
            Console.WriteLine(pipeBenchmark.PrintReport());
        }
    }

    public static class Sizes
    {
        public const int _1_KB = 1024;
        public const int _1_MB = 1024 * _1_KB;
        public const int _10_MB = 10 * _1_MB;
        public const int _50_MB = 50 * _1_MB;
        public const int _100_MB = 100 * _1_MB;
    }
}
/*

446,347 msg/sec, TimePerOneMessage=2.24 us, Messages=10,000,000, Elapsed=00:00:22.4040861

*/
