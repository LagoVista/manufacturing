using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederBarcodeIdFormatTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void EmptyValueIsOptional(string value)
        {
            Assert.That(FeederBarcodeIdFormat.TryNormalize(value, out var normalized), Is.True);
            Assert.That(normalized, Is.Null);
        }

        [TestCase("0000", "0000")]
        [TestCase("00aF", "00AF")]
        [TestCase("dead", "DEAD")]
        [TestCase("FFFF", "FFFF")]
        [TestCase(" a1b2 ", "A1B2")]
        public void FourHexDigitsPreserveLeadingZerosAndNormalizeCase(string input, string expected)
        {
            Assert.That(FeederBarcodeIdFormat.TryNormalize(input, out var normalized), Is.True);
            Assert.That(normalized, Is.EqualTo(expected));
        }

        [TestCase("123")]
        [TestCase("12345")]
        [TestCase("0x1A")]
        [TestCase("GGGG")]
        [TestCase("00+1")]
        public void InvalidFeederCodesAreRejectedWithoutNormalization(string value)
        {
            // A space is allowed only around an otherwise complete four-digit ID.
            Assert.That(FeederBarcodeIdFormat.TryNormalize(value, out var normalized),
                Is.False);
            Assert.That(normalized, Is.Null);
        }
    }
}
