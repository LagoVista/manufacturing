// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a28df1aa45d867955ff2a2d52a7205aae10702e4deab016f05f3012b530979f3
// IndexVersion: 0
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
    /// Interaction logic for MachineCoreActionsControl.xaml
    /// </summary>
    public partial class MachineCoreActionsControl : VMBoundUserControl<IMachineCoreActionsViewModel>
    {
        public MachineCoreActionsControl()
        {
            InitializeComponent();
        }
    }
}
