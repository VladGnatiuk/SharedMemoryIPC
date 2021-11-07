using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace SharedMemoryIPC.Tests
{
    [TestFixture]
    public class PipeTests
    {
        private string _testId;
        private TaskCompletionSource<Unit> _taskCompletionSource;
        private int _numberOfMessages;
        private int _processedCnt;
        private Stopwatch _stopwatch;
        private int _chunkSize;
        private int _numberOfChunks;
        private List<(TestMessageHeader messageHeader, long offset, byte[] payload)> _receivedData;
        private int _headerSize;

        [SetUp]
        public void SetUp()
        {
            _testId = $"{GetType()}-{DateTime.Now.Ticks}";

            _processedCnt = 0;
            _taskCompletionSource = new TaskCompletionSource<Unit>();

            _stopwatch = new Stopwatch();

            _headerSize = Marshal.SizeOf(typeof(TestMessageHeader));
            Console.WriteLine($"{nameof(TestMessageHeader)} size={_headerSize}");

            _receivedData = new List<(TestMessageHeader, long, byte[])>();
        }

        [Test]
        public async Task TestPipeAsync()
        {
            _chunkSize = 16;
            _numberOfChunks = 4;
            _numberOfMessages = 3;

            await using var sink = new PipeSink<TestMessageHeader>(
                _testId,
                ProcessMessage,
                _chunkSize,
                _numberOfChunks
            );
            sink.OpenGate();

            using var source = new PipeSource<TestMessageHeader>(
                _testId,
                _chunkSize,
                _numberOfChunks
            );

             await Task.Factory.StartNew(async () =>
                {
                    // _numberOfMessages = 3
                    Send(source, 1);
                    Send(source, _chunkSize);
                    Send(source, _chunkSize);

                    await _taskCompletionSource.Task;
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );

            await Task.WhenAll(_taskCompletionSource.Task);

            Console.WriteLine(JsonConvert.SerializeObject(_receivedData));

            var r0 = _receivedData[0];
            Assert.AreEqual((1, _headerSize, 0), (r0.messageHeader.PayloadSize, r0.offset, r0.payload.Last()));

            var r1 = _receivedData[1];
            Assert.AreEqual((_chunkSize, _chunkSize + _headerSize, _chunkSize - 1), (r1.messageHeader.PayloadSize, r1.offset, r1.payload.Last()));

            var r2 = _receivedData[2];
            Assert.AreEqual((_chunkSize, _headerSize, _chunkSize - 1), (r2.messageHeader.PayloadSize, r2.offset, r2.payload.Last()));
        }

        private void Send(PipeSource<TestMessageHeader> client, int size)
        {
            var messageHeader = new TestMessageHeader
            {
                PayloadSize = size
            };
            client.Send(
                ref messageHeader,
                stream => stream.Write(GetPayload(size), 0, size)
            );
        }

        byte[] GetPayload(int n) => Enumerable.Range(0, n).Select(i => (byte)i).ToArray();

        private void ProcessMessage(TestMessageHeader messageHeader, Stream stream, long offset, MemoryMappedFile memoryMappedFile)
        {
            var bytes = new byte[messageHeader.PayloadSize];
            stream.Read(bytes, 0, messageHeader.PayloadSize);
            _receivedData.Add((messageHeader, offset, bytes));

            if (++_processedCnt == _numberOfMessages)
            {
                Task.Delay(100).ContinueWith(_ => _taskCompletionSource.TrySetResult(Unit.Default));
            }
        }

        public struct TestMessageHeader : IMessageHeader
        {
            public bool Skip { get; set; }
            public int PayloadSize { get; set; }

            public int MessageId { get; set; }
            public long Ticks { get; set; }
        }
    }
}