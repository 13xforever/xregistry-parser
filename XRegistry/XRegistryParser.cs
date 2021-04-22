using System;
using System.IO;
using System.Linq;
using System.Text;
using BitConverter;

namespace XRegistryFormat
{
    public static class XRegistryParser
    {
        public static XRegistry Parse(byte[] data)
        {
            using (var stream = new MemoryStream(data))
                return Parse(stream);
        }

        public static XRegistry Parse(Stream stream)
        {
            var result = new XRegistry();
            var baseOffset = stream.Position;
            using (var reader = new BinaryReader(stream))
            {
                var header = reader.ReadBytes(XRegistry.Header.Length);
                if (!XRegistry.Header.SequenceEqual(header))
                    throw new FormatException("Invalid header");

                var keyTableOffset = reader.BaseStream.Position;
                do
                {
                    var key = new KeyEntry();
                    key.KeyAbsoluteOffset = reader.BaseStream.Position;
                    key.KeyRelativeOffset = (ushort)(key.KeyAbsoluteOffset - keyTableOffset);
                    var buf = reader.ReadBytes(5);
                    if (KeyEntry.Terminator.SequenceEqual(buf))
                        break;

                    key.Unknown = EndianBitConverter.BigEndian.ToUInt16(buf, 0);
                    key.KeyLength = EndianBitConverter.BigEndian.ToUInt16(buf, 2);
                    key.KeyType = (KeyType)buf[4];
                    key.Key = Encoding.UTF8.GetString(reader.ReadBytes(key.KeyLength));
                    if (reader.ReadByte() != 0)
                        throw new FormatException($"Key entry terminator is not 0 (key offset: 0x{key.KeyAbsoluteOffset:x8}, terminator offset: 0x{reader.BaseStream.Position - 1:x8})");

                    result.Keys.Add(key.KeyRelativeOffset, key);
                } while (reader.BaseStream.Position - baseOffset < 0xFFF0);

                reader.BaseStream.Seek(baseOffset + 0xFFF0, SeekOrigin.Begin);
                var footer = reader.ReadBytes(XRegistry.KeysFooter.Length);
                if (!XRegistry.KeysFooter.SequenceEqual(footer))
                    throw new FormatException("Invalid key table footer");

                do
                {
                    var value = new ValueEntry();
                    value.AbsoluteValueOffset = reader.BaseStream.Position;
                    var buf = reader.ReadBytes(6);
                    if (KeyEntry.Terminator.SequenceEqual(buf.Take(5)))
                        break;

                    value.Unknown1 = EndianBitConverter.BigEndian.ToUInt16(buf, 0);
                    value.KeyOffset = EndianBitConverter.BigEndian.ToUInt16(buf, 2);
                    value.Unknown2 = EndianBitConverter.BigEndian.ToUInt16(buf, 4);
                    value.ValueLength = EndianBitConverter.BigEndian.ToUInt16(reader.ReadBytes(2), 0);
                    value.ValueType = (ValueType)reader.ReadByte();
                    var valueBytes = reader.ReadBytes(value.ValueLength);
                    switch (value.ValueType)
                    {
                        case ValueType.Boolean:
                            value.ValueAsBoolean = valueBytes[0] == 1;
                            break;
                        case ValueType.Integer:
                            if (value.ValueLength != 4)
                                throw new FormatException($"Integer of length {value.ValueLength}");

                            value.ValueAsInteger = EndianBitConverter.BigEndian.ToInt32(valueBytes, 0);
                            break;
                        case ValueType.String:
                            value.ValueAsString = valueBytes;
                            break;
                        default:
                            throw new FormatException($"Unknown value type {(byte)value.ValueType}");
                    }
                    var valueTerminator = reader.ReadByte();
                    if (valueTerminator != 0)
                        //throw new FormatException($"Value terminator is not 0 (value offset: 0x{value.AbsoluteValueOffset:x8}, terminator offset: 0x{reader.BaseStream.Position-1:x8})");
                        Console.WriteLine($"Value terminator was {valueTerminator} (value offset: 0x{value.AbsoluteValueOffset:x8}, terminator offset: 0x{reader.BaseStream.Position-1:x8})");

                    result.Values.Add(value);
                } while (reader.BaseStream.Position < 0x20000);
                return result;
            }
        }
    }
}