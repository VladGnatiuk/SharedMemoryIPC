using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SharedMemoryIPC.Benchmark
{
    public class ChildPipeBenchmarkRunner : IDisposable
    {
        #region Ctor & fields

        private readonly int _pipeId;
        private readonly int _numberOfMessagesToSend;
        private readonly int _payloadSize;

        private readonly TaskCompletionSource<Unit> _taskCompletionSource = new();
        private readonly PipeSource<BenchmarkMessageHeader> _source;

        public ChildPipeBenchmarkRunner(
            int pipeId,
            int chunkSize,
            int numberOfChunks,
            int numberOfMessagesToSend,
            int messageSize
        )
        {
            Console.WriteLine($"Starting PipeSource with parameters: pipeId={pipeId}, chunkSize={chunkSize}, numberOfChunks={numberOfChunks}, numberOfMessagesToSend={numberOfMessagesToSend}, messageSize={messageSize}");

            _pipeId = pipeId;
            _numberOfMessagesToSend = numberOfMessagesToSend;

            var structSerializer = new StructSerializer<BenchmarkMessageHeader>();
            _payloadSize = messageSize - structSerializer.StructSize;

            _source = new PipeSource<BenchmarkMessageHeader>(
                $"benchmark-pipe-{pipeId}",
                chunkSize,
                numberOfChunks
            );
        }

        #endregion

        public Task OnTestComplete => _taskCompletionSource.Task;

        public void Run()
        {
            var payload = GetPayload(_payloadSize);

            for (int i = 0; i < _numberOfMessagesToSend; i++)
            {
                var msgId = i + 1;
                var messageHeader = new BenchmarkMessageHeader
                {
                    PayloadSize = payload.Length,
                    PipeId = _pipeId,
                    MessageId = msgId,
                    Ticks = DateTime.Now.Ticks,
                };
                //Console.WriteLine($"Sending: {JsonConvert.SerializeObject(messageHeader)}");
                // Put changing numbers into the first and last position to verify that boundaries have been read correctly
                (payload[0], payload[^1]) = BenchmarkMessageHeader.GenerateCheckValues(ref messageHeader);
                _source.Send(messageHeader, stream => stream.Write(payload, 0, payload.Length));
            }
        }

        byte[] GetPayload(int n) => Enumerable.Range(0, n).Select(i => (byte)i).ToArray();

        public void Dispose()
        {
            _source.Dispose();
        }
    }
}