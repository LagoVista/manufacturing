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
