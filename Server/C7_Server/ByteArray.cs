using System;

namespace C7_Server
{
    public class ByteArray
    {
        const int Default_Size = 1024;

        int initSize = 0;

        public byte[] bytes;

        public int readIdx;
        public int writeIdx;

        private int capacity = 0;

        public int Remain => capacity - writeIdx;

        public int Length => writeIdx - readIdx;

        public ByteArray(int size = Default_Size)
        {
            bytes = new byte[size];
            capacity = size;
            initSize = size;
            readIdx = 0;
            writeIdx = 0;
        }

        public ByteArray(byte[] defaultBytes)
        {
            bytes = defaultBytes;
            capacity = defaultBytes.Length;
            initSize = defaultBytes.Length;
            readIdx = 0;
            writeIdx = defaultBytes.Length;
        }

        // 重新设置大小
        public void ReSize(int size)
        {
            if (size < Length) return; //小于当前长度copy不进去
            if (size < initSize) return;
            int n = 1;
            while (n < size) n *= 2; //每次长度翻倍
            capacity = n;
            byte[] newBytes = new byte[capacity];
            Array.Copy(bytes, readIdx, newBytes, 0, Length);
            bytes = newBytes;
            writeIdx = Length;
            readIdx = 0;
        }

        // 前移动数组,提高可用长度
        public void MoveBytes()
        {
            if (Length < 8)
            {
                if (Length > 0)
                {
                    Array.Copy(bytes, readIdx, bytes, 0, Length);
                }
                writeIdx = Length; //顺序不能变
                readIdx = 0;
            }
        }

        public int Write(byte[] wirteBytes, int offset, int count)
        {
            if (Remain < count)
            {
                // 扩容
                ReSize(Length + count);
            }
            Array.Copy(wirteBytes, offset, bytes, writeIdx, count);
            writeIdx += count;
            return count;
        }


        public int Read(byte[] bs, int offset, int count)
        {
            // 当前长度只有3个，然后他要读8个，所以我只给他3个
            count = Math.Min(count, Length);
            Array.Copy(bytes, readIdx, bs, offset, count);
            readIdx += count;
            MoveBytes();
            return count;
        }

        public short ReadInt16()
        {
            if (Length < 2) return 0;
            // 默认小端
            short ret = (short)((bytes[readIdx+1] << 8) | bytes[readIdx]);
            readIdx += 2;
            MoveBytes();
            return ret;
        }

        public int ReadInt32()
        {
            if (Length < 4) return 0;
            // 默认小端
            int ret = (short)((bytes[readIdx+3] << 24) | (bytes[readIdx+2] << 16) | (bytes[readIdx+1] << 8) | bytes[readIdx]);
            readIdx += 4;
            MoveBytes();
            return ret;
        }

        //调试
        public override string ToString()
        {
            return BitConverter.ToString(bytes, readIdx, Length);
        }

        public void Debug()
        {
            string debugStr = $"readIdx({readIdx}),writeIdx({writeIdx}),bytes({BitConverter.ToString(bytes, 0, bytes.Length)})";
            Console.WriteLine(debugStr);
        }
    }
}
