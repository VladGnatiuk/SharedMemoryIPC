using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SharedMemoryIPC.Benchmark
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Contains("runAs=Child"))
                await new Program().RunAsChild(args);
            else
                await new Program().RunAsParent(args);
        }

        private readonly TaskCompletionSource<Unit> _taskCompletionSource = new();
        private int _processedCnt;
        private int _numberOfMessagesToExpect;
        private readonly Stopwatch _stopwatch = new();

        private async Task RunAsParent(string[] args)
        {
            var x = ParseArgs(args);
            _numberOfMessagesToExpect = x.numberOfMessagesToSend;

            var numberOfChunksPerPipe = x.numberOfChunks / x.pipesCount;
            var numberOfMessagesPerPipe = x.numberOfMessagesToSend / x.pipesCount;

            //Console.WriteLine($"Starting {x.pipesCount} child processes...");

            var parentPipeBenchmarkRunners = Enumerable
                .Range(0, x.pipesCount)
                .Select(pipeId => new ParentPipeBenchmarkRunner(
                    pipeId,
                    x.chunkSize,
                    numberOfChunksPerPipe,
                    numberOfMessagesPerPipe,
                    x.messageSize,
                    ProcessRequest
                ))
                .ToList();

            try
            {
                parentPipeBenchmarkRunners.ForEach(r => r.RunChildProcess());
                await Task.Delay(1000);
                parentPipeBenchmarkRunners.ForEach(r => r.OpenGate());

                var timeout = TimeSpan.FromMinutes(2);
                await Task.WhenAny(_taskCompletionSource.Task, Task.Delay(timeout));

                if (_taskCompletionSource.Task.IsCompleted)
                {
                    // Print report
                    var totalBytes = (double)_processedCnt * x.messageSize;
                    var bytesPerSecond = totalBytes / _stopwatch.Elapsed.TotalSeconds;
                    var messagesPerSecond = _processedCnt / _stopwatch.Elapsed.TotalSeconds;
                    var timePerOneMessage = _stopwatch.Elapsed / _processedCnt;

                    var sb = new StringBuilder();
                    //sb.AppendLine($"Test result:");
                    sb.AppendLine($"==============================================================================================================================================================================================================");
                    sb.Append($"NumberOfPipes={x.pipesCount:N0}");
                    sb.Append($"; {bytesPerSecond / 1024 / 1024:N2} MB/sec");
                    sb.Append($"; {messagesPerSecond:N0} msg/sec");
                    sb.Append($"; TimePerOneMessage={timePerOneMessage.TotalMilliseconds:N3} ms");
                    sb.Append($"; NumberOfMessages={_processedCnt:N0}");
                    sb.Append($"; MessageSize={x.messageSize:N0}");
                    sb.Append($"; ChunkSize={x.chunkSize:N0}");
                    sb.Append($"; NumberOfChunks={x.numberOfChunks:N0}");
                    sb.Append($"; AllData={totalBytes / 1024 / 1024:N0} MB");
                    sb.Append($"; Elapsed={_stopwatch.Elapsed}");
                    sb.AppendLine();
                    sb.AppendLine($"==============================================================================================================================================================================================================");

                    Console.WriteLine(sb.ToString());
                }
                else
                {
                    Console.WriteLine($"Timed out after {timeout}");
                }
            }
            finally
            {
                await Task.WhenAll(parentPipeBenchmarkRunners.Select(r => r.DisposeAsync().AsTask()));
            }
        }

        private async Task RunAsChild(string[] args)
        {
            // pipeId=0 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=100
            var x = ParseArgs(args);
            using var childPipeBenchmarkRunner = new ChildPipeBenchmarkRunner(
                x.pipeId,
                x.chunkSize,
                x.numberOfChunks,
                x.numberOfMessagesToSend,
                x.messageSize
            );
            await Task.Run(childPipeBenchmarkRunner.Run);

            Console.ReadKey();
        }

        static (
            int pipesCount,
            int pipeId,
            int chunkSize,
            int numberOfChunks,
            int numberOfMessagesToSend,
            int messageSize
        ) ParseArgs(string[] args)
        {
            var d = args
                .Select(s => s.Split('='))
                .ToDictionary(a => a[0], a => a[1]);

            int pipesCount = d.TryGetValue("pipesCount", out var pipesCountStr) ? int.Parse(pipesCountStr) : 1;
            int pipeId = d.TryGetValue("pipeId", out var pipeIdStr) ? byte.Parse(pipeIdStr) : 0;
            int chunkSize = int.Parse(d["chunkSize"]);
            int numberOfChunks = int.Parse(d["numberOfChunks"]);
            int numberOfMessagesToSend = int.Parse(d["numberOfMessagesToSend"]);
            int messageSize = int.Parse(d["messageSize"]);

            return (pipesCount, pipeId, chunkSize, numberOfChunks, numberOfMessagesToSend, messageSize);
        }

        void ProcessRequest(BenchmarkMessageHeader messageHeader, Stream stream, long offset, MemoryMappedFile memoryMappedFile)
        {
            try
            {
                // Console.WriteLine($"On received: offset={offset}, {JsonConvert.SerializeObject(messageHeader)}");

                var buffer = ArrayPool<byte>.Shared.Rent(messageHeader.PayloadSize);
                try
                {
                    stream.Read(buffer, 0, messageHeader.PayloadSize);
                    BenchmarkMessageHeader.AssertCheckValues(ref messageHeader, buffer[0], buffer[messageHeader.PayloadSize - 1]);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                Interlocked.Increment(ref _processedCnt);
                if (_processedCnt == 1)
                {
                    _stopwatch.Start();
                }
                else if (_processedCnt == _numberOfMessagesToExpect)
                {
                    _stopwatch.Stop();

                    // Let handler complete
                    Task.Delay(100).ContinueWith(_ => _taskCompletionSource.TrySetResult(Unit.Default));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
