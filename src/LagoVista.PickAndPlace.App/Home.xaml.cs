using LagoVista.Core.IOC;
using LagoVista.PickAndPlace.App.Properties;
using LagoVista.PickAndPlace.App.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

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
                Windowed.Visibility = Visibility.Visible;
                Maximize.Visibility = Visibility.Collapsed;
            }
            else
            {
                Width = Settings.Default.Width;
                Height = Settings.Default.Height;
                Windowed.Visibility = Visibility.Collapsed;
                Maximize.Visibility = Visibility.Visible;
            }

            Left = Settings.Default.CurrentX;
            Top = Settings.Default.CurrentY;
        }

        private void Home_StateChanged(object sender, EventArgs e)
        {
            Settings.Default.WasMaximized = WindowState == WindowState.Maximized;
            Settings.Default.Save();

            if (WindowState == WindowState.Maximized)
            {
                Windowed.Visibility = Visibility.Visible;
                Maximize.Visibility = Visibility.Collapsed;
            }
            else if (WindowState == WindowState.Normal)
            {
                Windowed.Visibility = Visibility.Collapsed;
                Maximize.Visibility = Visibility.Visible;
            }
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

        private void Windowed_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void TitleBar_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowMenu_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}