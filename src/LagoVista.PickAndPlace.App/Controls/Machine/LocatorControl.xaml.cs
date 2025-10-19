// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cbfc594362fcbf7d525459d54e8321d5994dfa54992ab893bee787ff11f017bc
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
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
    /// Interaction logic for LocatorControl.xaml
    /// </summary>
    public partial class LocatorControl : VMBoundUserControl<ILocatorViewModel>
    {
        public LocatorControl()
        {
            InitializeComponent();
        }
    }
}
