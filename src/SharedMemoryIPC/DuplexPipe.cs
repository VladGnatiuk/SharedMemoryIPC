using System;
using System.Threading.Tasks;

namespace SharedMemoryIPC
{
    public class DuplexPipe<TMessageHeader> : IAsyncDisposable
        where TMessageHeader : struct, IMessageHeader
    {

        #region Ctor & fields

        private readonly IPipeDescriptor _outgoingPipeDescriptor;
        private readonly PipeSink<TMessageHeader> _sink;
        private PipeSource<TMessageHeader> _source;

        public DuplexPipe(IPipeDescriptor outgoingPipeDescriptor,
            IPipeDescriptor incomingPipeDescriptor,
            OnMessageDelegate<TMessageHeader> onMessageCallback)
        {
            _outgoingPipeDescriptor = outgoingPipeDescriptor;

            _sink = new PipeSink<TMessageHeader>(
                incomingPipeDescriptor,
                onMessageCallback
            );
        }

        #endregion

        public void Connect()
        {
            if (_source != null)
                throw new InvalidOperationException($"Already connected");

            _source = new PipeSource<TMessageHeader>(_outgoingPipeDescriptor);
            _sink.OpenGate();
        }

        public void Disconnect()
        {
            if (_source == null)
                return;

            _sink.CloseGate();
            _source.Dispose();
            _source = null;
        }

        public void Send(ref TMessageHeader messageHeader, WriteToStreamDelegate writeToStream) => _source.Send(ref messageHeader, writeToStream);

        public async ValueTask DisposeAsync()
        {
            Disconnect();
            await _sink.DisposeAsync();
        }
    }
}