// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fa3b15329b65e67a8ec2b7653a1ca3fc7d76be5ecaac3fc2232e715607793267
// IndexVersion: 0
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
    /// Interaction logic for PhotonFeedersControl.xaml
    /// </summary>
    public partial class PhotonFeedersControl : VMBoundUserControl<IPhotonFeederViewModel>
    {
        public PhotonFeedersControl()
        {
            InitializeComponent();
        }
    }
}
