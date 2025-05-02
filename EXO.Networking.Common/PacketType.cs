namespace EXO.Networking.Common
{
    public enum PacketType : byte
    {
        RequestHostRoom = 0x00,
        RequestJoinRoom = 0x01,

        Custom = 0x02,

        ResponseHostRoom = 0x03,
        ResponseJoinRoom = 0x04,

        ClientLeftRoom = 0x05,
        ClientJoinedRoom = 0x06,
    }
}
