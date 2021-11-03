namespace SharedMemoryIPC
{
    public static class Timeouts
    {
        public const int MutexTimeoutMs = 10_000; // 10 sec
        public const int RequestReceivedEventHandleTimeoutMs = 5000;
        public const int ResponseReadyEventHandleTimeoutMs = 1000; // 1 sec
        public const int PayloadDeserializationAcknowledgeTimeoutMs = 1000; // 1 sec
    }
}