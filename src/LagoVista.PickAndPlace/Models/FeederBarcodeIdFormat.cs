using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Display/storage format for the existing StripFeeder.FeederId.
    /// The physical feeder barcode carries a sixteen-bit ID, rendered as
    /// exactly four hex digits. Keep it as text to preserve leading zeros.
    /// </summary>
    public static class FeederBarcodeIdFormat
    {
        public static bool TryNormalize(string value, out string normalized)
        {
            normalized = null;
            if (String.IsNullOrWhiteSpace(value))
                return true;

            var candidate = value.Trim();
            if (candidate.Length != 4)
                return false;

            foreach (var digit in candidate)
            {
                if (!((digit >= '0' && digit <= '9') ||
                      (digit >= 'A' && digit <= 'F') ||
                      (digit >= 'a' && digit <= 'f')))
                    return false;
            }

            normalized = candidate.ToUpperInvariant();
            return true;
        }
    }
}
