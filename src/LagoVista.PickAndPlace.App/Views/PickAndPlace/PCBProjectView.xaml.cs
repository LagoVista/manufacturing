// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6e03f28a3b0dea7702e3bc06d5ac6a481e233c1ada6de164646ae1e1b93658b4
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Client.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.MediaServices.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using LagoVista.PickAndPlace.Interfaces.Windows;
using LagoVista.PickAndPlace.ViewModels.PcbFab;
using LagoVista.PickAndPlace.ViewModels.PcbFab.PcbFab;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for PCBProject.xaml
    /// </summary>
    public partial class PCBProjectView : Window, IModalWindow, IPcbMillingProjectWindow
    {
        public PCBProjectView()
        {        
            InitializeComponent();
            var designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!designTime)
            {
                this.Loaded += PCBProject_Loaded;
            }
        }



        private void PCBProject_Loaded(object sender, RoutedEventArgs e)
        {
            //ViewModel.GenerateIsolationEvent += (s, a) => PCB.PCB2Gode.CreateGCode(ViewModel.Project.EagleBRDFileLocalPath, ViewModel.Project);
        }

        public bool ForCreate
        {
            get; set;
        }

        public string PCBFilepath { get; set; }

        public IViewModel ViewModel 
        { 
            get => DataContext as IPcbMillingViewModel;
            set => DataContext = value;
        }

        //private void CloseButton_Click(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //    this.Close();
        //}

        //private async void SaveButton_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void Add_Click(object sender, RoutedEventArgs e)
        //{
        //    var selectedDrill = (sender as Button).DataContext as Hole;

        //    ViewModel.Project.Fiducials.Add(selectedDrill);
        //}

        private void ConsolidatedDrills_Drop(object sender, DragEventArgs e)
        {
            //if (ViewModel.ConsolidatedDrillBit != null)
            //{
            //    var drill = e.Data.GetData("Drill") as DrillBit;
            //    var existingDrill = ViewModel.AddDrillBit(drill);
            //    if (!String.IsNullOrEmpty(existingDrill))
            //    {
            //        MessageBox.Show($"This drill bit already exists on: {existingDrill}");
            //    }
            //}
        }

        private void SourceBitsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var drill = (sender as Grid).DataContext;
                var data = new DataObject();
                data.SetData("Drill", drill);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void ConsolidatedBitsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var drill = (sender as Grid).DataContext;

                var data = new DataObject();
                data.SetData("Drill", drill);
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }


        //private void AddConsolidatedDrill_Click(object sender, RoutedEventArgs e)
        //{
        //    var dlg = new ConsolidatedDrillBitView();
        //    dlg.ShowForNew(ViewModel.Project, this);
        //    if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
        //    {
        //       // ViewModel.ConsolidatedDrillBit = dlg.ConsolidatedDrill;
        //    }
        //}

        //private void ConsolidatedDrillBitItem_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount >= 2)
        //    {
        //        var consolidatedDrill = (sender as TextBlock).DataContext as ConsolidatedDrillBit;
        //        var dlg = new ConsolidatedDrillBitView();
        //        dlg.ShowForEdit(ViewModel.Project, consolidatedDrill, this);
        //    }
        //}

        private void FullDrillIst_Drop(object sender, DragEventArgs e)
        {
            //    if (ViewModel.ConsolidatedDrillBit != null)
            //    {
            //        var drill = e.Data.GetData("Drill") as DrillBit;
            //        ViewModel.RemoveBit(drill);
            //    }
        }

        //public Task<bool> CloseAsync()
        //{
        //    return Task.FromResult(false);
        //}
    }
}