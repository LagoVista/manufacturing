// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 40ea886c7cbf486266f0866a49dd7040a75022526f78dcf92373a5f45b5cc1e7
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
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
    /// Interaction logic for StripFeederControl.xaml
    /// </summary>
    public partial class StripFeederControl : VMBoundUserControl<IStripFeederViewModel>
    {
        public StripFeederControl()
        {
            InitializeComponent();
        }
    }
}
