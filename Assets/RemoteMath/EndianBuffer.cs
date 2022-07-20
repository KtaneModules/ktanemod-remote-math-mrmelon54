using System;
using System.Collections.Generic;

namespace RemoteMath
{
    public class EndianBuffer
    {
        int _position;
        readonly bool _isBigEndian;
        readonly List<byte> _buffer = new List<byte>();

        public EndianBuffer(byte[] bytes, bool bigEndian)
        {
            _isBigEndian = bigEndian;
            _buffer.AddRange(bytes);
        }

        public byte[] ToArray()
        {
            return _buffer.ToArray();
        }

        public int Length()
        {
            return _buffer.Count;
        }

        // Read values
        public short ReadInt16()
        {
            return BitConverter.ToInt16(GetBytes(2), 0);
        }

        public int ReadInt32()
        {
            return BitConverter.ToInt32(GetBytes(4), 0);
        }

        public long ReadInt64()
        {
            return BitConverter.ToInt64(GetBytes(8), 0);
        }

        public string ReadString()
        {
            string str = "";
            int size = ReadInt32();
            byte[] estring = new byte[size];

            for (int x = _position, i = 0; x < _position + size; x++, i++) estring[i] = _buffer[x];

            foreach (byte x in estring) str += (char) x;

            _position += size;
            return str;
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(GetBytes(4), 0);
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(GetBytes(8), 0);
        }

        public bool ReadBool()
        {
            return GetBytes(1)[0] != 0;
        }

        public byte ReadByte()
        {
            return GetBytes(1)[0];
        }

        public char ReadChar()
        {
            return (char) ReadByte();
        }

        // Write values
        public void WriteInt16(short v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian && _isBigEndian) Array.Reverse(bytes);
            _buffer.AddRange(bytes);
        }

        public void WriteInt32(int v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian && _isBigEndian) Array.Reverse(bytes);
            _buffer.AddRange(bytes);
        }

        public void WriteInt64(long v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian && _isBigEndian) Array.Reverse(bytes);
            _buffer.AddRange(bytes);
        }

        public void WriteFloat(float v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian && _isBigEndian) Array.Reverse(bytes);
            _buffer.AddRange(bytes);
        }

        public void WriteDouble(double v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian && _isBigEndian) Array.Reverse(bytes);
            _buffer.AddRange(bytes);
        }

        public void WriteString(string v)
        {
            WriteInt32(v.Length);
            byte[] bytes = new byte[v.Length];
            for (int x = 0; x < v.Length; x++) bytes[x] = (byte) v[x];
            _buffer.AddRange(bytes);
        }

        public void WriteByte(byte v)
        {
            _buffer.Add(v);
        }

        public void WriteBytes(byte[] bytes)
        {
            _buffer.AddRange(bytes);
        }

        public void WriteChar(char v)
        {
            WriteByte((byte) v);
        }


        byte[] GetBytes(int pos)
        {
            byte[] bytes = new byte[pos];
            for (int x = _position, i = 0; x < _position + pos; x++, i++) bytes[i] = _buffer[x];
            if (BitConverter.IsLittleEndian && _isBigEndian) Array.Reverse(bytes);
            _position += pos;
            return bytes;
        }
    }
}