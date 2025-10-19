// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ccd514bad38b96efe783f9b41762c9d47fb49bcdc7be3a1e43dde0c255633826
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle
{
    public static class XElementExtensions
    {
        public static double GetDouble(this XElement element, string name)
        {
            return Convert.ToDouble(element.Attribute(name).Value);
        }

        public static double? GetDoubleNullable(this XElement element, string name)
        {
            if (element.Attributes(name).Any())
            {
                return Convert.ToDouble(element.Attribute(name).Value);
            }

            return null;
        }

        public static string GetChildString(this XElement element, string name)
        {
            if(element.Descendants(XName.Get(name)).Any())
            {
                return element.Descendants(XName.Get(name)).First().Value;
            }
            else
            {
                return String.Empty;
            }
        }
       

        public static Int32 GetInt32(this XElement element, string name)
        {
            return Convert.ToInt32(element.Attribute(name).Value);
        }

        public static string GetString(this XElement element, string name, string defaultValue = "")
        {
            if(element.Attributes(name).Any())
            {
                return element.Attribute(name).Value;
            }

            return defaultValue;
        }
    }
}
