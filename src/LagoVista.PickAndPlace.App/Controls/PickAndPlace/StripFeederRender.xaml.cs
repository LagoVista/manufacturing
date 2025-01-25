using LagoVista.Manufacturing.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.Controls.PickAndPlace
{
    /// <summary>
    /// Interaction logic for StripFeederRender.xaml
    /// </summary>
    public partial class StripFeederRender : UserControl
    {
        ThreeD.StripFeeder _sf = new ThreeD.StripFeeder();
        public StripFeederRender()
        {
            InitializeComponent();
        }

        public void RenderStripFeeder(LagoVista.Manufacturing.Models.StripFeeder feeder)
        {
            var modelGroup = new Model3DGroup();
            _sf.Render3DModel(modelGroup, feeder);
            PCBLayer.Content = modelGroup;
        }

        private static void OnStripFeederChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var host = dependencyObject as StripFeederRender;
            var newRevision = e.NewValue as LagoVista.Manufacturing.Models.StripFeeder;
            var oldRevision = e.OldValue as LagoVista.Manufacturing.Models.StripFeeder;

            if (newRevision != null)
            {
                host.RenderStripFeeder(newRevision);
            }
        }

        public static readonly DependencyProperty StripFeederProperty = DependencyProperty.RegisterAttached(nameof(StripFeeder), typeof(LagoVista.Manufacturing.Models.StripFeeder), typeof(StripFeederRender), new FrameworkPropertyMetadata(OnStripFeederChanged));
        public LagoVista.Manufacturing.Models.StripFeeder StripFeeder
        {
            get => (LagoVista.Manufacturing.Models.StripFeeder)GetValue(StripFeederProperty);
            set => SetValue(StripFeederProperty, value);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RenderStripFeeder(StripFeeder);
        }
    }
}
