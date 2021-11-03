namespace SharedMemoryIPC
{
    public static class NamingHelper
    {
        public static string GetChannelReadySemaphoreName(string prefix) => $"{prefix}/sm-ch";
        public static string GetDataReadySemaphoreName(string prefix) => $"{prefix}/sm-req";

        public static string GetPeerAddress(ushort globalPeerId) => $"peer-{globalPeerId}";
        public static string GetPipeName(string prefix, string fromPeer, string toPeer) => $"{prefix}/pipe-from-{fromPeer}-to-{toPeer}";
    }
}