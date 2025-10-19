// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 597a676423cd4f8f1bdd289439b0a583789392616d315b2454574abe9c011926
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

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for ToolAlignment.xaml
    /// </summary>
    public partial class ToolAlignment : VMBoundUserControl<ICurrentMachineViewModel>
    {
        public ToolAlignment()
        {
            InitializeComponent();
        }
    }
}
