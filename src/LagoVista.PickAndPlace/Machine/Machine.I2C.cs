// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b09f6b0dba4a03ab566b823e73a22e9683f6648e5aa2cb0a48efc050ff5c3ca2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.PickAndPlace.Models;
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

        public void I2CSend(byte address, byte register, byte value)
        {
            var cmd = $"M260 A{address} B{register} B{value} S1";
            SendCommand($"M260 A{address}");
            SendCommand($"M260 B{register}");
            SendCommand($"M260 B{value}");
            SendCommand($"M260 S1");
        }

        public void I2CSend(byte address, byte[] buffer)
        {
            throw new NotImplementedException("Has not been tested.");

            /*var cmd = $"M260 A{address}";
            foreach (byte b in buffer)
                cmd += $" B{b}";

            cmd += " S1";

            SendCommand(cmd);*/
        }

        private readonly SemaphoreSlim _i2cReadGate = new SemaphoreSlim(1, 1);
        private readonly ManualResetEventSlim _i2cResetEvent = new ManualResetEventSlim(false);
        private byte _byteReceived;
        private byte _expectedI2cAddress;

        public async Task<InvokeResult<byte>> I2CReadHexByte(byte address)
        {
            await _i2cReadGate.WaitAsync();
            try
            {
                _expectedI2cAddress = address;
                _i2cResetEvent.Reset();
                LineReceived += I2C_LineReceived;
                try
                {
                    SendCommand($"M261 A{address} B1 S1");
                    for (var attempt = 0; attempt < 200 && !_i2cResetEvent.IsSet; attempt++)
                        await Task.Delay(5);
                    return _i2cResetEvent.IsSet
                        ? InvokeResult<byte>.Create(_byteReceived)
                        : InvokeResult<byte>.FromError("Timed out waiting for a matching M261 I2C response.");
                }
                finally
                {
                    LineReceived -= I2C_LineReceived;
                }
            }
            finally
            {
                _i2cReadGate.Release();
            }
        }

        private void I2C_LineReceived(object sender, string line)
        {
            if (!VacuumI2CByteResponse.TryParse(line, _expectedI2cAddress, out var value))
                return;
            _byteReceived = value;
            _i2cResetEvent.Set();
        }
    }
}
