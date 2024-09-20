using MemoryPack;

namespace Shared.Network.Packets
{
    [MemoryPackable]
    public partial struct MSG_Hello_SC
    {
        public int ReceivedMessageLength { get; set; }
    }
}
