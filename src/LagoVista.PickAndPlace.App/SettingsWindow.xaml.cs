﻿using DirectShowLib;
using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Windows;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
	{

        LagoVista.Manufacturing.Models.Machine _settings;

        public SettingsWindow(IMachine machine, LagoVista.Manufacturing.Models.Machine settings, int index = 0)
		{
            _settings = settings;

            DataContext = new SettingsViewModel(machine, _settings);
            
            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var idx = 0;

            ViewModel.Cameras.Add(new MachineCamera()
            {
                Id = "-1",
                Name = "none",
                CameraIndex = -1
            });

            foreach (var camera in cameras)
            {
                ViewModel.Cameras.Add(new MachineCamera()
                {
                    Id = camera.DevicePath,
                    Name = camera.Name,
                    CameraIndex = idx++
                });
            }

            InitializeComponent();

            Tabs.SelectedIndex = index;
		}
        public SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
        }


        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(_settings.MachineName))
            {
                MessageBox.Show("Machine Name is a Required Field");
                Tabs.TabIndex = 0;
                MachineName.Focus();
                return;
            }

            if (String.IsNullOrEmpty(ViewModel.MachineType))
            {
                MessageBox.Show("Machine Type is a Required Field");
                Tabs.TabIndex = 0;
                MachineName.Focus();
                return;
            }

            var rest = SLWIOC.Get<IRestClient>();
            var result = await rest.PutAsync("/api/mfg/machine", _settings);
            if (result.Successful)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(result.ErrorMessage);             
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
