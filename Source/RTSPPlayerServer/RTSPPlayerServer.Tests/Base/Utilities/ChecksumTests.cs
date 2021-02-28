using System.Text;
using NUnit.Framework;
using RTSPPlayerServer.Service.Base.Utilities;

namespace RTSPPlayerServer.Tests.Base.Utilities
{
    /// <summary>
    /// A class that contains tests for <see cref="Checksum"/> class.
    /// </summary>
    [TestFixture]
    public class ChecksumTests
    {
        /// <summary>
        /// Tests the <see cref="Checksum.Crc16"/> method.
        /// </summary>
        /// <param name="asciiString">ASCII string.</param>
        [TestCase("IIOt8", ExpectedResult = 0x77A9)]
        [TestCase("rXmUAPprxa", ExpectedResult = 0xBEEE)]
        [TestCase("S9PNuy2QbAdSVwN", ExpectedResult = 0xA3FA)]
        [TestCase("6zb0CzS55Quf93D2Qy6yz5Zzg", ExpectedResult = 0xB1C8)]
        [TestCase("kY9KbVI7rml9HIAR2gkzeZuyDk7jzgUQ8mPLy81s", ExpectedResult = 0x25EE)]
        [TestCase("indZikywrrlhX5kcyW4lgc1lyENTxgzmhTwtqNZtMq8e3PTK2iAEOgLU4E9a", ExpectedResult = 0x80BC)]
        public int TestCrc16(string asciiString)
        {
            var bytes = Encoding.ASCII.GetBytes(asciiString);
            return Checksum.Crc16(bytes);
        }
    }
}
