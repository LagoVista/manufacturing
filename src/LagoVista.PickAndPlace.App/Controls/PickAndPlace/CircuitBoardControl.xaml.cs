// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c6357b0866b3f5052c0e07632f6b07cf79e547d64f8c6b9a2db4f7507032e5b3
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
    /// Interaction logic for CircuitBoardControl.xaml
    /// </summary>
    public partial class CircuitBoardControl : VMBoundUserControl<ICircuitBoardViewModel>
    {
        public CircuitBoardControl()
        {
            InitializeComponent();
        }
    }
}
