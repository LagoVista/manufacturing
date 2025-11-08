// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4687125303456e1dce11ba8c3916cd240895c1701b147c00b3445dde7df2f797
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
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

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// Interaction logic for VacuumControl.xaml
    /// </summary>
    public partial class VacuumControl : VMBoundUserControl<IVacuumViewModel>
    {
        public VacuumControl()
        {
            InitializeComponent();
        }
    }
}
