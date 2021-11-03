using System;
using System.Threading.Tasks;

namespace SharedMemoryIPC
{
    public static class AsyncDisposableEx
    {
        public static IAsyncDisposable Create(Func<ValueTask> callback) => new DelegateAsyncDisposable(callback);
    }

    public class DelegateAsyncDisposable : IAsyncDisposable
    {
        private readonly Func<ValueTask> _callback;

        public DelegateAsyncDisposable(Func<ValueTask> callback)
        {
            _callback = callback;
        }

        public ValueTask DisposeAsync() => _callback();
    }
}