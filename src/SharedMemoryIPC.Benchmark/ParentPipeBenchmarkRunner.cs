using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;

namespace SharedMemoryIPC.Benchmark
{
    public class ParentPipeBenchmarkRunner : IAsyncDisposable
    {
        #region Ctor & fields

        private readonly int _pipeId;
        private readonly int _chunkSize;
        private readonly int _numberOfChunks;
        private readonly int _numberOfMessagesToSend;
        private readonly int _messageSize;
        private readonly PipeSink<BenchmarkMessageHeader> _sink;
        private Process _childProcess;

        public ParentPipeBenchmarkRunner(
            int pipeId,
            int chunkSize,
            int numberOfChunks,
            int numberOfMessagesToSend,
            int messageSize,
            Action<BenchmarkMessageHeader, Stream, long, MemoryMappedFile> processMessage
        )
        {
            //Console.WriteLine($"pipeId={pipeId}, chunkSize={chunkSize}, numberOfChunks={numberOfChunks}, messageSize={messageSize}, numberOfMessagesToSend={numberOfMessagesToSend}");

            _pipeId = pipeId;
            _chunkSize = chunkSize;
            _numberOfChunks = numberOfChunks;
            _numberOfMessagesToSend = numberOfMessagesToSend;
            _messageSize = messageSize;

            _sink = new PipeSink<BenchmarkMessageHeader>(
                $"benchmark-pipe-{pipeId}",
                processMessage,
                chunkSize,
                numberOfChunks
            );
        }

        #endregion

        public void RunChildProcess()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            var path = commandLineArgs[0];
            path = path.Substring(0, path.Length - 3) + "exe";
            var args = $"runAs=Child pipeId={_pipeId} chunkSize={_chunkSize} numberOfChunks={_numberOfChunks} numberOfMessagesToSend={_numberOfMessagesToSend} messageSize={_messageSize}";

            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = args,
                UseShellExecute = true,
            };
            _childProcess = Process.Start(startInfo);
        }

        public void OpenGate() => _sink.OpenGate();

        public async ValueTask DisposeAsync()
        {
            try
            {
                _childProcess.Kill();
            }
            catch { }

            await _sink.DisposeAsync();
        }
    }
}