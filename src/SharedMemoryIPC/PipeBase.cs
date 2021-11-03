using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace SharedMemoryIPC
{
    public abstract class PipeBase<TMessageHeader> : IDisposable
        where TMessageHeader : struct, IMessageHeader
    {
        #region Ctor & fields

        protected readonly MemoryMappedFile MemoryMappedFile;
        protected readonly string MemoryMappedFileName;
        protected readonly int ChunkSize;
        protected readonly int NumberOfChunks;
        protected readonly StructSerializer<TMessageHeader> HeaderSerializer;
        protected readonly MemoryMappedViewStream AllFileViewStream;
        protected readonly Semaphore ChannelReadySemaphore;
        protected readonly Semaphore DataReadySemaphore;

        protected PipeBase(
            bool isSink,
            string memoryMappedFileName,
            int chunkSize,
            int numberOfChunks)
        {
            MemoryMappedFileName = memoryMappedFileName;
            ChunkSize = chunkSize;
            NumberOfChunks = numberOfChunks;

            HeaderSerializer = new StructSerializer<TMessageHeader>();
            if (HeaderSerializer.StructSize > chunkSize)
                throw new ArgumentOutOfRangeException($"Message header {typeof(TMessageHeader)} is large than the chunk size ({HeaderSerializer.StructSize} > {chunkSize})");

            try
            {
                if (isSink)
                {
                    MemoryMappedFile = MemoryMappedFile.CreateNew(MemoryMappedFileName, (long)ChunkSize * NumberOfChunks);
                    AllFileViewStream = MemoryMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
                    ChannelReadySemaphore = new Semaphore(0, NumberOfChunks, NamingHelper.GetChannelReadySemaphoreName(MemoryMappedFileName));
                    DataReadySemaphore = new Semaphore(0, NumberOfChunks, NamingHelper.GetDataReadySemaphoreName(MemoryMappedFileName));
                }
                else
                {
                    MemoryMappedFile = MemoryMappedFile.OpenExisting(MemoryMappedFileName);
                    AllFileViewStream = MemoryMappedFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Write);
                    ChannelReadySemaphore = Semaphore.OpenExisting(NamingHelper.GetChannelReadySemaphoreName(MemoryMappedFileName));
                    DataReadySemaphore = Semaphore.OpenExisting(NamingHelper.GetDataReadySemaphoreName(MemoryMappedFileName));
                }
            }
            catch
            {
                DoClose();
                throw;
            }
        }

        #endregion

        protected int GetNumberOfChunksToFitTheMessage(int payloadSize)
        {
            var totalMessageSize = payloadSize + HeaderSerializer.StructSize;
            return (int)(totalMessageSize / ChunkSize + (totalMessageSize % ChunkSize > 0 ? 1 : 0));
        }

        void DoClose()
        {
            ChannelReadySemaphore?.Dispose();
            DataReadySemaphore?.Dispose();
            AllFileViewStream?.Dispose();
            MemoryMappedFile?.Dispose();
        }

        public virtual void Dispose()
        {
            DoClose();
        }
    }
}