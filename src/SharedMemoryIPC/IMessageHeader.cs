namespace SharedMemoryIPC
{
    public interface IMessageHeader
    {
        bool Skip { get; set; }
        int PayloadSize { get; set; }
    }
}