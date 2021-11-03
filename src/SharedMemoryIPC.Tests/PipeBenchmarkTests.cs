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
        public async Task CombinedTests()
        {
            await Test01Async();
            await Test02Async();
            await Test03Async();
            await Test04Async();
            await Test05Async();
        }


        [Test]
        public async Task Test01Async()
        {
            await using var pipeBenchmark = new PipeBenchmark(
                "Test 01:",
                Sizes._1_KB,
                10 * 1024,
                1_000_000,
                100
            );
            pipeBenchmark.Run();
            await pipeBenchmark.OnTestComplete;
            Console.WriteLine(pipeBenchmark.PrintReport());
        }

        [Test]
        public async Task Test02Async()
        {
            await using var pipeBenchmark = new PipeBenchmark(
                "Test 02:",
                Sizes._1_KB,
                10 * 1024,
                1_000_000,
                Sizes._1_KB
            );
            pipeBenchmark.Run();
            await pipeBenchmark.OnTestComplete;
            Console.WriteLine(pipeBenchmark.PrintReport());
        }

        [Test]
        public async Task Test03Async()
        {
            await using var pipeBenchmark = new PipeBenchmark(
                "Test 03:",
                Sizes._1_KB,
                10 * 1024,
                100_000,
                Sizes._10_KB
            );
            pipeBenchmark.Run();
            await pipeBenchmark.OnTestComplete;
            Console.WriteLine(pipeBenchmark.PrintReport());
        }

        [Test]
        public async Task Test04Async()
        {
            await using var pipeBenchmark = new PipeBenchmark(
                "Test 04:",
                Sizes._1_MB,
                1024,
                10_000,
                Sizes._1_MB
            );
            pipeBenchmark.Run();
            await pipeBenchmark.OnTestComplete;
            Console.WriteLine(pipeBenchmark.PrintReport());
        }

        [Test]
        public async Task Test05Async()
        {
            await using var pipeBenchmark = new PipeBenchmark(
                "Test 05:",
                Sizes._10_MB,
                100,
                1_000,
                Sizes._10_MB
            );
            pipeBenchmark.Run();
            await pipeBenchmark.OnTestComplete;
            Console.WriteLine(pipeBenchmark.PrintReport());
        }

        //[Test]
        //public async Task __Test02Async()
        //{
        //    await using var pipeBenchmark = new PipeBenchmark(
        //        "Test 01:",
        //        Sizes._10_MB,
        //        100,
        //        100,
        //        Sizes._100_MB
        //    );
        //    pipeBenchmark.Run();
        //    await pipeBenchmark.OnTestComplete;
        //    Console.WriteLine(pipeBenchmark.PrintReport());
        //}
    }

    public static class Sizes
    {
        public const int _1_KB = 1024;
        public const int _10_KB = 10 * _1_KB;
        public const int _1_MB = 1024 * _1_KB;
        public const int _10_MB = 10 * _1_MB;
        public const int _50_MB = 50 * _1_MB;
        public const int _100_MB = 100 * _1_MB;
    }
}
/*

446,347 msg/sec, TimePerOneMessage=2.24 us, Messages=10,000,000, Elapsed=00:00:22.4040861

*/
