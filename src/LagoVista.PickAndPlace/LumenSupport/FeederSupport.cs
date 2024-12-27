using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.LumenSupport
{
    public enum FeederCommands
    {
        GetId = 1,
        Initialize = 2,
        GetVersion = 3,
        MoveFeedForward = 4,
        MoveFeedBackward = 5,
        MoveFeedStatus = 6,
        VendorOptions = 192,
        IdentifyFeeder = 193,
        ProgramFeederFloor = 194,
        UninitialziedFeedersRespond = 195,
    }

    public class CommandStructure
    {
        public bool IsUnicast { get; set; }
        public byte PayloadLength { get; set; }
    }



    public class FeederSupport
    {
        public byte packetID = 0;

        private Dictionary<FeederCommands, CommandStructure> _feederCommands = new Dictionary<FeederCommands, CommandStructure>()
        {
            {FeederCommands.GetId, new CommandStructure() { PayloadLength = 1,} },
            {FeederCommands.Initialize, new CommandStructure() { PayloadLength = 1,} },
            {FeederCommands.GetVersion, new CommandStructure() { PayloadLength = 1,} },
            {FeederCommands.MoveFeedForward, new CommandStructure() { PayloadLength = 1,} },
            {FeederCommands.MoveFeedBackward, new CommandStructure() { PayloadLength = 1,} },
            {FeederCommands.MoveFeedStatus, new CommandStructure() { PayloadLength = 1,} },
            {FeederCommands.VendorOptions, new CommandStructure() { PayloadLength = 1,} },
            {FeederCommands.IdentifyFeeder, new CommandStructure() { PayloadLength = 13, IsUnicast = false} },
            {FeederCommands.ProgramFeederFloor, new CommandStructure() { PayloadLength = 14, IsUnicast = false} },
            {FeederCommands.UninitialziedFeedersRespond, new CommandStructure() { PayloadLength = 1, IsUnicast = false} },
        };


        public byte CalcCRC(List<byte> e)
        {
            ushort t = 0;
            for (int a = 0; a < e.Count(); a++)
            {
                t ^= (ushort)(e[a] << 8); // XOR with the byte shifted 8 bits left
                for (int n = 0; n < 8; n++)
                {
                    if ((t & 32768) != 0) // Check if the most significant bit is 1
                    {
                        t ^= 33664; // XOR with the polynomial value
                    }
                    t <<= 1; // Left shift by 1
                }
            }
            return (byte)((t >> 8) & 255); // Right shift by 8 and mask with 255 to get the final byte
        }

        public int[] HexStringToIntArray(string e)
        {
            List<int> t = new List<int>(); // Use a List<int> to dynamically store the values
            Console.WriteLine("string: " + e); // Log the string (similar to `console.log` in JavaScript)

            for (int a = 0; a < e.Length / 2; a++)
            {
                int n = a * 2;
                string i = e.Substring(n, 2); // Extract two characters from the string
                int l = Convert.ToInt32(i, 16); // Convert the hex string to an integer
                t.Add(l); // Add the integer to the list
            }

            return t.ToArray(); // Convert the List to an array and return it
        }

        public bool ValidatePacketCrc(List<byte> e)
        {
            byte t = e[4]; // Extract the CRC from the fifth byte (index 4)
            byte[] a = e.Take(4).Concat(e.Skip(5)).ToArray(); // Concatenate the first 4 bytes and the rest after the 5th byte
            byte n = CalcCRC(new List<byte>(a)); // Calculate CRC of the array excluding the 5th byte
            if (t != n)
            {
                Console.WriteLine("Received packet had CRC mismatch.");
                return false; // CRC mismatch
            }
            return true; // CRC matched
        }

        public string IntArrayToHexString(List<byte> buffer)
        {
            var bldr = new StringBuilder();
            foreach (var ch in buffer)
            {
                bldr.Append(ch.ToString("X").ToUpper());
            }
            return bldr.ToString();
        }

        public string GetGcodeFromPacketAndPayloadArray(List<byte> buffer)
        {
            var bldr = new StringBuilder();
            foreach (var ch in buffer)
            {
                var chout = ch.ToString("X2");
                Console.WriteLine(chout);
                bldr.Append(ch.ToString("X2").ToUpper());
            }
            bldr.ToString();
            Console.WriteLine($"Final String {bldr.ToString()}");
            return $"M485 {bldr}";
        }

        public string GenerateGCode(FeederCommands command, byte toAddress = 0, byte[] p = null)
        {
            var payload = p == null ? new List<byte>() : new List<byte>( p);
            payload.Insert(0, (byte)command);

            var cmdStructure = _feederCommands[command];
            if(cmdStructure.IsUnicast && toAddress == 0)
            {
                throw new ArgumentException($"Command {command} expects target");
            }

            if (cmdStructure.PayloadLength > 1 && (payload.Count == 0)) 
            {
                throw new ArgumentException($"Command {command} expects a payload");
            }

            if(cmdStructure.PayloadLength == 0 && (payload.Count == 0))
            {
                throw new ArgumentException($"Command {command} expects a payload.");
            }
            
            var buffer = new List<byte>();
            buffer.Add(toAddress);
            buffer.Add(0x00);
            buffer.Add(packetID);
            buffer.Add(cmdStructure.PayloadLength == 0 ? (byte)payload.Count : cmdStructure.PayloadLength);
            Console.WriteLine("Header => " + string.Join(", ", buffer));
            buffer.AddRange(payload);


            var crc = CalcCRC(buffer);
            buffer.Insert(4,crc);

    // Get the final GCode
            var gCode = GetGcodeFromPacketAndPayloadArray(buffer);
            Console.WriteLine(gCode);

            packetID = (packetID < 255) ? (byte)(packetID + 1) : (byte)0;

            return gCode;

            //// Clear serial buffer and send data
            //serial.ClearBuffer();
            //await serial.SendAsync(new List<byte> { (byte)l.Length }); // Send the command (send a byte array in this case)

            //long s = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //bool packetReceived = false;

            //// Wait for the serial response (with timeout)
            //while (true)
            //{
            //    if (await serial.DelayAsync(10)) // Simulate delay of 10 ms
            //    {
            //        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - s > 400)
            //        {
            //            Console.WriteLine("Timeout: didn'address get serial response.");
            //            return false;
            //        }

            //        bool p = false;
            //        Console.WriteLine(serial.ReceiveBuffer);

            //        // Regex pattern for 'ok'
            //        Regex f = new Regex("ok");

            //        foreach (var w in serial.ReceiveBuffer)
            //        {
            //            if (f.IsMatch(w.ToString()))
            //            {
            //                p = true;
            //                break;
            //            }
            //        }

            //        if (p)
            //            break;
            //    }
            //}

            //byte currentPacketID = packetID;
            //packetID = (packetID < 255) ? (byte)(packetID + 1) : (byte)0;

            //string r = "";
            //Regex u = new Regex("rs485-reply:");

            //foreach (var w in serial.ReceiveBuffer)
            //{
            //    if (u.IsMatch(w.ToString()))
            //    {
            //        r = u.Match(w.ToString()).Groups[1].Value;
            //        break;
            //    }
            //}

            //if (r == "TIMEOUT")
            //{
            //    Console.WriteLine("Received TIMEOUT.");
            //    return false;
            //}

            //if (string.IsNullOrEmpty(r))
            //{
            //    modal.Show("Photon Support", "Your version of Marlin does not support Photon. Please update Marlin to the version in the <a href='https://github.com/opulo-inc/lumenpnp/releases'>latest LumenPnP release</a> using the instructions <a href='https://docs.opulo.io/byop/motherboard/update-firmware/'>here</a>.");
            //    return false;
            //}

            //byte[] h = HexStringToIntArray(r);

            //// Validate CRC and check packet ID
            //if (ValidatePacketCrc(h))
            //{
            //    if (h[2] != currentPacketID)
            //    {
            //        Console.WriteLine("Returning packet ID mismatched sent packet ID.");
            //        return false;
            //    }
            //    return true;
            //}

            //return false;
        }
    }
}
