// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fef864a76d94dd339d33c45ed487d5fa92c9efe7f4ba6c6829e27b53d6eea28d
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace
{
    public static class StringExtensions
    {
        public const string NotSelectedId = "-1";

        public static bool HasValidId(this string id)
        {
            if (String.IsNullOrEmpty(id))
                return false;

            if (id == NotSelectedId)
                return false;

            return true;
        }
    }
}
