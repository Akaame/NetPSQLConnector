using System;
using System.Text;
using System.Buffers.Binary;

namespace NPSQLConnector
{
    public class WriteBuffer
    {
        private const int bufferSize = 4096; // Normally this would be system wide
        private byte[] InnerBuffer {get; set;} // We need underlying buffer for span
        // https://stackoverflow.com/questions/49020894/how-is-the-new-c-sharp-spant-different-from-arraysegmentt
        private int _offset;
        public EndianType Endianness{get;}
        public WriteBuffer(EndianType endianness = EndianType.BigEndian)
        {
            Endianness = endianness;
            InnerBuffer = new byte[bufferSize];
            _offset = 0;
        }

        public void WriteInt32(int val)
        {
            Span<byte> newSlice = new Span<byte>(InnerBuffer);
            if (Endianness.Equals(EndianType.BigEndian))
            {
                BinaryPrimitives.WriteInt32BigEndian(newSlice.Slice(_offset, 4), val);
            }
            else
            {
                BinaryPrimitives.WriteInt32LittleEndian(newSlice.Slice(_offset, 4), val);
            }
            _offset += sizeof(int);
        }

        public void WriteString(string val)
        {
            var srcBuffer = Encoding.UTF8.GetBytes(val);
            Buffer.BlockCopy(srcBuffer, 0, InnerBuffer, _offset, srcBuffer.Length);
            _offset += srcBuffer.Length;
        }

        public void WriteByteArray(byte[] val)
        {
            Buffer.BlockCopy(val, 0, InnerBuffer, _offset, val.Length);
            _offset += val.Length;
        }

        public void WriteByte(byte val)
        {
            InnerBuffer[_offset] = val;
            _offset +=1;
        }

        public Span<byte> FinalizeBuffer() 
        {
            return new Span<byte>(InnerBuffer).Slice(0, _offset);
        }
    }
}