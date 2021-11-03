using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;

namespace SharedMemoryIPC
{
    public class PipeSink<TMessageHeader> : PipeBase<TMessageHeader>, IAsyncDisposable
        where TMessageHeader : struct, IMessageHeader
    {
        private const int ReceiveRequestTimeoutMs = 500;

        #region Ctor & fields

        private readonly Action<TMessageHeader, Stream, long, MemoryMappedFile> _onMessageCallback;
        private readonly IAsyncDisposable _listener;

        private bool _isGateOpen;
        private bool _isDisposed;
        private int _nextChunkIndex;

        /// <summary>
        /// </summary>
        /// <param name="memoryMappedFileName"></param>
        /// <param name="onMessageCallback">If the caller uses protobuf parser having AllFileViewStream won't be enough. Protobuf parser doesn't accept offset and size arguments, the caller will need to create new stream from the MemoryMappedFile using the size from messageHeader and offset argument</param>
        /// <param name="chunkSize"></param>
        /// <param name="numberOfChunks"></param>
        public PipeSink(
            string memoryMappedFileName,
            Action<TMessageHeader, Stream, long, MemoryMappedFile> onMessageCallback,
            int chunkSize,
            int numberOfChunks
        ) : base(true, memoryMappedFileName, chunkSize, numberOfChunks)
        {
            _onMessageCallback = onMessageCallback;
            _listener = StartListening();
        }

        #endregion

        public bool IsInErrorState { get; private set; }
        public event EventHandler<Exception> Error;

        public void OpenGate()
        {
            if (_isDisposed)
                throw new ObjectDisposedException($"{GetType()} for '{MemoryMappedFileName}' has been disposed already");

            if (_isGateOpen) return;
            _isGateOpen = true;

            ChannelReadySemaphore.Release(NumberOfChunks);
        }

        public void CloseGate()
        {
            if (_isDisposed)
                throw new ObjectDisposedException($"{GetType()} for '{MemoryMappedFileName}' has been disposed already");

            if (!_isGateOpen) return;
            _isGateOpen = false;

            for (var i = 0; i < NumberOfChunks; i++)
                ChannelReadySemaphore.WaitOne();
        }

        private IAsyncDisposable StartListening()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var listeningTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            // Release lock periodically to let cancellation work
                            if (!DataReadySemaphore.WaitOne(ReceiveRequestTimeoutMs))
                                continue;

                            var numberOfChunksToFitTheMessage = 1;
                            try
                            {
                                if (cancellationToken.IsCancellationRequested) return;

                                AllFileViewStream.Seek(_nextChunkIndex * ChunkSize, SeekOrigin.Begin);
                                var messageHeader = HeaderSerializer.Read(AllFileViewStream);

                                numberOfChunksToFitTheMessage = GetNumberOfChunksToFitTheMessage(messageHeader.PayloadSize);

                                // Capture additional semaphores to cover all the message
                                for (var i = 1; i < numberOfChunksToFitTheMessage; i++)
                                    DataReadySemaphore.WaitOne();

                                if (!messageHeader.Skip)
                                {
                                    _onMessageCallback(
                                        messageHeader,
                                        AllFileViewStream,
                                        AllFileViewStream.Position,
                                        MemoryMappedFile
                                    );
                                }
                            }
                            catch (Exception exception)
                            {
                                // Better to handle exceptions inside the callback. This is just a safety measure
                            }
                            finally
                            {
                                // Release channel anyways
                                _nextChunkIndex = (_nextChunkIndex + numberOfChunksToFitTheMessage) % NumberOfChunks;
                                ChannelReadySemaphore.Release(numberOfChunksToFitTheMessage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IsInErrorState = true;
                        var error = Error;
                        error?.Invoke(this, ex);
                    }
                },
                CancellationToken.None, // Don't bother with exceptions, just exit gracefully all the times
                TaskCreationOptions.LongRunning, // Make sure the caller's thread isn't blocked
                TaskScheduler.Default
            );

            return AsyncDisposableEx.Create(async () =>
            {
                cancellationTokenSource.Cancel(false);
                await listeningTask;
            });
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (_isGateOpen)
            {
                for (var i = 0; i < NumberOfChunks; i++)
                    ChannelReadySemaphore.WaitOne();
            }

            await _listener.DisposeAsync();
            base.Dispose();
        }
    }
}