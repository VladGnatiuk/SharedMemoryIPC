# Shared Memory IPC

Define Pipe Sink:

```
_sink = new PipeSink<BenchmarkMessageHeader>(
    "myFile",
    ProcessRequest,
    ChunkSize,
    NumberOfChunks
);
_sink.OpenGate();
```
```
private void ProcessRequest(BenchmarkMessageHeader messageHeader, Stream stream, long offset, MemoryMappedFile memoryMappedFile)
{
    var buffer = ArrayPool<byte>.Shared.Rent(messageHeader.PayloadSize);
    try
    {
        stream.Read(buffer, 0, messageHeader.PayloadSize);
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }            
}
```

Define Pipe Source:
```
_source = new PipeSource<BenchmarkMessageHeader>(
    "myFile",
    ChunkSize,
    NumberOfChunks
);
```
```
var messageHeader = new BenchmarkMessageHeader
{
    PayloadSize = PayloadSize
};
_source.Send(ref messageHeader, stream => stream.Write(payload, 0, payload.Length));
```

Test results:
```
Test 01: 37.34 MB/sec, 391,542 msg/sec, TimePerOneMessage=0.003 ms, NumberOfMessages=1,000,000, MessageSize=100, ChunkSize=1,024, NumberOfChunks=10,240, Total MB=95, Elapsed=00:00:02.5540036
Test 02: 412.06 MB/sec, 421,954 msg/sec, TimePerOneMessage=0.002 ms, NumberOfMessages=1,000,000, MessageSize=1,024, ChunkSize=1,024, NumberOfChunks=10,240, Total MB=977, Elapsed=00:00:02.3699275
Test 03: 1,041.53 MB/sec, 106,652 msg/sec, TimePerOneMessage=0.009 ms, NumberOfMessages=100,000, MessageSize=10,240, ChunkSize=1,024, NumberOfChunks=10,240, Total MB=977, Elapsed=00:00:00.9376264
Test 04: 5,523.70 MB/sec, 5,524 msg/sec, TimePerOneMessage=0.181 ms, NumberOfMessages=10,000, MessageSize=1,048,576, ChunkSize=1,048,576, NumberOfChunks=1,024, Total MB=10,000, Elapsed=00:00:01.8103822
Test 05: 3,377.31 MB/sec, 338 msg/sec, TimePerOneMessage=2.961 ms, NumberOfMessages=1,000, MessageSize=10,485,760, ChunkSize=10,485,760, NumberOfChunks=100, Total MB=10,000, Elapsed=00:00:02.9609359
```

