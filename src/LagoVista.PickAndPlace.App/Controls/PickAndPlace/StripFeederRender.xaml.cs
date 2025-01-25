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

            Vector3D axis = new Vector3D(0, 0, 1); //In case you want to rotate it about the x-axis
            Matrix3D transformationMatrix = modelGroup.Transform.Value; //Gets the matrix indicating the current transformation value
            var rotate = new Matrix3D();
            rotate.Rotate(new Quaternion(axis, -90));
            
            var translate = new Matrix3D();
            translate.Translate(new Vector3D(-feeder.Length / 2, -feeder.Width / 2, 0));
            translate.Rotate(new Quaternion(axis, -90));
            //   var moveToCenter = Matrix3D. TranslateTransform3D(-feeder.Length / 2, -feeder.Width / 2, 0);
       //     var combinedTransForm = translate + rotate;

            modelGroup.Transform = new MatrixTransform3D(translate);

            PCBLayer.Content = modelGroup;
//            PCBLayer.Transform = new TranslateTransform3D(-feeder.Length / 2, -feeder.Width / 2, 0);
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
