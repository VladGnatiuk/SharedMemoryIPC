using System.IO.MemoryMappedFiles;

namespace SharedMemoryIPC
{
    public delegate void WriteToStreamDelegate(MemoryMappedViewStream stream);
}