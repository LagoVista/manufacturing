// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: aaf19c08906fac23330e64f8fc88171e11162a8d6bf4b8e15a19b3dea3cfde90
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
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
    /// Interaction logic for PickAndPlaceMachineControl.xaml
    /// </summary>
    public partial class PickAndPlaceMachineControl : VMBoundUserControl<IPartsViewModel>
    {
        public PickAndPlaceMachineControl()
        {
            InitializeComponent();
        }
    }
}
