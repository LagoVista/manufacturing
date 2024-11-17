using LagoVista.Client.Core;
using LagoVista.Client.Core.Auth;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.Net;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Core.WPF.PlatformSupport;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Logging.Models;
using LagoVista.PickAndPlace.App.Services;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.ViewModels;
using LagoVista.XPlat.WPF.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
            SLWIOC.Register<IDispatcherServices>(new NuvIoTDispatcher(Dispatcher));
            SLWIOC.Register<ISocketClient, SocketClient>();
            SLWIOC.Register<IDeviceInfo, DeviceInfo>();
            SLWIOC.RegisterSingleton<ILogger>(new AdminLogger(new DebugWriter()));
            SLWIOC.RegisterSingleton<IDeviceManager, Core.WPF.PlatformSupport.DeviceManager>();
            SLWIOC.RegisterSingleton<IPopupServices, PopupService>();
            SLWIOC.RegisterSingleton<IStorageService, Core.WPF.PlatformSupport.StorageService>();
            SLWIOC.RegisterSingleton<IViewModelNavigation, VMNav>();
            SLWIOC.RegisterSingleton<INetworkService, NetworkService>();
            SLWIOC.RegisterSingleton<IAppConfig, _appCOnfg>();
            SLWIOC.RegisterSingleton<IClientAppInfo, ClientAppInfo>();

            LagoVista.Client.Core.Startup.Init(new ServerInfo()
            {
                RootUrl = "localhost",
                SSL = false,
                Port = 5001
            });
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

    #region Strubs
    public class ClientAppInfo : IClientAppInfo
    {
        public Type MainViewModel => typeof(MainViewModel);
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

        public EntityHeader SystemOwnerOrg => EntityHeader.Create("08AB41B2727F4E398FCF6A683EE0EEB4", "NA");

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
