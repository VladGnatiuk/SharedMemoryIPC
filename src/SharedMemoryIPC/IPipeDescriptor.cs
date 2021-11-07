namespace SharedMemoryIPC
{
    public interface IPipeDescriptor
    {
        string PipeName { get; }
        int ChunkSize { get; }
        int NumberOfChunks { get; }
    }
}