// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 75e5378ae6659afce14c9ad5937a99d7f00ad833fa7dd242e7eae46f04cb661d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.IOC;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.ViewModels.Machine;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    public partial class StagingPlateSelectorControl : UserControl
    {
        StagingPlateSelectorViewModel _vm;

        public StagingPlateSelectorControl()
        {
            InitializeComponent();

            _vm = new StagingPlateSelectorViewModel(SLWIOC.Get<IMachineRepo>());

            this.StagingPlates.SetBinding(ComboBox.ItemsSourceProperty, new Binding(nameof(StagingPlateNavigationViewModel.StagingPlates)));
            this.StagingPlates.SetBinding(ComboBox.SelectedValueProperty, new Binding(nameof(StagingPlateNavigationViewModel.SelectedStagingPlateId)) { Mode = BindingMode.TwoWay });

            this.StagingPlateRows.SetBinding(ComboBox.ItemsSourceProperty, new Binding(nameof(StagingPlateNavigationViewModel.StagingPlateRows)));
            this.StagingPlateRows.SetBinding(ComboBox.SelectedValueProperty, new Binding(nameof(StagingPlateNavigationViewModel.SelectedStagingPlateRowId)) { Mode = BindingMode.TwoWay });
            this.StagingPlateRows.SetBinding(ComboBox.IsEnabledProperty, new Binding(nameof(StagingPlateNavigationViewModel.CanSelectStagingPlateRow)));


            this.StagingPlateCols.SetBinding(ComboBox.ItemsSourceProperty, new Binding(nameof(StagingPlateNavigationViewModel.StagingPlateCols)));
            this.StagingPlateCols.SetBinding(ComboBox.SelectedValueProperty, new Binding(nameof(StagingPlateNavigationViewModel.SelectedStagingPlateColId)) { Mode = BindingMode.TwoWay });
            this.StagingPlateCols.SetBinding(ComboBox.IsEnabledProperty, new Binding(nameof(StagingPlateNavigationViewModel.CanSelectStagingPlateCol)));

            this.LocationSummary.SetBinding(TextBlock.TextProperty, new Binding(nameof(StagingPlateNavigationViewModel.Summary)));

            this.LocationSummary.DataContext = _vm;
            this.StagingPlates.DataContext = _vm;
            this.StagingPlateRows.DataContext = _vm;
            this.StagingPlateCols.DataContext = _vm;
        }

        private static void LocatedObjectChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ctl = dependencyObject as StagingPlateSelectorControl;
            var locatedObject = e.NewValue as IStagingPlateLocatedObject;
            if(locatedObject != null)
            {
                ctl.StagingPlates.SelectedValue = locatedObject.StagingPlate?.Id ?? StringExtensions.NotSelectedId;
                ctl._vm.SelectedStagingPlateId = locatedObject.StagingPlate?.Id ?? StringExtensions.NotSelectedId;
                ctl._vm.SelectedStagingPlateRowId = locatedObject.ReferenceHoleRow?.Id ?? StringExtensions.NotSelectedId;
                ctl._vm.SelectedStagingPlateColId = locatedObject.ReferenceHoleColumn?.Id ?? StringExtensions.NotSelectedId;
            }            
            else
            {
                ctl._vm.SelectedStagingPlateId = StringExtensions.NotSelectedId;
                ctl._vm.SelectedStagingPlateRowId = StringExtensions.NotSelectedId;
                ctl._vm.SelectedStagingPlateColId = StringExtensions.NotSelectedId;
            }
        }

        private static readonly DependencyProperty StagePlateLocatedObjectProperty = DependencyProperty.RegisterAttached(nameof(StagePlateLocatedObject), typeof(IStagingPlateLocatedObject),
            typeof(StagingPlateSelectorControl), new FrameworkPropertyMetadata(LocatedObjectChanged));
        public IStagingPlateLocatedObject StagePlateLocatedObject
        {
            get => (IStagingPlateLocatedObject)GetValue(StagePlateLocatedObjectProperty);
            set => SetValue(StagePlateLocatedObjectProperty, value);
        }

        private void SavebButto_Click(object sender, RoutedEventArgs e)
        {
            if(StagePlateLocatedObject != null)
            {
                if (string.IsNullOrEmpty(_vm.SelectedStagingPlateRowId) ||
                    string.IsNullOrEmpty(_vm.SelectedStagingPlateRowId) ||
                    _vm.SelectedStagingPlate == null)
                {

                    this.ErrorMessage.Visibility = Visibility.Visible;
                    this.ErrorMessage.Content = "Staging Plate, Row and Column are all required.";
                }
                else
                {
                    StagePlateLocatedObject.StagingPlate = _vm.SelectedStagingPlate.ToEntityHeader();
                    StagePlateLocatedObject.ReferenceHoleRow = _vm.StagingPlateRows.Single(sr => sr.Id == _vm.SelectedStagingPlateRowId);
                    StagePlateLocatedObject.ReferenceHoleColumn = _vm.StagingPlateCols.Single(sr => sr.Id == _vm.SelectedStagingPlateColId);
                    this.EditView.Visibility = Visibility.Collapsed;
                    this.SummaryView.Visibility = Visibility.Visible;
                    this.ErrorMessage.Visibility = Visibility.Collapsed;
                }
                }
            }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.EditView.Visibility = Visibility.Collapsed;
            this.SummaryView.Visibility = Visibility.Visible;
        }

        private void Editbutton_Click(object sender, RoutedEventArgs e)
        {
            this.EditView.Visibility = Visibility.Visible;
            this.SummaryView.Visibility = Visibility.Collapsed;
        }
    }
}
