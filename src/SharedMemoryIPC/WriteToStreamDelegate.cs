using System.IO;
using System.IO.MemoryMappedFiles;

namespace SharedMemoryIPC
{
    public delegate void WriteToStreamDelegate(MemoryMappedViewStream stream);
    public delegate void OnMessageDelegate<TMessageHeader>(TMessageHeader messageHeader, Stream stream, long offset, MemoryMappedFile memoryMappedFile);
}