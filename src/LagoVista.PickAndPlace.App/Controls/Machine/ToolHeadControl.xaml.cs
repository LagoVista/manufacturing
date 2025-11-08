// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c0d2c7a25dd778f4700ae6a508296c070bc2187084e383e7667c006ce075680c
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
    /// Interaction logic for ToolHeadControl.xaml
    /// </summary>
    public partial class ToolHeadControl : VMBoundUserControl<IToolHeadViewModel>
    {
        public ToolHeadControl()
        {
            InitializeComponent();
        }
    }
}
