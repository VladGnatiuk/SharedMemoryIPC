using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace SharedMemoryIPC
{
    public class StructSerializer<T>
        where T : struct
    {
        public int StructSize { get; } = Marshal.SizeOf(typeof(T));

        public void Write(Stream pipeStream, ref T str)
        {
            IntPtr ptr = Marshal.AllocHGlobal(StructSize);
            var buffer = ArrayPool<byte>.Shared.Rent(StructSize);
            try
            {
                Marshal.StructureToPtr(str, ptr, true);
                Marshal.Copy(ptr, buffer, 0, StructSize);
                pipeStream.Write(buffer, 0, StructSize);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public T Read(Stream pipeStream)
        {
            IntPtr ptr = Marshal.AllocHGlobal(StructSize);
            var buffer = ArrayPool<byte>.Shared.Rent(StructSize);
            try
            {
                pipeStream.Read(buffer, 0, StructSize);
                Marshal.Copy(buffer, 0, ptr, StructSize);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}