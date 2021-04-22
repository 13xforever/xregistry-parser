namespace XRegistryFormat
{
    public class KeyEntry
    {
        public ushort Unknown; // 25 a0, 2e a8, 56 41
        public ushort KeyLength;
        public KeyType KeyType;
        public string Key; // KeyLength + '\0'

        public ushort KeyRelativeOffset { get; internal set; } // relative offset, first key = 0x00
        public long KeyAbsoluteOffset { get; internal set; } // relative offset, first key = 0x00

        public static readonly byte[] Terminator = {0xAA, 0xBB, 0xCC, 0xDD, 0xEE};
    }
}