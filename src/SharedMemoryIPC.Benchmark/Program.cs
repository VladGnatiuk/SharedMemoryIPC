using System;
using System.Threading.Tasks;
using SharedMemoryIPC.Tests;

namespace SharedMemoryIPC.Benchmark
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pipeBenchmarkTests = new PipeBenchmarkTests();
            await pipeBenchmarkTests.CombinedTests();
        }
    }
}
