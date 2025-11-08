// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b0fc2c1ba3560935ae2f5a309e68848c0e2d9a67d03274467aa1e97f694a2f44
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public class StatusMessage
    {
        public StatusMessageTypes MessageType { get; private set; }
        public DateTime DateStamp { get; private set; }
        public String Message { get; private set; }        

        public static StatusMessage Create(StatusMessageTypes type, String message)
        {
            return new StatusMessage()
            {
                MessageType = type,
                Message = message,
                DateStamp = DateTime.Now
            };
        }
    }
}
