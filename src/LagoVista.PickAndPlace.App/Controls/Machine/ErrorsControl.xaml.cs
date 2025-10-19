// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f01daf38db26ca86ebd1e7e89bb308774811bbd384ab6f622c41644cba913207
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
    /// Interaction logic for ErrorsControl.xaml
    /// </summary>
    public partial class ErrorsControl : VMBoundUserControl<IErrorsViewModel>
    {
        public ErrorsControl()
        {
            InitializeComponent();
        }
    }
}
