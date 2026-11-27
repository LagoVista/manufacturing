using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.Manufacturing.Models
{
    public static class StripFeederIdentity
    {
        public static bool TryNormalize(string value, out string normalized)
        {
            normalized = null;
            if (String.IsNullOrWhiteSpace(value)) return true;
            var candidate = value.Trim();
            if (candidate.Length != 4 || candidate.Any(c => !((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'))))
                return false;
            normalized = candidate.ToUpperInvariant();
            return true;
        }

        public static bool Matches(string value, string normalized)
            => TryNormalize(value, out var candidate) && candidate != null && candidate == normalized;

        public static string FindAvailable(IEnumerable<string> ids)
        {
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids)
                if (TryNormalize(id, out var normalized) && normalized != null)
                    used.Add(normalized);
            for (var i = 1; i <= 0xFFFF; i++)
            {
                var candidate = i.ToString("X4");
                if (!used.Contains(candidate)) return candidate;
            }
            return used.Contains("0000") ? null : "0000";
        }
    }
}
