using System;

namespace SharedMemoryIPC.Benchmark
{
    public struct BenchmarkMessageHeader : IMessageHeader
    {
        public bool Skip { get; set; }
        public int PayloadSize { get; set; }

        public int PipeId { get; set; }
        public int MessageId { get; set; }
        public long Ticks { get; set; }

        public static (byte first, byte last) GenerateCheckValues(ref BenchmarkMessageHeader h) =>
        (
            (byte)((h.PipeId + 1) ^ (h.MessageId + 1)),
            (byte)(h.PayloadSize ^ (h.PipeId + 1) ^ (h.MessageId + 1))
        );

        public static void AssertCheckValues(ref BenchmarkMessageHeader h, byte first, byte last)
        {
            if (GenerateCheckValues(ref h) != (first, last))
                throw new Exception($"Check values don't match for PipeId={h.PipeId}, MessageId={h.MessageId}");
        }
    }
}