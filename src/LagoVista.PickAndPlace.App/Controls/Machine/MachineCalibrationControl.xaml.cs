// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fda4631678668997e135f92a6fa9732e1bf84799bbc98e9dbbc66cd9e33b3c50
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
    /// Interaction logic for MachineCalibrationControl.xaml
    /// </summary>
    public partial class MachineCalibrationControl : VMBoundUserControl<IMachineCalibrationViewModel>
    {
        public MachineCalibrationControl()
        {
            InitializeComponent();
        }
    }
}
