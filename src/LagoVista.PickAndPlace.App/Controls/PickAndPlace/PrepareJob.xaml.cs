// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 00171db66e2f201cd2a2ad634f7cd8a6659784492eaf5277a49512e339aeffee
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.XPlat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LagoVista.PickAndPlace.App.Controls.PickAndPlace
{
    /// <summary>
    /// Interaction logic for PrepareJob.xaml
    /// </summary>
    public partial class PrepareJob : VMBoundUserControl<IJobManagementViewModel>
    {
        public PrepareJob()
        {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var placeablePart = e.Row.DataContext as PartsGroup;
            if (placeablePart != null && (placeablePart.AutoFeeder != null || placeablePart.StripFeeder != null && placeablePart.StripFeederRow != null))
            {
                if (placeablePart.AvailableCount >= placeablePart.Count)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Green);
                    e.Row.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    e.Row.Background = new SolidColorBrush(Colors.Yellow);
                }
            }
            else
            {
                e.Row.Background = new SolidColorBrush(Colors.White);
                e.Row.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
    }
}
