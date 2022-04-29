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
CPU: Intel i7-4930K CPU 3.40 GHz
RAM: DDR3 1866
OS: Windows 10 Pro x64

### Message size 1KB
```
.\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
.\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
.\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
.\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000

> .\SharedMemoryIPC.Benchmark.exe pipesCount=1 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
==============================================================================================================================================================================================================
NumberOfPipes=1; 254.49 MB/sec; 260,600 msg/sec; TimePerOneMessage=0.004 ms; NumberOfMessages=1,024,000; MessageSize=1,024; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:03.9293863
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
==============================================================================================================================================================================================================
NumberOfPipes=2; 473.01 MB/sec; 484,364 msg/sec; TimePerOneMessage=0.002 ms; NumberOfMessages=1,024,000; MessageSize=1,024; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:02.1141123
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
==============================================================================================================================================================================================================
NumberOfPipes=4; 674.48 MB/sec; 690,670 msg/sec; TimePerOneMessage=0.001 ms; NumberOfMessages=1,024,000; MessageSize=1,024; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:01.4826194
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1024 numberOfChunks=1024 messageSize=1024 numberOfMessagesToSend=1024000
==============================================================================================================================================================================================================
NumberOfPipes=8; 631.54 MB/sec; 646,699 msg/sec; TimePerOneMessage=0.002 ms; NumberOfMessages=1,024,000; MessageSize=1,024; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:01.5834254
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
NumberOfPipes=1; 810.26 MB/sec; 82,970 msg/sec; TimePerOneMessage=0.012 ms; NumberOfMessages=102,400; MessageSize=10,240; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:01.2341760
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
==============================================================================================================================================================================================================
NumberOfPipes=2; 1,343.33 MB/sec; 137,557 msg/sec; TimePerOneMessage=0.007 ms; NumberOfMessages=102,400; MessageSize=10,240; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.7444192
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
==============================================================================================================================================================================================================
NumberOfPipes=4; 1,811.60 MB/sec; 185,508 msg/sec; TimePerOneMessage=0.005 ms; NumberOfMessages=102,400; MessageSize=10,240; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.5519984
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1024 numberOfChunks=1024 messageSize=10240 numberOfMessagesToSend=102400
==============================================================================================================================================================================================================
NumberOfPipes=8; 1,920.94 MB/sec; 196,704 msg/sec; TimePerOneMessage=0.005 ms; NumberOfMessages=102,400; MessageSize=10,240; ChunkSize=1,024; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.5205793
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
NumberOfPipes=1; 4,338.76 MB/sec; 44,429 msg/sec; TimePerOneMessage=0.022 ms; NumberOfMessages=10,240; MessageSize=102,400; ChunkSize=102,400; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.2304807
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=2; 6,268.32 MB/sec; 64,188 msg/sec; TimePerOneMessage=0.016 ms; NumberOfMessages=10,240; MessageSize=102,400; ChunkSize=102,400; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.1595324
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=4; 9,314.29 MB/sec; 95,378 msg/sec; TimePerOneMessage=0.011 ms; NumberOfMessages=10,240; MessageSize=102,400; ChunkSize=102,400; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.1073619
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=102400 numberOfChunks=1024 messageSize=102400 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=8; 12,361.63 MB/sec; 126,583 msg/sec; TimePerOneMessage=0.008 ms; NumberOfMessages=10,240; MessageSize=102,400; ChunkSize=102,400; NumberOfChunks=1,024; AllData=1,000 MB; Elapsed=00:00:00.0808955
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
NumberOfPipes=1; 5,789.42 MB/sec; 5,789 msg/sec; TimePerOneMessage=0.173 ms; NumberOfMessages=10,240; MessageSize=1,048,576; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=10,240 MB; Elapsed=00:00:01.7687444
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=2; 10,198.28 MB/sec; 10,198 msg/sec; TimePerOneMessage=0.098 ms; NumberOfMessages=10,240; MessageSize=1,048,576; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=10,240 MB; Elapsed=00:00:01.0040911
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=4; 8,224.31 MB/sec; 8,224 msg/sec; TimePerOneMessage=0.122 ms; NumberOfMessages=10,240; MessageSize=1,048,576; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=10,240 MB; Elapsed=00:00:01.2450899
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1048576 numberOfChunks=1024 messageSize=1048576 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=8; 8,116.95 MB/sec; 8,117 msg/sec; TimePerOneMessage=0.123 ms; NumberOfMessages=10,240; MessageSize=1,048,576; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=10,240 MB; Elapsed=00:00:01.2615579
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
NumberOfPipes=1; 3,564.59 MB/sec; 356 msg/sec; TimePerOneMessage=2.805 ms; NumberOfMessages=10,240; MessageSize=10,485,760; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=102,400 MB; Elapsed=00:00:28.7269740
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=2 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=2; 4,947.05 MB/sec; 495 msg/sec; TimePerOneMessage=2.021 ms; NumberOfMessages=10,240; MessageSize=10,485,760; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=102,400 MB; Elapsed=00:00:20.6992032
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=4 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=4; 5,557.10 MB/sec; 556 msg/sec; TimePerOneMessage=1.800 ms; NumberOfMessages=10,240; MessageSize=10,485,760; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=102,400 MB; Elapsed=00:00:18.4268628
==============================================================================================================================================================================================================

> .\SharedMemoryIPC.Benchmark.exe pipesCount=8 chunkSize=1048576 numberOfChunks=1024 messageSize=10485760 numberOfMessagesToSend=10240
==============================================================================================================================================================================================================
NumberOfPipes=8; 5,394.98 MB/sec; 539 msg/sec; TimePerOneMessage=1.854 ms; NumberOfMessages=10,240; MessageSize=10,485,760; ChunkSize=1,048,576; NumberOfChunks=1,024; AllData=102,400 MB; Elapsed=00:00:18.9806075
==============================================================================================================================================================================================================
```

