using System;

namespace KernelManagementJam.Benchmarks
{
    public static class XorShiftRandom
    {
        public static void FillByteArray(byte[] bytes)
        {
            var seed = (ulong) new Random().Next(int.MaxValue-1);
            FillByteArray(bytes, seed);
        }
        
        public static unsafe void FillByteArray(byte[] bytes, ulong seed)
        {
            if (bytes.Length == 0)
                return;
            
            ulong x_ = seed << 1;
            ulong y_ = seed >> 1;
            int next;
            ulong temp_x, temp_y;

            int* ptr;
            int count = bytes.Length;
            fixed (byte* ptrFrom = &bytes[0])
            {
                ptr = (int*) ptrFrom;
                while (count >= 4)
                {
                    temp_x = y_;
                    x_ ^= x_ << 23;
                    temp_y = x_ ^ y_ ^ (x_ >> 17) ^ (y_ >> 26);
                    next = (int) (temp_y + y_);
                    x_ = temp_x;
                    y_ = temp_y;

                    *ptr++ = next;
                    count -= 4;
                }
            }

            if (count > 0)
            {
                // Never goes here for disk benchmark
                byte* ptrByte = (byte*) ptr;
                
                x_ ^= x_ << 23;
                temp_y = x_ ^ y_ ^ (x_ >> 17) ^ (y_ >> 26);
                next = (int) (temp_y + y_);
                
                while (count-- > 0)
                {
                    *ptrByte++ = (byte) (next & 0xFF);
                    next = next >> 8;
                }
            }
        }
    }
}