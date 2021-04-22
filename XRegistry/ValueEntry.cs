using System;
using System.Linq;
using System.Text;

namespace XRegistryFormat
{
    public class ValueEntry
    {
        public ushort Unknown1;
        public ushort KeyOffset; // relative to keytable start, so +Header.Length to this value
        public ushort Unknown2;
        public ushort ValueLength;
        public ValueType ValueType;
        public bool ValueAsBoolean;
        public int ValueAsInteger;
        public byte[] ValueAsString;
        // terminated by '\0' for all types

        public long AbsoluteValueOffset { get; internal set; }

        public override string ToString()
        {
            switch (ValueType)
            {
                case ValueType.Boolean:
                    return ValueAsBoolean ? "[x]" : "[ ]";
                case ValueType.Integer:
                    return $"{ValueAsInteger:x8} ({ValueAsInteger})";
                case ValueType.String:
                {
                    if (ValueAsString == null)
                        return null;

                    var result = Encoding.UTF8.GetString(ValueAsString).TrimEnd('\0');
                    if (result.Any(char.IsControl))
                        return ValueAsString.AsHexString();
                    return $"\"{result}\"";
                }
            }

            throw new InvalidOperationException();
        }
    }

    public static class StringEx
    {
        public static string AsHexString(this byte[] value)
        {
            var result = new StringBuilder(value.Length * 2+2);
            result.Append("0x");
            foreach (var b in value)
                result.Append(b.ToString("x2"));
            return result.ToString();
        }
    }
}