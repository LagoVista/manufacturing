﻿using LagoVista.PickAndPlace.App.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.ViewModels;
using LagoVista.XPlat;
using RingCentral;
using System.ComponentModel;
using System.Windows.Controls;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for CurrentFileStatus.xaml
    /// </summary>
    public partial class CurrentFileControl : VMBoundUserControl<IGCodeFileManager>
    {
        GCodeWindow _gCodeWindow;
        public CurrentFileControl()
        {
            InitializeComponent();
        }


        private void ShowGCodeWindow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_gCodeWindow == null)
            {
                _gCodeWindow = new GCodeWindow(DataContext as HomeViewModel);
                //_gCodeWindow.Owner = MainWindow.This;
                _gCodeWindow.Closing += _window_Closing;
                _gCodeWindow.Show();
            }
        }

        private void _window_Closing(object sender, CancelEventArgs e)
        {
            _gCodeWindow = null;
        }

        private void EditGCode_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
