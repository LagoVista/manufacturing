// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4f1963fa55af30f6cf340b67690be75dfea11299b68cf337b770919b2fcfe35d
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.PlatformSupport;
using LagoVista.GCode.Commands;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class ToolChangeManager : MachineViewModelBase, IToolChangeManager
    {
        private readonly IProbingManager _probingManager;

        public ToolChangeManager(IMachineRepo machineRepo, IProbingManager probingManager,  ILogger logger) : base(machineRepo)
        {
            _probingManager = probingManager ?? throw new ArgumentNullException(nameof(probingManager));
        }

        private string _oldTool = "unknown";

        private async Task<bool> PerformToolChange()
        {
            if (!await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", "Please confirm the probe is attached.\r\n\r\nThen press Yes to Continue or No to Abort."))
            {
                return false;
            }

              Machine.SendCommand("G0 Z10 F1000");
            _probingManager.StartProbe();

            SpinWait.SpinUntil(() => Machine.Mode == OperatingMode.Manual, Machine.Settings.ProbeTimeoutSeconds * 1000);

            return _probingManager.Status == ProbeStatus.Success;
        }

        public async Task<bool> HandleToolChange(ToolChangeCommand mcode)
        {
            Machine.SetMode(OperatingMode.Manual);
            Machine.SpindleOff();

            if (await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", $"Start Tool Change cycle?\r\n\r\nLast Changed Tool: {_oldTool}\r\n\r\nNew tool: {mcode.ToolName} ({mcode.ToolSize})"))
            {
                Machine.SendCommand("G0 Z18 F1000");
                Machine.SendCommand("G0 X0 Y0 F1000");

                bool shouldRetry = true;
                while (shouldRetry)
                {
                    if (await PerformToolChange())
                    {
                        _oldTool = mcode.ToolSize;

                        await Core.PlatformSupport.Services.Popups.ShowAsync("IMPORTANT!\r\n\r\nConfirm Probe is Removed and Press Ok.");
                        Machine.SetMode(OperatingMode.SendingGCodeFile);
                        shouldRetry = false;
                    }
                    else
                    {
                        if (!await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", "The Probing Cycle has Failed\r\n\r\nRetry Tool Change Cycle?"))
                        {
                            if (!await Core.PlatformSupport.Services.Popups.ConfirmAsync("Tool Change", "The Tool Change Process has Failed.\r\n\r\nPress Yes to Continue Job.\r\n\r\nNo to Abort"))
                            {
                                shouldRetry = false;
                                return false;
                                
                            }
                            else
                            {
                                Machine.SetMode(OperatingMode.SendingGCodeFile);
                            }
                        }
                    }
                }

                return true;
            }
            else
            {
                Machine.SetMode(OperatingMode.SendingGCodeFile);
                return true;
            }
        }
    }
}
