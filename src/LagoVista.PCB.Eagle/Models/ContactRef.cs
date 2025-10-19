// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 07e9db3778e711a999495e105ca788274bf1c3cec75f187fb7eabad83ea1b38d
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class ContactRef
    {
        public string ElementName { get; set; }
        public string Pad { get; set; }

        public static ContactRef Create(XElement element)
        {
            return new ContactRef()
            {
                ElementName = element.GetString("element"),
                Pad = element.GetString("pad"),
            };
        }
    }
}
