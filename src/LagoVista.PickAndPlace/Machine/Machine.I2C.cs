using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
        public void I2CSend(byte address, byte register)
        {
            var cmd = $"M260 A{address} B{register} S1";
            SendCommand(cmd);
        }

        public void I2CSend(byte address, byte[] buffer)
        {
            throw new NotImplementedException("Has not been tested.");

            var cmd = $"M260 A{address}";
            foreach (byte b in buffer)
                cmd += $" B{b}";

            cmd += " S1";

            SendCommand(cmd);
        }

        ManualResetEventSlim _i2cResetEvent = new ManualResetEventSlim(false);
        private byte _byteReceived;

        public async Task<InvokeResult<byte>> I2CReadHexByte(byte address)
        {
            _i2cResetEvent.Reset();
            LineReceived += I2C_LineReceived;
            var cmd = $"M261 A{address} B1 S1";
            SendCommand (cmd);

            var attempt = 0;
            while (!_i2cResetEvent.IsSet)
            {
                await Task.Delay(5);
                attempt++;
                if (attempt > 200)
                {
                    LineReceived -= I2C_LineReceived;
                    return InvokeResult<byte>.FromError("Timeout requesting I2C");
                }
            }
            LineReceived -= I2C_LineReceived;
            return InvokeResult<byte>.Create(_byteReceived);
        }

        private void I2C_LineReceived(object sender, string e)
        {
            var regEx = new Regex("^echo:i2c-reply: from:(?'address'[0-9]+).+bytes:(?'numbytes'[0-9]+).+data:(?'data'[0-9A-Fa-f]+)$");
            var match = regEx.Match(e);
            if (match.Success)
            {
                _byteReceived = Byte.Parse(match.Groups["data"].Value,System.Globalization.NumberStyles.HexNumber);
                _i2cResetEvent.Set();
            }
        }
    }
}
