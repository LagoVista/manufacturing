using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.XPlat;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    public partial class StagingPlateSelectorControl : VMBoundUserControl<IStagingPlateSelectorViewModel>
    {
        public StagingPlateSelectorControl()
        {
            InitializeComponent();
        }

        private static void OnStagingPlateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctl = dependencyObject as StagingPlateSelectorControl;
            ctl.ViewModel.SelectedStagingPlateId = e.NewValue as string;
        }

        private static void OnStagingPlateColChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctl = dependencyObject as StagingPlateSelectorControl;
            ctl.ViewModel.SelectedStagingPlateColId = e.NewValue as string;
        }

        private static void OnStagingPlateRowChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctl = dependencyObject as StagingPlateSelectorControl;
            ctl.ViewModel.SelectedStagingPlateRowId = e.NewValue as string;
        }

        public static readonly DependencyProperty SelectedStagingPlateRowIdProperty = DependencyProperty.RegisterAttached(nameof(SelectedStagingPlateRowId), typeof(string),
            typeof(StagingPlateSelectorControl), new FrameworkPropertyMetadata(OnStagingPlateRowChanged));
        public string SelectedStagingPlateRowId
        {
            get => (string)GetValue(SelectedStagingPlateRowIdProperty);
            set => SetValue(SelectedStagingPlateRowIdProperty, value);
        }

        public static readonly DependencyProperty SelectedStagingPlateColIdProperty = DependencyProperty.RegisterAttached(nameof(SelectedStagingPlateColId), typeof(string),
            typeof(StagingPlateSelectorControl), new FrameworkPropertyMetadata(OnStagingPlateColChanged)); 
        public string SelectedStagingPlateColId
        {
            get => (string)GetValue(SelectedStagingPlateColIdProperty);
            set => SetValue(SelectedStagingPlateColIdProperty, value);
        }

        public static readonly DependencyProperty SelectedStagingPlateIdProperty = DependencyProperty.RegisterAttached(nameof(SelectedStagingPlateId), typeof(MachineStagingPlate),
            typeof(StagingPlateSelectorControl), new FrameworkPropertyMetadata(OnStagingPlateChanged));
        public string SelectedStagingPlateId
        {
            get => (string)GetValue(SelectedStagingPlateIdProperty);
            set => SetValue(SelectedStagingPlateIdProperty, value);
        }
    }
}
