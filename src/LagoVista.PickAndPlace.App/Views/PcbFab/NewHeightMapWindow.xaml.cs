﻿using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels.PcbFab;
using System.Windows;

namespace LagoVista.PickAndPlace.App
{
    public partial class NewHeightMapWindow : Window
	{
        NewHeightMapViewModel _viewModel;
        public NewHeightMapWindow(Window owner, IMachineRepo machine, IHeightMapManager heightMapManager, bool edit)
		{
            Owner = owner;
            _viewModel = new NewHeightMapViewModel(machine);
            _viewModel.HeightMap = edit ? heightMapManager.HeightMap : new HeightMap(_viewModel.Logger);
            ///TODO: Should really disable the edit option if we don't have a height map.
            if (_viewModel.HeightMap == null)
            {
                _viewModel.HeightMap = new HeightMap(_viewModel.Logger);
            }

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataContext = _viewModel;

            InitializeComponent();
        }

        public HeightMap HeightMap
        {
            get { return _viewModel.HeightMap; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
