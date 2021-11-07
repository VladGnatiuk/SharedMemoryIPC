using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SharedMemoryIPC.Tests
{
    [TestFixture]
    public class DuplexPipeTests
    {
        #region Srtup/Teardown

        private TaskCompletionSource<Unit> _taskCompletionSource;
        private int _numberOfMessages;
        private int _processedCnt;
        private Stopwatch _stopwatch;

        private DuplexPipe<TestMessageHeader> _componentA_DuplexPipe;
        private DuplexPipe<TestMessageHeader> _componentB_DuplexPipe;
        private List<(TestMessageHeader, byte[])> _componentA_IncomingMessages;
        private List<(TestMessageHeader, byte[])> _componentB_IncomingMessages;

        [SetUp]
        public async Task SetUp()
        {
            _processedCnt = 0;
            _taskCompletionSource = new TaskCompletionSource<Unit>();
            _stopwatch = new Stopwatch();

            Console.WriteLine($"{nameof(TestMessageHeader)} size={Marshal.SizeOf(typeof(TestMessageHeader))}");

            _componentA_IncomingMessages = new List<(TestMessageHeader, byte[])>();
            _componentB_IncomingMessages = new List<(TestMessageHeader, byte[])>();

            var pipeDescriptor_A2B = new TestPipeDescriptor
            {
                PipeName = "A2B",
                ChunkSize = 1024,
                NumberOfChunks = 1024
            };
            var pipeDescriptor_B2A = new TestPipeDescriptor
            {
                PipeName = "B2A",
                ChunkSize = 1024,
                NumberOfChunks = 1024
            };
            _componentA_DuplexPipe = new DuplexPipe<TestMessageHeader>(
                pipeDescriptor_A2B,
                pipeDescriptor_B2A,
                ProcessMessage_B2A
            );
            _componentB_DuplexPipe = new DuplexPipe<TestMessageHeader>(
                pipeDescriptor_B2A,
                pipeDescriptor_A2B,
                ProcessMessage_A2B
            );
            _componentA_DuplexPipe.Connect();
            _componentB_DuplexPipe.Connect();
        }

        [TearDown]
        public async Task TearDown()
        {
            _componentA_DuplexPipe.Disconnect();
            _componentB_DuplexPipe.Disconnect();

            await _componentA_DuplexPipe.DisposeAsync();
            await _componentB_DuplexPipe.DisposeAsync();
        }

        #endregion

        [Test]
        public async Task CorrectnessTest()
        {
            _numberOfMessages = 1_000;

            var messagesA2B = Enumerable.Range(0, _numberOfMessages)
                .Select(i => (
                        new TestMessageHeader
                        {
                            MessageId = i,
                            Ticks = DateTime.Now.Ticks,
                            PayloadSize = 10,
                        },
                        GetPayload(10, i)
                ))
                .ToList();

            var messagesB2A = Enumerable.Range(0, _numberOfMessages)
                .Select(i => (
                        new TestMessageHeader
                        {
                            MessageId = i,
                            Ticks = DateTime.Now.Ticks,
                            PayloadSize = 20,
                        },
                        GetPayload(20, i)
                ))
                .ToList();

            _stopwatch.Start();
            var _1 = Task.Run(async () =>
            {
                await Task.Yield();
                foreach (var x in messagesA2B)
                {
                    var header = x.Item1;
                    _componentA_DuplexPipe.Send(ref header, s => s.Write(x.Item2, 0, header.PayloadSize));
                }

            });
            var _2 = Task.Run(async () =>
            {
                await Task.Yield();

                foreach (var x in messagesB2A)
                {
                    var header = x.Item1;
                    _componentB_DuplexPipe.Send(ref header, s => s.Write(x.Item2, 0, header.PayloadSize));
                }

            });
            await _taskCompletionSource.Task;
            _stopwatch.Stop();

            Console.WriteLine($"Elapsed={_stopwatch.Elapsed}");

            CollectionAssert.AreEqual(messagesB2A, _componentA_IncomingMessages);
            CollectionAssert.AreEqual(messagesA2B, _componentB_IncomingMessages);
        }

        [Test]
        public async Task PerformanceTest()
        {
            _numberOfMessages = 10_000;
            var payloadSize = 1000;
            Console.WriteLine($"NumberOfMessages={_numberOfMessages:N0} each way, payloadSize={payloadSize:N0}");

            var b1 = GetPayload(payloadSize, 0);
            var b2 = GetPayload(payloadSize, payloadSize);

            _stopwatch.Start();
            var _1 = Task.Run(async () =>
            {
                await Task.Yield();
                foreach (var i in Enumerable.Range(0, _numberOfMessages))
                {
                    var header = new TestMessageHeader
                    {
                        MessageId = i,
                        Ticks = DateTime.Now.Ticks,
                        PayloadSize = payloadSize,
                    };
                    _componentA_DuplexPipe.Send(ref header, s => s.Write(b1, 0, payloadSize));
                }
            });
            var _2 = Task.Run(async () =>
            {
                await Task.Yield();
                foreach (var i in Enumerable.Range(0, _numberOfMessages))
                {
                    var header = new TestMessageHeader
                    {
                        MessageId = i,
                        Ticks = DateTime.Now.Ticks,
                        PayloadSize = payloadSize,
                    };
                    _componentA_DuplexPipe.Send(ref header, s => s.Write(b2, 0, payloadSize));
                }
            });
            await _taskCompletionSource.Task;
            _stopwatch.Stop();

            Console.WriteLine($"Elapsed={_stopwatch.Elapsed}");
            /*
                TestMessageHeader size=24
                NumberOfMessages=1,000,000, payloadSize=1,000
                Elapsed=00:00:09.6187071
            
                TestMessageHeader size=24
                NumberOfMessages=100,000 each way, payloadSize=1,000
                Elapsed=00:00:01.0104112

                TestMessageHeader size=24
                NumberOfMessages=10,000 each way, payloadSize=1,000
                Elapsed=00:00:00.1834010
            
                TestMessageHeader size=24
                NumberOfMessages=1,000 each way, payloadSize=1,000
                Elapsed=00:00:00.0189401

                TestMessageHeader size=24
                NumberOfMessages=100 each way, payloadSize=1,000
                Elapsed=00:00:00.0081555
            
                TestMessageHeader size=24
                NumberOfMessages=10 each way, payloadSize=1,000
                Elapsed=00:00:00.0068083

            */
        }

        #region Routines

        byte[] GetPayload(int n, int shift) => Enumerable.Range(0, n).Select(i => (byte)(i + shift)).ToArray();

        private void ProcessMessage_B2A(TestMessageHeader messageHeader, Stream stream, long offset, MemoryMappedFile memoryMappedFile)
        {
            var bytes = new byte[messageHeader.PayloadSize];
            stream.Read(bytes, 0, messageHeader.PayloadSize);
            _componentA_IncomingMessages.Add((messageHeader, bytes));

            CheckTestCompletion();
        }

        private void ProcessMessage_A2B(TestMessageHeader messageHeader, Stream stream, long offset, MemoryMappedFile memoryMappedFile)
        {
            var bytes = new byte[messageHeader.PayloadSize];
            stream.Read(bytes, 0, messageHeader.PayloadSize);
            _componentB_IncomingMessages.Add((messageHeader, bytes));

            CheckTestCompletion();
        }

        private void CheckTestCompletion()
        {
            bool endOfTest;
            lock (_taskCompletionSource)
            {
                _processedCnt++;
                endOfTest = _processedCnt == _numberOfMessages * 2; // both ways
            }
            if (endOfTest)
                _taskCompletionSource.TrySetResult(Unit.Default);
        }

        #endregion

        #region Nested classes

        public struct TestMessageHeader : IMessageHeader
        {
            public bool Skip { get; set; }
            public int PayloadSize { get; set; }

            public int MessageId { get; set; }
            public long Ticks { get; set; }
        }

        class TestPipeDescriptor : IPipeDescriptor
        {
            public string PipeName { get; set; }
            public int ChunkSize { get; set; }
            public int NumberOfChunks { get; set; }
        }

        #endregion
    }
}