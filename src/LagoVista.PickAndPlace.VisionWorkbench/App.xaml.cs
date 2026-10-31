using LagoVista.Client.Core.Models;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.XPlat.WPF.Services;
using System.Windows;

namespace LagoVista.PickAndPlace.VisionWorkbench;

public partial class App : Application
{
    public App()
    {
        DeviceInfo.Register("vision-workbench");

        var config = new WorkbenchAppConfig();
        SLWIOC.RegisterSingleton<IAppConfig>(config);
        LagoVista.Xplat.WPF.Startup.Init(config);

        LagoVista.Client.Core.Startup.Init(new ServerInfo
        {
            RootUrl = "api.nuviot.com",
            SSL = true,
            Port = 443
        });

        SLWIOC.Register<IDispatcherServices>(new WorkbenchDispatcher(Dispatcher));
    }

    private sealed class WorkbenchDispatcher : IDispatcherServices
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher;
        public WorkbenchDispatcher(System.Windows.Threading.Dispatcher dispatcher) => _dispatcher = dispatcher;
        public void Invoke(Action action) => _dispatcher.BeginInvoke(action);
    }

    private sealed class WorkbenchAppConfig : IAppConfig
    {
        public PlatformTypes PlatformType => PlatformTypes.WindowsUWP;
        public Environments Environment => Environments.Production;
        public AuthTypes AuthType => AuthTypes.User;
        public EntityHeader SystemOwnerOrg => null;
        public string WebAddress => "";
        public string CompanyName => "Software Logistics";
        public string CompanySiteLink => "";
        public string AppName => "Vision Workbench";
        public string AppId => "{6EAE9868-40BA-47D0-863B-B6A25CA45C28}";
        public string APIToken => "N/A";
        public string AppDescription => "Pick and place vision and calibration workbench";
        public string TermsAndConditionsLink => "";
        public string PrivacyStatementLink => "";
        public string ClientType => "PC";
        public string AppLogo { get; set; }
        public string CompanyLogo { get; set; }
        public string InstanceId { get; set; }
        public string InstanceAuthKey { get; set; }
        public string DeviceId { get; set; } = "vision-workbench";
        public string DeviceRepoId { get; set; }
        public string DefaultDeviceLabel => "Device";
        public string DefaultDeviceLabelPlural => "Devices";
        public bool EmitTestingCode => false;
        public VersionInfo Version => new() { Major = 0, Minor = 1 };
        public string AnalyticsKey { get; set; } = "NA";
    }
}
