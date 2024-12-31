using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.XPlat;
using System.Windows.Controls;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for MachineControl.xaml
    /// </summary>
    public partial class MachineControl : VMBoundUserControl<IMachineControlViewModel>
    {
        public MachineControl()
        {            
            InitializeComponent();
        }
    }
}
