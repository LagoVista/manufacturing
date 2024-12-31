using LagoVista.PickAndPlace.App.ViewModels;
using LagoVista.PickAndPlace.ViewModels;
using System.Windows;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for GCodeWindow.xaml
    /// </summary>
    public partial class GCodeWindow : Window
    {
        public GCodeWindow(HomeViewModel mainViewModel)
        {
            InitializeComponent();

            DataContext = mainViewModel;

            CurrentFile.ShowGCodeWindow.Visibility = Visibility.Collapsed;
        }
    }
}
