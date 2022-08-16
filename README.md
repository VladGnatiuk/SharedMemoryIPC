# Shared Memory IPC

## How to use

Create Pipe Sink:
```
_sink = new PipeSink<BenchmarkMessageHeader>(
    "myFile",
    ProcessRequest,
    ChunkSize,
    NumberOfChunks
);
_sink.OpenGate();
```

Implement message handler:
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

Create Pipe Source:
```
_source = new PipeSource<BenchmarkMessageHeader>(
    "myFile",
    ChunkSize,
    NumberOfChunks
);
```

Send message:
```
var messageHeader = new BenchmarkMessageHeader
{
    PayloadSize = payload.Length
};
_source.Send(ref messageHeader, stream => stream.Write(payload, 0, payload.Length));
```


## Benchmark test results
CPU: Intel i9-12900K 3.20 GHz<br/>
RAM: DDR5 4800<br/>
OS: Windows 11 Pro x64<br/>
Max data transfer speed: 23.1 GB per second (100 KB message size)<br/>
Max messages count: 1.1 million messages per second (1 KB message size)<br/>

### Message size 1KB
```
.\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
.\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
.\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
.\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000

> .\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
==============================================================================================================================================================================================================
NumberOfPipes=1; 290.70 MB/sec; 297,674 msg/sec; TimePerOneMessage=0.003 ms; NumberOfMessages=1,024,000; MessageSize=1,024; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:03.4400037
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
==============================================================================================================================================================================================================
NumberOfPipes=2; 656.59 MB/sec; 672,343 msg/sec; TimePerOneMessage=0.002 ms; NumberOfMessages=1,024,000; MessageSize=1,024; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:01.5230314
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
==============================================================================================================================================================================================================
NumberOfPipes=4; 924.87 MB/sec; 947,070 msg/sec; TimePerOneMessage=0.001 ms; NumberOfMessages=1,024,000; MessageSize=1,024; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:01.0812296
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
==============================================================================================================================================================================================================
NumberOfPipes=8; 1,166.81 MB/sec; 1,194,814 msg/sec; TimePerOneMessage=0.001 ms; NumberOfMessages=1,024,000; MessageSize=1,024; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.8570371
==============================================================================================================================================================================================================
```

### Message size 10KB
```
.\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
.\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
.\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
.\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400

> .\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
==============================================================================================================================================================================================================
NumberOfPipes=1; 2,017.96 MB/sec; 206,639 msg/sec; TimePerOneMessage=0.005 ms; NumberOfMessages=102,400; MessageSize=10,240; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.4955493
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
==============================================================================================================================================================================================================
NumberOfPipes=2; 3,218.16 MB/sec; 329,540 msg/sec; TimePerOneMessage=0.003 ms; NumberOfMessages=102,400; MessageSize=10,240; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.3107366
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
==============================================================================================================================================================================================================
NumberOfPipes=4; 3,991.21 MB/sec; 408,700 msg/sec; TimePerOneMessage=0.002 ms; NumberOfMessages=102,400; MessageSize=10,240; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.2505503
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
==============================================================================================================================================================================================================
NumberOfPipes=8; 4,535.67 MB/sec; 464,453 msg/sec; TimePerOneMessage=0.002 ms; NumberOfMessages=102,400; MessageSize=10,240; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.2204745
==============================================================================================================================================================================================================
```

### Message size 100KB
```
.\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240

> .\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=1; 7,821.32 MB/sec; 80,090 msg/sec; TimePerOneMessage=0.013 ms; NumberOfMessages=10,240; MessageSize=102,400; ChunkSize=102,400; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.1278557
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=2; 14,378.83 MB/sec; 147,239 msg/sec; TimePerOneMessage=0.007 ms; NumberOfMessages=10,240; MessageSize=102,400; ChunkSize=102,400; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.0695467
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=4; 19,096.51 MB/sec; 195,548 msg/sec; TimePerOneMessage=0.005 ms; NumberOfMessages=10,240; MessageSize=102,400; ChunkSize=102,400; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.0523656
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=8; 23,185.02 MB/sec; 237,415 msg/sec; TimePerOneMessage=0.004 ms; NumberOfMessages=10,240; MessageSize=102,400; ChunkSize=102,400; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.0431313
==============================================================================================================================================================================================================
```

### Message size 1MB
```
.\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240

> .\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=1; 9,957.23 MB/sec; 9,957 msg/sec; TimePerOneMessage=0.100 ms; NumberOfMessages=10,240; MessageSize=1,048,576; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=10,240 MB; Elapsed=00:00:01.0283981
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=2; 15,369.76 MB/sec; 15,370 msg/sec; TimePerOneMessage=0.065 ms; NumberOfMessages=10,240; MessageSize=1,048,576; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=10,240 MB; Elapsed=00:00:00.6662435
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=4; 15,837.95 MB/sec; 15,838 msg/sec; TimePerOneMessage=0.063 ms; NumberOfMessages=10,240; MessageSize=1,048,576; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=10,240 MB; Elapsed=00:00:00.6465484
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=8; 15,445.42 MB/sec; 15,445 msg/sec; TimePerOneMessage=0.065 ms; NumberOfMessages=10,240; MessageSize=1,048,576; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=10,240 MB; Elapsed=00:00:00.6629799
==============================================================================================================================================================================================================
```

### Message size 10MB
```
.\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
.\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240

> .\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=1; 13,161.46 MB/sec; 1,316 msg/sec; TimePerOneMessage=0.760 ms; NumberOfMessages=10,240; MessageSize=10,485,760; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=102,400 MB; Elapsed=00:00:07.7802928
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=2; 15,869.71 MB/sec; 1,587 msg/sec; TimePerOneMessage=0.630 ms; NumberOfMessages=10,240; MessageSize=10,485,760; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=102,400 MB; Elapsed=00:00:06.4525451
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=4; 14,771.24 MB/sec; 1,477 msg/sec; TimePerOneMessage=0.677 ms; NumberOfMessages=10,240; MessageSize=10,485,760; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=102,400 MB; Elapsed=00:00:06.9323889
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=8; 13,348.95 MB/sec; 1,335 msg/sec; TimePerOneMessage=0.749 ms; NumberOfMessages=10,240; MessageSize=10,485,760; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=102,400 MB; Elapsed=00:00:07.6710139
==============================================================================================================================================================================================================
```

