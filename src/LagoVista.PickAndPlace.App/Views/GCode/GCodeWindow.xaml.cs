// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fe6c1b2a5378b9ecd648e828c6ab17eb494b88cee7fecc53a1ebcead72323aea
// IndexVersion: 2
// --- END CODE INDEX META ---
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
