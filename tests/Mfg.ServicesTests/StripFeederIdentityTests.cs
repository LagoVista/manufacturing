using LagoVista.Manufacturing.Models;
using System.Linq;

namespace Mfg.ServicesTests
{
    public class StripFeederIdentityTests
    {
        [TestCase("0000", "0000")]
        [TestCase(" 00af ", "00AF")]
        [TestCase("ffff", "FFFF")]
        [TestCase("", null)]
        public void Normalizes(string input, string expected)
        {
            Assert.That(StripFeederIdentity.TryNormalize(input, out var normalized), Is.True);
            Assert.That(normalized, Is.EqualTo(expected));
        }

        [TestCase("abc")]
        [TestCase("0x12")]
        [TestCase("000G")]
        [TestCase("12345")]
        public void RejectsMalformedId(string input)
        {
            Assert.That(StripFeederIdentity.TryNormalize(input, out _), Is.False);
        }

        [Test]
        public void AllocatesNextUnusedIdIgnoringCaseAndLegacyInvalidValues()
        {
            Assert.That(StripFeederIdentity.FindAvailable(new[] { "0001", "0002", " 0003 ", "garbage" }), Is.EqualTo("0004"));
            Assert.That(StripFeederIdentity.Matches("00af", "00AF"), Is.True);
        }

        [Test]
        public void HandlesExhaustion()
        {
            Assert.That(StripFeederIdentity.FindAvailable(Enumerable.Range(0, 65536).Select(i => i.ToString("X4"))), Is.Null);
        }
    }
}
