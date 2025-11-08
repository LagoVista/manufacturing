// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a7e2d29ff15447f797bec43d14565a11860f5368b996fa6d58a0355f42c4f67c
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
    /// Interaction logic for AutoFeederControl.xaml
    /// </summary>
    public partial class AutoFeederControl : VMBoundUserControl<IAutoFeederViewModel>
    {
        public AutoFeederControl()
        {
            InitializeComponent();
        }
    }
}
