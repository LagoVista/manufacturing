// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7719aa2ab376aba1602ae4fea4ee7d020631f2f27cc0bf594fac477c3292fa89
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
    /// Interaction logic for PickAndPlaceJobControl.xaml
    /// </summary>
    public partial class PickAndPlaceJobControl : VMBoundUserControl<IJobExecutionViewModel>
    {
        public PickAndPlaceJobControl()
        {
            InitializeComponent();
        }
    }
}
