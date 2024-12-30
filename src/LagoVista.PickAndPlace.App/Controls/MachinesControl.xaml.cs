using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Repos;
using LagoVista.XPlat;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for MachinesControl.xaml
    /// </summary>
    public partial class MachinesControl : VMBoundUserControl<IMachineRepo>
    {
        public MachinesControl()
        {
            InitializeComponent();
        }
    }
}
