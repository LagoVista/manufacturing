// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cbeefa772d5d1a8cdb953115a2bf3f5860957f536026d74f06fe883bfc8a30b3
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces.ViewModels.GCode;
using LagoVista.XPlat;
using System.Windows.Controls;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for JobControl.xaml
    /// </summary>
    public partial class JobControl : VMBoundUserControl<IGCodeJobControlViewModel>
    {
        public JobControl()
        {
            InitializeComponent();
        }
    }
}
