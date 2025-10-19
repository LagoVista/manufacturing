// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4eedfed5aa2c6e22fcdb85b93d9de44e68088886ed8e976205df0876ef65dea9
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for ImageAdjustments.xaml
    /// </summary>
    public partial class ImageAdjustments : UserControl
    {
        public ImageAdjustments()
        {
            InitializeComponent();
        }

        private void ShowLink_Handler(object sender, MouseButtonEventArgs e)
        {
            var ctl = sender as System.Windows.Controls.Label;
            var url = ctl.Tag;
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url as string,
                UseShellExecute = true
            });
        }
    }
}
