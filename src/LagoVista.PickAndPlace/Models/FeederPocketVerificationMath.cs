namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Pure pocket-index validation for operator-supervised checks.
    /// Never updates CurrentPartIndex or feeder inventory.
    /// </summary>
    public static class FeederPocketVerificationMath
    {
        public static bool TryGetInspectionIndex(
            int startingIndex, int capacity, bool preceding, out int index)
        {
            index = 0;
            if (startingIndex < 1 || capacity < 1 || startingIndex > capacity ||
                (preceding && startingIndex <= 1))
                return false;

            index = preceding ? startingIndex - 1 : startingIndex;
            return true;
        }
    }
}
