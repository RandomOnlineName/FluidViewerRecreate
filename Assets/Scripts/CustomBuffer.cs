using System;

namespace Assets
{
    class CustomBuffer
    {
        byte[] bytes;
        int readHead = 0;
        public CustomBuffer(byte[] bytes) {
            this.bytes = bytes;
        }

        public float readFloat() {
            float returnVal =  BitConverter.ToSingle(bytes, readHead);
            readHead += 4;
            return returnVal;
        }

        public bool readBool() {
            bool returnVal = bytes[readHead] != 0;
            readHead += 1;
            return returnVal;
        }

        public int readInt() {
            int returnVal = BitConverter.ToInt32(bytes, readHead);
            readHead += 4;
            return returnVal;
        }
    }
}
