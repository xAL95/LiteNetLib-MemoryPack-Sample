using MemoryPack;

namespace Shared.Network.Packets
{
    [MemoryPackable]
    public partial struct MSG_Hello_CS
    {
        public string Message { get; set; }
    }
}
