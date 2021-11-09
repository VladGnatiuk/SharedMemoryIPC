using System;
using System.IO;

namespace SharedMemoryIPC
{
    public class PipeSource<TMessageHeader> : PipeBase<TMessageHeader>
        where TMessageHeader : struct, IMessageHeader
    {
        #region Ctor & fields

        private readonly object _concurrentSendLock = new();

        private int _nextChunkIndex;
        private bool _isDisposed;

        public PipeSource(
            string memoryMappedFileName,
            int chunkSize,
            int numberOfChunks
        ) : base(false, memoryMappedFileName, chunkSize, numberOfChunks) { }

        public PipeSource(IPipeDescriptor pipeDescriptor) 
            : this(pipeDescriptor.PipeName, pipeDescriptor.ChunkSize, pipeDescriptor.NumberOfChunks) { }

        #endregion

        public void Send(ref TMessageHeader messageHeader, WriteToStreamDelegate writeToStream)
        {
            lock (_concurrentSendLock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException($"{GetType()} for '{MemoryMappedFileName}' has been disposed already");

                SendSafe(ref messageHeader, writeToStream);
            }
        }

        private void SendSafe(ref TMessageHeader messageHeader, WriteToStreamDelegate writeToStream)
        {
            var numberOfChunksToFitTheMessage = GetNumberOfChunksToFitTheMessage(messageHeader.PayloadSize);
            if (numberOfChunksToFitTheMessage > NumberOfChunks)
                throw new ArgumentOutOfRangeException($"Can't fit payload of size {messageHeader.PayloadSize:N0} bytes and a header of size {HeaderSerializer.StructSize:N0} bytes into the memory mapped file");

            EnsureCanFitMessageSequentially(numberOfChunksToFitTheMessage);
            CaptureChannelReadySemaphore(numberOfChunksToFitTheMessage);
            WriteData(ref messageHeader, writeToStream);

            _nextChunkIndex = (_nextChunkIndex + numberOfChunksToFitTheMessage) % NumberOfChunks;

            DataReadySemaphore.Release(numberOfChunksToFitTheMessage);
        }

        /// <summary>
        /// If at the end of the buffer and can't fit multi-chunk payload sequentially - yield chunks until at the beginning of the buffer
        /// </summary>
        /// <param name="numberOfChunksToFitTheMessage"></param>
        private void EnsureCanFitMessageSequentially(int numberOfChunksToFitTheMessage)
        {
            if (_nextChunkIndex + numberOfChunksToFitTheMessage <= NumberOfChunks) return;

            var skipChunkMessageHeader = new TMessageHeader
            {
                Skip = true,
                PayloadSize = 0
            };
            while (_nextChunkIndex != 0)
                SendSafe(ref skipChunkMessageHeader, null);
        }

        private void CaptureChannelReadySemaphore(int numberOfChunksToCapture)
        {
            for (var i = 0; i < numberOfChunksToCapture; i++)
            {
                try
                {
                    if (!ChannelReadySemaphore.WaitOne(Timeouts.RequestReceivedEventHandleTimeoutMs))
                        throw new RemotePartyIsNotResponsiveException($"Timed out on event handle '{NamingHelper.GetChannelReadySemaphoreName(MemoryMappedFileName)}'");
                }
                catch
                {
                    // If the contra peer is unresponsive - don't send the message and release already captured chunks
                    if (i > 0)
                        ChannelReadySemaphore.Release(i);

                    throw;
                }
            }
        }

        private void WriteData(ref TMessageHeader messageHeader, WriteToStreamDelegate writeToStream)
        {
            AllFileViewStream.Seek(_nextChunkIndex * ChunkSize, SeekOrigin.Begin);
            HeaderSerializer.Write(AllFileViewStream, ref messageHeader);

            if (messageHeader.PayloadSize > 0)
                writeToStream(AllFileViewStream);

            // No flush needed since memory stream writes directly into memory w/o any buffer
        }

        public override void Dispose()
        {
            lock (_concurrentSendLock)
            {
                if (_isDisposed) return;
                _isDisposed = true;

                base.Dispose();
            }
        }
    }
}