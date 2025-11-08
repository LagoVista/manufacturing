// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 87166993ae974896a4da0f296f97bf2d76a99b463be8306a458bb4ec34fa6256
// IndexVersion: 2
// --- END CODE INDEX META ---
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
