﻿using LagoVista.Client.Core;
using LagoVista.Client.Core.Models;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Core.WPF.PlatformSupport;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Logging.Models;
using LagoVista.Manufacturing;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Managers;
using LagoVista.Manufacturing.Services;
using LagoVista.PickAndPlace.App.PCB;
using LagoVista.PickAndPlace.App.Services;
using LagoVista.PickAndPlace.App.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces.ViewModels.GCode;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Interfaces.Windows;
using LagoVista.PickAndPlace.LumenSupport;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Repos;
using LagoVista.PickAndPlace.ViewModels;
using LagoVista.PickAndPlace.ViewModels.GCode;
using LagoVista.PickAndPlace.ViewModels.Machine;
using LagoVista.PickAndPlace.ViewModels.PcbFab;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.ViewModels.Vision;
using LagoVista.XPlat.WPF.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DeviceInfo.Register("uwpapp");

            var popups = new PopupService();

            SLWIOC.Register<IDispatcherServices>(new NuvIoTDispatcher(Dispatcher));
            SLWIOC.RegisterSingleton<ILogger>(new AdminLogger(new DebugWriter()));
            SLWIOC.RegisterSingleton<IDeviceManager, Core.WPF.PlatformSupport.DeviceManager>();
            SLWIOC.RegisterSingleton<IPopupServices>(popups);
            SLWIOC.RegisterSingleton<IStorageService, Core.WPF.PlatformSupport.StorageService>();
            SLWIOC.RegisterSingleton<IViewModelNavigation, VMNav>();
            SLWIOC.RegisterSingleton<INetworkService, NetworkService>();
            SLWIOC.RegisterSingleton<IAppConfig, _appCOnfg>();
            SLWIOC.RegisterSingleton<IClientAppInfo, ClientAppInfo>();
            SLWIOC.RegisterSingleton<ITimerFactory, TimerFactory>();
            SLWIOC.Register<IStorageService, Core.WPF.PlatformSupport.StorageService>();
            SLWIOC.Register<IPhotonProtocolHandler, PhotonProtocolHandler>();
            SLWIOC.Register<IPcb2GCodeService, Pcb2GCodeService>();

            var local = new ServerInfo()
            {
                RootUrl = "localhost",
                SSL = false,
                Port = 5001
            };


            var live = new ServerInfo()
            {
                RootUrl = "api.nuviot.com",
                SSL = true,
                Port = 443
            };

            var dev = new ServerInfo()
            {
                RootUrl = "dev-api.nuviot.com",
                SSL = true,
                Port = 443
            };

            LagoVista.Client.Core.Startup.Init(live);

            SLWIOC.RegisterSingleton<IMachineRepo, MachineRepo>();
            
            SLWIOC.RegisterSingleton<IMachineUtilitiesViewModel, MachineUtilitiesViewModel>();
            SLWIOC.Register<IVisionProfileViewModel, VisionProfileViewModel>();
            SLWIOC.Register<IImageCaptureService, ImageCaptureService>();
            SLWIOC.RegisterSingleton<ILocatorViewModel, LocatorViewModel>();
            SLWIOC.Register<IMachineControlViewModel, MachineControlViewModel>();
            SLWIOC.Register<IManualSendViewModel, ManualSendViewModel>();
            SLWIOC.Register<ICurrentMachineViewModel, CurrentMachineViewModel>();
            SLWIOC.Register<INozzleChangeViewModel, NozzleChangeViewModel>();
            SLWIOC.Register<IPickAndPlaceJobResolverService, PickAndPlaceJobResolverService>();

            SLWIOC.RegisterSingleton<IErrorsViewModel, ErrorsViewModel>();
            SLWIOC.RegisterSingleton<IPartInspectionViewModel, PartInspectionViewModel>();
            SLWIOC.RegisterSingleton<IVacuumViewModel, VacuumViewModel>();
            SLWIOC.RegisterSingleton<IPhotonFeederViewModel, PhotonFeederViewModel>();
            SLWIOC.RegisterSingleton<IStripFeederViewModel, StripFeederViewModel>();
            SLWIOC.RegisterSingleton<IAutoFeederViewModel, AutoFeederViewModel>();
            SLWIOC.RegisterSingleton<IPartsViewModel, PartsViewModel>();
            SLWIOC.RegisterSingleton<IJobManagementViewModel, JobManagementViewModel>();            
            SLWIOC.RegisterSingleton<IStagingPlateNavigationViewModel, StagingPlateNavigationViewModel>();
            SLWIOC.RegisterSingleton<IMachineCalibrationViewModel, MachineCalibrationViewModel>();

            SLWIOC.RegisterSingleton<IProbingManager, ProbingManager>();
            SLWIOC.RegisterSingleton<IToolChangeManager, ToolChangeManager>();
            SLWIOC.RegisterSingleton<IGCodeFileManager, GCodeFileManager>();
            

            SLWIOC.RegisterSingleton<IPCBManager, PCBManager>();
            SLWIOC.RegisterSingleton<IHeightMapManager, HeightMapManager>();            
            SLWIOC.RegisterSingleton<IGCodeJobControlViewModel, GCodeJobControlViewModel>();

            SLWIOC.RegisterSingleton<IStagingPlateSelectorViewModel, StagingPlateSelectorViewModel>();
            SLWIOC.RegisterSingleton<IToolHeadViewModel, ToolHeadViewModel>();
            SLWIOC.RegisterSingleton<ICircuitBoardViewModel, CircuitBoardViewModel>();
            SLWIOC.Register<IMachineCoreActionsViewModel, MachineCoreActionsViewModel>();
            SLWIOC.Register<IDryRunViewModel, DryRunViewModel>();
            SLWIOC.Register<IJobExecutionViewModel, JobExecutionViewModel>();
            SLWIOC.Register<IGCodeViewModel, GCodeViewModel>();
            SLWIOC.Register<IPcbMillingViewModel, PcbMillingViewModel>();

            SLWIOC.RegisterSingleton<IMruManager, MruManager>();


            popups.RegisterWindow<IPcbMillingProjectWindow, PCBProjectView>();

            MachineVision.Startup.Init();
            Services.Startup.Init();
        }
    }

    public class DebugWriter : ILogWriter
    {
        public Task WriteError(LogRecord record)
        {
            Debug.WriteLine($"ERROR => {record.Tag} {record.Message}");
            foreach (var param in record.Parameters)
                Debug.WriteLine($"\t{param.Key}={param.Value}");

            return Task.CompletedTask;
        }

        public Task WriteEvent(LogRecord record)
        {
            Debug.WriteLine($"EVENT => {record.Tag} {record.Message}");
            foreach (var param in record.Parameters)
                Debug.WriteLine($"\t{param.Key}={param.Value}");

            return Task.CompletedTask;
        }
    }

    #region Stubs
    public class ClientAppInfo : IClientAppInfo
    {
        public Type MainViewModel => typeof(HomeViewModel);
    }

    public class VMNav : IViewModelNavigation
    {
        public bool CanGoBack()
        {
            throw new NotImplementedException();
        }

        public Task GoBackAsync()
        {
            throw new NotImplementedException();
        }

        public Task GoBackAsync(int dropPageCount)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndCreateAsync<TViewModel>(ViewModelBase parentViewModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndCreateAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndPickAsync<TViewModel>(ViewModelBase parentViewModel, Action<object> selectedAction, Action cancelledAction = null, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync(ViewModelLaunchArgs args)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync(ViewModelBase parentViewModel, Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync<TViewModel>(ViewModelBase parentViewModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task SetAsNewRootAsync<TViewModel>(params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task SetAsNewRootAsync(Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            throw new NotImplementedException();
        }
    }


    public class _appCOnfg : IAppConfig
    {
        public PlatformTypes PlatformType => PlatformTypes.WindowsUWP;

        public Environments Environment => Environments.Production;

        public AuthTypes AuthType => AuthTypes.User;

        public EntityHeader SystemOwnerOrg => null;

        public string WebAddress => "";

        public string CompanyName => "";

        public string CompanySiteLink => "";

        public string AppName => "PCB Commander";

        public string AppId => "{6EAE9868-40BA-47D0-863B-B6A25CA45C28}";

        public string APIToken => "N/A";    

        public string AppDescription => "";

        public string TermsAndConditionsLink => "";
        
        public string PrivacyStatementLink => "";

        public string ClientType => "PC";

        public string AppLogo { get; set; }

        public string CompanyLogo { get; set; }

        public string InstanceId { get; set; }
        public string InstanceAuthKey { get; set; }
        public string DeviceId { get; set; } = "uwpapp";
        public string DeviceRepoId { get; set; }

        public string DefaultDeviceLabel => "Device";

        public string DefaultDeviceLabelPlural => "Devices";

        public bool EmitTestingCode => false;

        public VersionInfo Version => new VersionInfo() { Major = 0, Minor = 5 };

        public string AnalyticsKey { get; set; } = "NA";
    }
    #endregion

}
