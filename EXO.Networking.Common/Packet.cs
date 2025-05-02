using System;
using System.IO;
using System.Text;

namespace EXO.Networking.Common
{
    public class Packet : IDisposable
    {

        private const int PACKET_HEADER_LENGTH = 1;

        private MemoryStream mStream;
        private BinaryWriter mWriter;
        private BinaryReader mReader;

        public byte Header { get; }

        public Packet(byte header)
        { 
            mStream = new MemoryStream();

            mWriter = new BinaryWriter(mStream);
            mReader = new BinaryReader(mStream);

            mWriter.Write(header);

            Header = header;
        }

        public Packet(byte[] _rawData)
        {
            mStream = new MemoryStream(_rawData);

            mWriter = new BinaryWriter(mStream);
            mReader = new BinaryReader(mStream);

            Header = mReader.ReadByte();
        }

        #region Write

        public void Write(float _toWrite)
        {
            mWriter.Write(_toWrite);
        }

        public void Write(int _toWrite)
        {
            mWriter.Write(_toWrite);
        }

        public void Write(long _toWrite)
        {
            mWriter.Write(_toWrite);
        }

        public void Write(string _toWrite)
        {
            var bytes = Encoding.UTF8.GetBytes(_toWrite);
            mWriter.Write(_toWrite.Length);
            mWriter.Write(bytes);
        }

        public void Write(Packet _toWrite)
        {
            mWriter.Write(_toWrite.Length);
            mWriter.Write(_toWrite.RawData);
        }

        public void Write(byte[] _toWrite)
        {
            mStream.Write(_toWrite, 0, _toWrite.Length);
        }
        #endregion

        #region Read

        public float ReadFloat()
        { 
            return mReader.ReadSingle();
        }

        public string ReadString()
        {
            var length = mReader.ReadInt32();
            return Encoding.UTF8.GetString(mReader.ReadBytes(length));
        }

        public int ReadInt()
        { 
            return mReader.ReadInt32();
        }

        public long ReadLong()
        {
            return mReader.ReadInt64();
        }

        public Packet ReadPacket()
        {
            var legnth = mReader.ReadInt64();
            var bytes = mReader.ReadBytes((int)legnth);
            return new Packet(bytes);
        }

        #endregion

        public int length;

        public virtual byte[] RawData => mStream.ToArray();

        public long Length => mStream.Length;

        public void Reset()
        { 
            mStream.Position = PACKET_HEADER_LENGTH;
        }

        public byte[] ReadBytes(int start, int count)
        {
            byte[] buffer = new byte[count];
            mStream.Read(buffer, PACKET_HEADER_LENGTH, count);
            return buffer;
        }

        public byte[] ReadRest()
        {
            int count = (int)(mStream.Length - mStream.Position);
            byte[] buffer = new byte[count];
            mStream.Read(buffer, 0, count);
            return buffer;
        }

        public void Dispose()
        {
            mStream.Dispose();
            mReader.Dispose();
            mWriter.Dispose();
        }

        public static Packet CreateCustomPacket(int customType, long? to = null)
        {

            // Custom Packet:
            // CLIENT: [Header][CustomType][Payload]
            // HOST: [HEADER][TO][Payload]
            

            Packet packet = new Packet((byte)PacketType.Custom);
            packet.Write(customType);

            if (to.HasValue)
            {
                packet.Write(to.Value);
            }

            return packet;
        }
    }
}
