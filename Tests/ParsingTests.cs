using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using XRegistryFormat;

namespace Tests
{
    [TestFixture]
    public class ParsingTests
    {
        [TestCase(@"C:\Games\Emulators Collection\Emulators\PS3\rpcs3\dev_flash2\etc\xRegistry.sys")]
        public void ParseTest(string path)
        {
            var bytes = File.ReadAllBytes(path);
            Assert.That(bytes, Is.Not.Empty);

            var xregistry = XRegistryParser.Parse(bytes);
            var maxKeyLength = xregistry.Keys.Values.Max(v => v.Key.Length);
            foreach (var value in xregistry.Values)
            {
                xregistry.Keys.TryGetValue(value.KeyOffset, out var key);
                Console.WriteLine($"{value.AbsoluteValueOffset:x8} -> {key?.KeyAbsoluteOffset:x8} {key?.KeyType.ToString()[0] ?? ' '}{value.ValueType.ToString()[0]}: {key?.Key.PadRight(maxKeyLength)} = {value}");
            }
        }
    }
}