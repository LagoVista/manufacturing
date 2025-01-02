using LagoVista.Core.IOC;
using LagoVista.PickAndPlace.App.Properties;
using LagoVista.PickAndPlace.App.ViewModels;
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
using System.Windows.Shapes;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
            DataContext = SLWIOC.CreateForType<HomeViewModel>();
            LocationChanged += Home_LocationChanged;
            SizeChanged += Home_SizeChanged;
            StateChanged += Home_StateChanged;

            if (Settings.Default.WasMaximized)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                Width = Settings.Default.Width;
                Height = Settings.Default.Height;
            }

            Left = Settings.Default.CurrentX;
            Top = Settings.Default.CurrentY;
        }

        private void Home_StateChanged(object sender, EventArgs e)
        {
            Settings.Default.WasMaximized = WindowState == WindowState.Maximized;
            Settings.Default.Save();
        }

        private void Home_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.Default.Width = this.Width;
            Settings.Default.Height = this.Height;
            Settings.Default.Save();
        }

        private void Home_LocationChanged(object sender, EventArgs e)
        {
            Settings.Default.CurrentX = this.Left;
            Settings.Default.CurrentY = this.Top;
            Settings.Default.Save();
        }
    }
}