using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using LagoVista.PickAndPlace.App.ViewModels;
using LagoVista.PickAndPlace.App.Views;
using LagoVista.PickAndPlace.Util;
using LagoVista.PCB.Eagle.Models;
using LagoVista.Client.Core;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.IOC;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.PickAndPlace.App.Properties;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System.Xml.Linq;
using LagoVista.PCB.Eagle.Managers;

namespace LagoVista.PickAndPlace.App
{
    public partial class MainWindow : Window
    {
        IRestClient _restClient;
        IAuthManager _authManager;

        public MainWindow()
        {
            _this = this;
            this._restClient = SLWIOC.Get<IRestClient>();
            this._authManager = SLWIOC.Get<IAuthManager>();


            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                ViewModel = new MainViewModel(Settings.Default.CurrentMachineId);
                DataContext = ViewModel;
                InitializeComponent();

                this.Loaded += MainWindow_Loaded;
            }
        }


        /* Make the main Window Available to contorls */
        static MainWindow _this;
        public static MainWindow This { get { return _this; } }

        

        private async void ChangeMachine_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Machine.Connected)
            {
                MessageBox.Show("Please disconnect before switching machines.");
            }
            else
            {
                var machineId = (sender as MenuItem).Tag.ToString();
                if (!String.IsNullOrEmpty(machineId))
                {
                    var machine = await _restClient.GetAsync<DetailResponse<Manufacturing.Models.Machine>>($"/api/mfg/machine/{machineId}");
                    if (machine.Successful)
                    {
                        Settings.Default.CurrentMachineId = machineId;
                        Settings.Default.Save();
                        ViewModel.Machine.Settings = machine.Result.Model;
                        foreach (var item in MachinesMenu.Items)
                        {
                            var menuItem = item as MenuItem;
                            if (menuItem != null)
                            {
                                //  menuItem.IsChecked = (string)menuItem.Tag == ViewModel.Machine.MachineRepo.CurrentMachineId;
                            }
                        }
                    }
                }
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitAsync();
            await GrblErrorProvider.InitAsync();
            await ViewModel.LoadMRUs();            

            foreach (var file in ViewModel.MRUs.PnPJobs)
            {
                var pnpJobMenu = new MenuItem() { Header = file, Tag = file };
                pnpJobMenu.Click += PnpJobMenu_Click; ;
                RecentPNPJobs.Items.Add(pnpJobMenu);
            }

            foreach (var file in ViewModel.MRUs.BoardFiles)
            {
                var boardMenu = new MenuItem() { Header = file, Tag = file };
                boardMenu.Click += BoardMenu_Click;
                RecentBoards.Items.Add(boardMenu);
            }

            foreach (var file in ViewModel.MRUs.ProjectFiles)
            {
                var projectMenu = new MenuItem() { Header = file, Tag = file };
                projectMenu.Click += ProjectMenu_Click;
                RecentProjects.Items.Add(projectMenu);
            }

            foreach (var file in ViewModel.MRUs.GCodeFiles)
            {
                var gcodeFile = new MenuItem() { Header = file, Tag = file };
                gcodeFile.Click += GcodeFile_Click;
                RecentGCodeFiles.Items.Add(gcodeFile);
            }

            var machines = await _restClient.GetListResponseAsync<LagoVista.Manufacturing.Models.Machine>("/api/mfg/machines", ListRequest.CreateForAll());

            foreach (var machine in machines.Model)
            {
                var menu = new MenuItem() { Header = machine.Name };
                menu.Tag = machine.Id;
                if(machine.Id == Settings.Default.CurrentMachineId)
                    menu.IsChecked = true;

                menu.Click += ChangeMachine_Click;

                MachinesMenu.Items.Add(menu);
            }

            OrganizationMenu.Items.Add(new MenuItem() { Header = $"Current: {_authManager.User.CurrentOrganization.Text} " });
            OrganizationMenu.Items.Add(new Separator());
            var ressponse = await _restClient.GetAsync<ListResponse<OrgUser>>("/api/user/orgs");
            foreach(var org in ressponse.Result.Model)
            {
                var orgMenu = new MenuItem() { Header = org.OrganizationName, Tag = org.OrgId };
                orgMenu.Click += OrgMenu_Click;
                OrganizationMenu.Items.Add(orgMenu);

            }
        }

        private async void OrgMenu_Click(object sender, RoutedEventArgs e)
        {
            var orgId = (sender as MenuItem).Tag;

            var response = await _restClient.GetAsync<InvokeResult<AppUser>>($"/api/org/{orgId}/change");
            if(response.Successful)
            {
                _authManager.User = response.Result.Result.ToUserInfo();
                await _authManager.PersistAsync();
                var machines = await _restClient.GetListResponseAsync<LagoVista.Manufacturing.Models.Machine>("/api/mfg/machines", ListRequest.CreateForAll());

                MachinesMenu.Items.Clear();

                foreach (var machine in machines.Model)
                {
                    var menu = new MenuItem() { Header = machine.Name };
                    menu.Tag = machine.Id;
                    if (machine.Id == Settings.Default.CurrentMachineId)
                        menu.IsChecked = true;

                    menu.Click += ChangeMachine_Click;

                    MachinesMenu.Items.Add(menu);
                }

            }


        }

        private async void PnpJobMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            
            await OpenPnPJobAsync(menu.Tag as string);

            await _pnpJob.OpenAsync();
        }

        private async void GcodeFile_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            await ViewModel.Machine.GCodeFileManager.OpenFileAsync(menu.Tag as string);
        }

        private async void ProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            await ViewModel.OpenProjectAsync(menu.Tag as String);
        }

        private async void BoardMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            await ViewModel.Machine.PCBManager.OpenFileAsync(menu.Tag as String);
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(ViewModel.Machine, ViewModel.Machine.Settings, false).ShowDialog();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ImageSensor.ShutDown();
            if (ViewModel.Machine.Connected)
            {
                await ViewModel.Machine.DisconnectAsync();
            }

            
        }

        MainViewModel _viewModel;
        public MainViewModel ViewModel
        {
            get { return _viewModel; }
            set { _viewModel = value; }
        }

        private void NewHeigtMap_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            if (ViewModel.Machine.PCBManager.HasProject && ViewModel.Machine.PCBManager.HasBoard)
            {
                var heightMap = new HeightMap(ViewModel.Machine, ViewModel.Logger);
                heightMap.Min = new Core.Models.Drawing.Vector2(ViewModel.Machine.PCBManager.Project.ScrapSides, ViewModel.Machine.PCBManager.Project.ScrapTopBottom);
                heightMap.Max = new Core.Models.Drawing.Vector2(ViewModel.Machine.PCBManager.Board.Width + ViewModel.Machine.PCBManager.Project.ScrapSides, ViewModel.Machine.PCBManager.Board.Height + ViewModel.Machine.PCBManager.Project.ScrapTopBottom);
                heightMap.GridSize = ViewModel.Machine.PCBManager.Project.HeightMapGridSize;
                ViewModel.Machine.HeightMapManager.NewHeightMap(heightMap);
            }
            else
            {
                var newHeightMapWindow = new NewHeightMapWindow(this, ViewModel.Machine, false);

                if (newHeightMapWindow.ShowDialog().HasValue && newHeightMapWindow.DialogResult.Value)
                {
                    ViewModel.Machine.HeightMapManager.NewHeightMap(newHeightMapWindow.HeightMap);
                }
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
           // await ViewModel.Machine.MachineRepo.SaveAsync();
        }

        private void NewGeneratedHeigtMap_Click(object sender, RoutedEventArgs e)
        {
            var heightMap = new HeightMap(ViewModel.Machine, ViewModel.Logger);
            ViewModel.Machine.HeightMapManager.CreateTestPattern();
        }

        private void EditHeightMap_Click(object sender, RoutedEventArgs e)
        {
            var newHeightMapWindow = new NewHeightMapWindow(this, ViewModel.Machine, true);
            newHeightMapWindow.ShowDialog();
        }

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void EditMachineMenu_Click(object sender, RoutedEventArgs e)
        {
            //Clone in case we cancel.
            var clonedSettings = ViewModel.Machine.Settings.Clone();
            var dlg = new SettingsWindow(ViewModel.Machine, clonedSettings, false);
            dlg.Owner = this;
            dlg.ShowDialog();
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                ViewModel.Machine.Settings = clonedSettings;
                //ViewModel.Machine.MachineRepo.Machines.Remove(ViewModel.Machine.Settings);
                //ViewModel.Machine.MachineRepo.Machines.Add(clonedSettings);
                //ViewModel.Machine.Settings = clonedSettings;
                //await ViewModel.Machine.MachineRepo.SaveAsync();
            }
        }

        private async void NewMachinePRofile_Click(object sender, RoutedEventArgs e)
        {
            var machineResult = await _restClient.GetAsync<DetailResponse<LagoVista.Manufacturing.Models.Machine>>("/api/mfg/machine/factory");
            machineResult.Result.Model.ApplyDefaults();
            var settings = machineResult.Result.Model;

            var dlg = new SettingsWindow(ViewModel.Machine, settings, true);
            dlg.Owner = this;
            dlg.ShowDialog();
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                    var menu = new MenuItem() { Header = settings.MachineName };
                    menu.Tag = settings.Id;
                    menu.Click += ChangeMachine_Click;

                    MachinesMenu.Items.Add(menu);
            }
        }

        private void PCB2GCode_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Project != null && !String.IsNullOrEmpty(ViewModel.Project.EagleBRDFilePath))
            {
                PCB.PCB2Gode.CreateGCode(ViewModel.Project.EagleBRDFilePath, ViewModel.Project);
            }
            else
            {
                MessageBox.Show("Please Create or Edit a Project PCB->New Project and Assign an Eagle Board File.");
            }
        }

        private async void OpenPCBProject_Click(object sender, RoutedEventArgs e)
        {
            var file = await Core.PlatformSupport.Services.Popups.ShowOpenFileAsync(Constants.PCBProject);
            if (!String.IsNullOrEmpty(file))
            {
                await ViewModel.OpenProjectAsync(file);
                ViewModel.Machine.PCBManager.Project = ViewModel.Project;
            }


        }

        private void ClosePCBProject_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Project = null;
        }

        private void EditPCBProject_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Project == null)
            {
                MessageBox.Show("Please Open or Create a Project First.");
                return;
            }
            var clonedProject = ViewModel.Project.Clone();
            var vm = new PCBProjectViewModel(clonedProject);

            var pcbWindow = new PCBProjectView();
            pcbWindow.DataContext = vm;
            pcbWindow.IsNew = false;
            pcbWindow.Owner = this;
            pcbWindow.PCBFilepath = ViewModel.Machine.PCBManager.ProjectFilePath;
            pcbWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            pcbWindow.ShowDialog();
            if (pcbWindow.DialogResult.HasValue && pcbWindow.DialogResult.Value)
            {
                ViewModel.Project = vm.Project;
            }
        }

        private async void NewPCBProject_Click(object sender, RoutedEventArgs e)
        {
            var pcbWindow = new PCBProjectView();
            var vm = new PCBProjectViewModel(new PcbProject());
            await vm.LoadDefaultSettings();
            pcbWindow.DataContext = vm;
            pcbWindow.IsNew = true;
            pcbWindow.Owner = this;
            pcbWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            pcbWindow.ShowDialog();
            if (pcbWindow.DialogResult.HasValue && pcbWindow.DialogResult.Value)
            {
                ViewModel.Project = vm.Project;
                ViewModel.AddProjectFileMRU(pcbWindow.PCBFilepath);
                if (!String.IsNullOrEmpty(vm.Project.EagleBRDFilePath))
                {
                    await ViewModel.Machine.PCBManager.OpenFileAsync(vm.Project.EagleBRDFilePath);
                }
                ViewModel.Machine.PCBManager.Project = vm.Project;
            }
        }

        private void EditPackageLibrary_Click(object sender, RoutedEventArgs e)
        {
            var librWindow = new Views.PackageLibraryWindow();
            librWindow.Owner = this;
            librWindow.ShowDialog();
        }
        
        private async Task OpenPnPJobAsync(string fileName)
        {
            _pnpJobFileName = fileName;
            ViewModel.AddPnPJobFile(fileName);
            _pnpJob = await Core.PlatformSupport.Services.Storage.GetAsync<PnPJob>(fileName);
            await _pnpJob.OpenAsync();
            var pnpViewModel = new PnPJobViewModel(ViewModel.Machine);
            
            pnpViewModel.InitJob(_pnpJob);
            pnpViewModel.FileName = fileName;
            await pnpViewModel.InitAsync();
            var jobWindow = new Views.PNPJobWindow();
            jobWindow.DataContext = pnpViewModel;
            jobWindow.Show();
            EditPnPJob.IsEnabled = true;
            ClosePnPJob.IsEnabled = true;
            FeederAlignementView.IsEnabled = true;
        }

        private async void OpenPnPJob_Click(object sender, RoutedEventArgs e)
        {
            var file = await Core.PlatformSupport.Services.Popups.ShowOpenFileAsync(Constants.PickAndPlaceProject);
            if (!String.IsNullOrEmpty(file) && System.IO.File.Exists(file))
            {                
                await OpenPnPJobAsync(file);
            }
        }

        PnPJob _pnpJob;
        String _pnpJobFileName;


        private async void NewPnPJob_Click(object sender, RoutedEventArgs e)
        {
            //pnpJob.Board = ViewModel.Machine.PCBManager.Board;
            // pnpJob.PnPMachinePath = ViewModel.Machine.Settings.DefaultPnPMachineFile;



            // pnpJob.EagleBRDFilePath = ViewModel.Machine.PCBManager.Project.EagleBRDFilePath;


            var file = await Core.PlatformSupport.Services.Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!String.IsNullOrEmpty(file) && System.IO.File.Exists(file))
            {
                var doc = XDocument.Load(file);
                var job = new PnPJob();
                job.Board = EagleParser.ReadPCB(doc);


                var pnpViewModel = new PnPJobViewModel(ViewModel.Machine);
                pnpViewModel.InitJob(job);
                await pnpViewModel.InitAsync();
                var jobWindow = new Views.PNPJobWindow();
                jobWindow.DataContext = pnpViewModel;
                jobWindow.Owner = this;
                jobWindow.ShowDialog();
                EditPnPJob.IsEnabled = true;
                FeederAlignementView.IsEnabled = true;
                if (String.IsNullOrEmpty(pnpViewModel.FileName))
                {
                    //         _pnpJob = pnpJob;
                    _pnpJobFileName = pnpViewModel.FileName;
                    EditPnPJob.IsEnabled = true;
                    ClosePnPJob.IsEnabled = true;
                    FeederAlignementView.IsEnabled = true;
                }
            }
        }

        private void ViewMenu_Show(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            switch (menu.Tag.ToString())
            {
                case "WorkAlignment": new Views.WorkAlignmentView(ViewModel.Machine).Show(); break;
                case "ToolAlignment": new Views.ToolAlignment(ViewModel.Machine).Show(); break;
                case "HomingView": new Views.HomingView(ViewModel.Machine).Show(); break;
                case "Calibration": new Views.MVCalibrationView(ViewModel.Machine).Show(); break;
                case "PickAndPlace": new Views.MVPNPView(ViewModel.Machine).Show(); break;
            }
        }

        private async void EditPnPJob_Click(object sender, RoutedEventArgs e)
        {
            if (_pnpJob != null)
            {
                var pnpViewModel = new PnPJobViewModel(ViewModel.Machine);
                pnpViewModel.FileName = _pnpJobFileName;
                pnpViewModel.InitJob(_pnpJob);
                await pnpViewModel.InitAsync();
                var jobWindow = new Views.PNPJobWindow();
                jobWindow.DataContext = pnpViewModel;
                //jobWindow.Owner = this;
                jobWindow.Show();
            }
            else
            {
                MessageBox.Show("Please open a PnP Job First");
            }
        }

        private void ClosePnPJob_Click(object sender, RoutedEventArgs e)
        {
            _pnpJob = null;
            _pnpJobFileName = null;
            EditPnPJob.IsEnabled = false;
            FeederAlignementView.IsEnabled = false;
            ClosePnPJob.IsEnabled = false;
        }

        private async void MachineControl_Click(object sender, RoutedEventArgs e)
        {
            var mcvm = new MVMachineControlViewModel(ViewModel.Machine);
            await mcvm.InitAsync();
            var mcv = new MVMachineControlView();
            mcv.DataContext = mcvm;
            mcv.Show();
        }
    }
}
