using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;

namespace SharedMemoryIPC.Tests
{
    public class PipeBenchmark : IAsyncDisposable
    {
        private readonly string _testName;

        #region Ctor & fields

        private int _processedCnt;
        private readonly TaskCompletionSource<Unit> _taskCompletionSource = new();
        private readonly Stopwatch _stopwatch = new();
        private StructSerializer<BenchmarkMessageHeader> _structSerializer;
        private readonly PipeSink<BenchmarkMessageHeader> _sink;
        private readonly PipeSource<BenchmarkMessageHeader> _source;

        public PipeBenchmark(
            string testName,
            int chunkSize,
            int numberOfChunks,
            int numberOfMessagesToSend,
            int messageSize
        )
        {
            _testName = testName;
            ChunkSize = chunkSize;
            NumberOfChunks = numberOfChunks;
            NumberOfMessagesToSend = numberOfMessagesToSend;
            MessageSize = messageSize;
            _structSerializer = new StructSerializer<BenchmarkMessageHeader>();
            PayloadSize = MessageSize - _structSerializer.StructSize;

            var testId = Guid.NewGuid().ToString();

            _sink = new PipeSink<BenchmarkMessageHeader>(
                testId,
                ProcessRequest,
                ChunkSize,
                NumberOfChunks
            );
            _sink.OpenGate();

            _source = new PipeSource<BenchmarkMessageHeader>(
                testId,
                ChunkSize,
                NumberOfChunks
            );
        }

        #endregion

        public int ChunkSize { get; }
        public int NumberOfChunks { get; }
        public int NumberOfMessagesToSend { get; }
        public int MessageSize { get; }
        public int PayloadSize { get; }
        public Task OnTestComplete => _taskCompletionSource.Task;

        public double TotalBytes { get; set; }
        public double BytesPerSecond { get; private set; }
        public double MessagesPerSecond { get; private set; }
        public TimeSpan TimePerOneMessage { get; private set; }

        public void Run()
        {
            var payload = GetPayload(PayloadSize);

            Task.Factory.StartNew(() =>
                {
                    _stopwatch.Start();
                    for (int i = 0; i < NumberOfMessagesToSend; i++)
                    {
                        var messageHeader = new BenchmarkMessageHeader
                        {
                            PayloadSize = PayloadSize
                        };
                        _source.Send(ref messageHeader, stream => stream.Write(payload, 0, payload.Length));
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        public string PrintReport()
        {
            var sb = new StringBuilder();
            sb.Append(_testName);
            sb.Append($" {BytesPerSecond / 1024 / 1024:N2} MB/sec");
            sb.Append($", {MessagesPerSecond:N0} msg/sec");
            sb.Append($", TimePerOneMessage={TimePerOneMessage.TotalMilliseconds:N3} ms");
            sb.Append($", NumberOfMessages={NumberOfMessagesToSend:N0}");
            sb.Append($", MessageSize={MessageSize:N0}");
            sb.Append($", ChunkSize={ChunkSize:N0}");
            sb.Append($", NumberOfChunks={NumberOfChunks:N0}");
            sb.Append($", Total MB={TotalBytes / 1024 / 1024:N0}");
            sb.Append($", Elapsed={_stopwatch.Elapsed}");
            return sb.ToString();
        }

        byte[] GetPayload(int n) => Enumerable.Range(0, n).Select(i => (byte)i).ToArray();

        private void ProcessRequest(BenchmarkMessageHeader messageHeader, Stream stream, long offset, MemoryMappedFile memoryMappedFile)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(messageHeader.PayloadSize);
            try
            {
                stream.Read(buffer, 0, messageHeader.PayloadSize);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }            

            if (++_processedCnt != NumberOfMessagesToSend) return;

            _stopwatch.Stop();
            GenerateTestResult();

            // Let handler complete
            Task.Delay(100).ContinueWith(_ => _taskCompletionSource.TrySetResult(Unit.Default));
        }

        private void GenerateTestResult()
        {
            if (_stopwatch.Elapsed.TotalSeconds > 0)
            {
                TotalBytes = (double)NumberOfMessagesToSend * MessageSize;
                BytesPerSecond = TotalBytes / _stopwatch.Elapsed.TotalSeconds;
                MessagesPerSecond = NumberOfMessagesToSend / _stopwatch.Elapsed.TotalSeconds;
                TimePerOneMessage = _stopwatch.Elapsed / NumberOfMessagesToSend;
            }
        }


        public struct BenchmarkMessageHeader : IMessageHeader
        {
            public bool Skip { get; set; }
            public int PayloadSize { get; set; }
        }

        public async ValueTask DisposeAsync()
        {
            _source.Dispose();
            await _sink.DisposeAsync();
        }
    }
}