
using EXO.Networking.Common;

public static class PacketExtensions
{
    public static ExoClient ReadClient(this Packet packet)
    {
        return new ExoClient(packet.ReadLong(), packet.ReadString());
    }
}
