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
