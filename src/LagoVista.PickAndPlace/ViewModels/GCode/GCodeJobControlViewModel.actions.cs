using LagoVista.Core.IOC;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.GCode
{
    public partial class GCodeJobControlViewModel
    {
        public void EmergencyStop()
        {
           _machineRepo.CurrentMachine.EmergencyStop();
        }

        public void StopJob()
        {
           _machineRepo.CurrentMachine.HeightMapManager.Reset();
           _machineRepo.CurrentMachine.GCodeFileManager.ResetJob();
           _machineRepo.CurrentMachine.SetMode(OperatingMode.Manual);
        }

        public void FeedHold()
        {
           _machineRepo.CurrentMachine.FeedHold();
        }

        public void CycleStart()
        {
           _machineRepo.CurrentMachine.CycleStart();
        }

        public void SoftReset()
        {
           _machineRepo.CurrentMachine.SoftReset();
            if(_machineRepo.CurrentMachine.GCodeFileManager.HasValidFile)
            {
               _machineRepo.CurrentMachine.GCodeFileManager.ResetJob();
            }
        }

        public void SpindleOn()
        {
           _machineRepo.CurrentMachine.SpindleOn();
        }

        public void SpindleOff()
        {
           _machineRepo.CurrentMachine.SpindleOff();
        }

        public void LaserOn()
        {
           _machineRepo.CurrentMachine.LaserOn();
        }

        public void LaserOff()
        {
           _machineRepo.CurrentMachine.LaserOff();
        }

        public void SetWorkspaceHome()
        {
           _machineRepo.CurrentMachine.SetWorkspaceHome();
        }

        public void GotoWorkspaceHome()
        {
           _machineRepo.CurrentMachine.GotoWorkspaceHome();
        }
        
        public void SetFavorite1()
        {
           _machineRepo.CurrentMachine.SetFavorite1();
        }

        public void SetFavorite2()
        {
           _machineRepo.CurrentMachine.SetFavorite2();
        }

        public void GotoFavorite1()
        {
           _machineRepo.CurrentMachine.GotoFavorite1();
        }

        public void GotoFavorite2()
        {
           _machineRepo.CurrentMachine.GotoFavorite2();
        }

        public void HomingCycle()
        {
           _machineRepo.CurrentMachine.HomingCycle();
        }

        public void HomeViaOrigin()
        {
           _machineRepo.CurrentMachine.HomeViaOrigin();
        }

        public void SetAbsoluteWorkSpaceHome()
        {
           _machineRepo.CurrentMachine.SetAbsoluteWorkSpaceHome();
        }

        public void StartProbe()
        {
           _machineRepo.CurrentMachine.ProbingManager.StartProbe();
        }

        public void StartHeightMap()
        {
           _machineRepo.CurrentMachine.HeightMapManager.StartProbing();
        }

        public void SendGCodeFile()
        {
           _machineRepo.CurrentMachine.GCodeFileManager.StartJob();
        }

        public void PauseJob()
        {
           _machineRepo.CurrentMachine.SetMode(OperatingMode.Manual);
        }

        public void ClearAlarm()
        {
           _machineRepo.CurrentMachine.ClearAlarm();
        }
        

        public async void Connect()
        {
            if (_machineRepo.CurrentMachine.Connected)
            {
                await _machineRepo.CurrentMachine.DisconnectAsync();
            }
            else
            {
                if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.SimulatedMachine)
                {
                    await _machineRepo.CurrentMachine.ConnectAsync(new SimulatedSerialPort(_machineRepo.CurrentMachine.Settings.MachineType));
                }
                else if(_machineRepo.CurrentMachine.Settings.ConnectionType == Manufacturing.Models.ConnectionTypes.Serial_Port)
                {
                    var port1 = new SerialPort(_machineRepo.CurrentMachine.Settings.CurrentSerialPort.Name,_machineRepo.CurrentMachine.Settings.CurrentSerialPort.BaudRate);
                    await _machineRepo.CurrentMachine.ConnectAsync(port1);
                }
                else
                {
                    try
                    {
                        var socketClient = SLWIOC.Create<ISocketClient>();

                        await socketClient.ConnectAsync(_machineRepo.CurrentMachine.Settings.IPAddress, 3000);
                        await _machineRepo.CurrentMachine.ConnectAsync(socketClient);
                    }
                    catch(Exception)
                    {
                        
                    }
                }
            }
        }

    }
}
