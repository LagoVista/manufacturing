// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f34185f54a0030f257ab79ea7697c5d1b3a5fbb501d04c801debd76ed7885585
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
        public void SendCommand(String cmd)
        {
            if (AssertConnected())
            {
                Enqueue(cmd);
            }
        }
    }
}
