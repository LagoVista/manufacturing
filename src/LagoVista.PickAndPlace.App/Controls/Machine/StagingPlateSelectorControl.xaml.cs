using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.XPlat;
using System.Windows;
using System.Windows.Controls;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// Interaction logic for StagingPlateSelectorControl.xaml
    /// </summary>
    public partial class StagingPlateSelectorControl : VMBoundUserControl<IStagingPlateSelectorViewModel>
    {
        public StagingPlateSelectorControl()
        {
            InitializeComponent();
        }


        private static void OnPropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {

        }


        public static readonly DependencyProperty SelectedStagingPlateRowIdProperty = DependencyProperty.RegisterAttached(nameof(SelectedStagingPlateRowId), typeof(string), typeof(StagingPlateSelectorControl), new FrameworkPropertyMetadata(OnPropChanged));
        public string SelectedStagingPlateRowId
        {
            get => (string)GetValue(SelectedStagingPlateRowIdProperty);
            set => SetValue(SelectedStagingPlateRowIdProperty, value);
        }

        public static readonly DependencyProperty SelectedStagingPlateColIdProperty = DependencyProperty.RegisterAttached(nameof(SelectedStagingPlateColId), typeof(string), typeof(StagingPlateSelectorControl), new FrameworkPropertyMetadata(OnPropChanged)); 
        public string SelectedStagingPlateColId
        {
            get => (string)GetValue(SelectedStagingPlateColIdProperty);
            set => SetValue(SelectedStagingPlateColIdProperty, value);
        }


        public static readonly DependencyProperty SelectedStagingPlateProperty = DependencyProperty.RegisterAttached(nameof(SelectedStagingPlate), typeof(MachineStagingPlate), typeof(StagingPlateSelectorControl), new FrameworkPropertyMetadata(OnPropChanged));
        public MachineStagingPlate SelectedStagingPlate
        {
            get => (MachineStagingPlate)GetValue(SelectedStagingPlateProperty);
            set => SetValue(SelectedStagingPlateProperty, value);
        }
    }
}
