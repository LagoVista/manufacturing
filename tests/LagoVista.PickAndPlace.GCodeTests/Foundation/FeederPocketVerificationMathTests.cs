using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederPocketVerificationMathTests
    {
        [TestCase(2, 30, false, 2)]
        [TestCase(2, 30, true, 1)]
        [TestCase(30, 30, false, 30)]
        [TestCase(30, 30, true, 29)]
        public void ChoosesCurrentOrPreviousWithoutChangingState(
            int currentIndex, int capacity, bool preceding, int expected)
        {
            Assert.That(FeederPocketVerificationMath.TryGetInspectionIndex(
                currentIndex, capacity, preceding, out var index), Is.True);
            Assert.That(index, Is.EqualTo(expected));
        }

        [TestCase(1, 30, true)]
        [TestCase(0, 30, false)]
        [TestCase(31, 30, false)]
        [TestCase(2, 0, false)]
        [TestCase(-1, 30, true)]
        public void RejectsAbsentPrecedingOrOutOfRangeIndices(
            int currentIndex, int capacity, bool preceding)
        {
            Assert.That(FeederPocketVerificationMath.TryGetInspectionIndex(
                currentIndex, capacity, preceding, out var index), Is.False);
            Assert.That(index, Is.EqualTo(0));
        }
    }
}
