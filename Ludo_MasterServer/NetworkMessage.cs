using System;
using System.Collections.Generic;
using System.Text;
using static Ludo_MasterServer.Enums;

namespace Ludo_MasterServer
{
    public class NetworkMessage
    {


        public List<byte> m_payload { get; protected set; }
        public byte[] m_raw { get; private set; }
        private int m_position = 0;
        private const int m_headerSize = 1;
        public MessageType m_type { get; private set; }
        public Client m_owner { get; private set; }

        public NetworkMessage()
        {
            m_payload = new List<byte>();
        }

        public NetworkMessage(Client owner, byte[] raw) //Crear mensajes desde Owner, con los datos almacenados en bytes
        {
            this.m_raw = raw;
            this.m_owner = owner;
            m_type = (MessageType)ReadByte();
        }

        public void CopyTo(byte[] bytes, List<byte> lbytes) //Add the data to the stream
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                lbytes.Add(bytes[i]);
            }
        }

        public void WriteTo(List<byte> lbytes, byte[] bytes, int position)
        {
            for (int i = 0; i < lbytes.Count; i++)
            {
                bytes[position + i] = lbytes[i];
            }
        }

        public void Build(MessageType type)
        {
            m_raw = new byte[m_payload.Count + m_headerSize];
            WriteTo(m_payload, m_raw, m_headerSize);
            m_raw[0] = (byte)type;
            this.m_type = type;
        }

        public byte ReadByte()
        {
            byte result = m_raw[m_position];
            m_position++;
            return result;
        }

        public ushort ReadUshort()
        {
            ushort l_result = BitConverter.ToUInt16(m_raw, m_position);
            m_position = m_position + 2;
            return l_result;
        }

        public uint ReadUInt()
        {
            uint l_result = BitConverter.ToUInt32(m_raw, m_position);
            m_position = m_position + 4;
            return l_result;
        }

        public string ReadString()
        {
            int l_size = ReadInt();
            byte[] l_chain = new byte[l_size];
            Array.Copy(m_raw, m_position, l_chain, 0, l_size);
            m_position = m_position + l_size;
            return Encoding.UTF8.GetString(l_chain);
        }

        public int ReadInt()
        {
            int l_result = BitConverter.ToInt32(m_raw, m_position);
            m_position = m_position + 4;
            return l_result;
        }

        public byte[] ReadByteArray()
        {
            int l_size = ReadInt();
            byte[] l_result = new byte[l_size];
            Array.Copy(m_raw, m_position, l_result, 0, l_size);
            m_position = m_position + l_size;
            return l_result;
        }

        public void Write(byte value)
        {
            m_payload.Add(value);
        }

        public void Write(bool value)
        {
            m_payload.Add(value ? (byte)0x01 : (byte)0x00);
        }

        public void Write(ushort value)
        {
            byte[] l_byteValue = BitConverter.GetBytes(value);
            CopyTo(l_byteValue, m_payload);
        }

        public void Write(int value)
        {
            byte[] l_byteValue = BitConverter.GetBytes(value);
            CopyTo(l_byteValue, m_payload);
        }

        public void Write(float value)
        {
            byte[] l_byteValue = BitConverter.GetBytes(value);
            CopyTo(l_byteValue, m_payload);
        }

        public void Write(long value)
        {
            byte[] l_byteValue = BitConverter.GetBytes(value);
            CopyTo(l_byteValue, m_payload);
        }

        public void Write(double value)
        {
            byte[] l_byteValue = BitConverter.GetBytes(value);
            CopyTo(l_byteValue, m_payload);
        }

        public void Write(string text)
        {
            byte[] l_byteText = Encoding.UTF8.GetBytes(text);
            int l_size = l_byteText.Length;
            Write(l_size);
            CopyTo(l_byteText, m_payload);
        }
        public void Write(byte[] value)
        {
            int l_size = value.Length;
            Write(l_size);
            CopyTo(value, m_payload);
        }
    }
}
