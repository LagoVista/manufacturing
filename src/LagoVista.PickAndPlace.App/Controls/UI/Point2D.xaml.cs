using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace LagoVista.PickAndPlace.App.Controls.UI
{
    /// <summary>
    /// Interaction logic for Point2D.xaml
    /// </summary>
    public partial class Point2D : UserControl, INotifyPropertyChanged
    {
        public Point2D()
        {
            InitializeComponent();
            FormControl.DataContext = this;
        }

        Point2D<double> _point;
        Object _model;
        PropertyInfo _propertyInfo;

        private void Populate(object model, string fieldName)
        {
            if (model != null && !string.IsNullOrEmpty(fieldName))
            {
                _model = model;
                _propertyInfo = model.GetType().GetProperty(fieldName);
                var attr = _propertyInfo.GetCustomAttribute<FormFieldAttribute>();
                var formField = FormField.Create(_propertyInfo.Name, attr, _propertyInfo);
                _point = _propertyInfo.GetValue(model) as Point2D<double>;
                this.Label.Text = formField.Label;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
            }
        }

        public double? X
        {
            get => _point?.X;
            set
            {
                if (value.HasValue)
                {
                    if (_point != null)
                    {
                        _point.X = value.Value;
                    }
                    else
                    {
                        _point = new Point2D<double>();
                        _point.X = value.Value;
                        _propertyInfo.SetValue(_model, _point);
                    }
                }
            }
        }

        public double? Y
        {
            get => _point?.Y; 
            set
            {
                if (value.HasValue)
                {
                    if (_point != null)
                    {
                        _point.Y = value.Value;
                    }
                    else
                    {
                        _point = new Point2D<double>();
                        _point.X = value.Value;
                        _propertyInfo.SetValue(_model, _point);
                    }
                }
            }
        }

        private static void OnModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var host = dependencyObject as Point2D;
            var model = e.NewValue;

            host.Populate(model, host.FieldName);
        }

        private static void OnFieldNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var host = dependencyObject as Point2D;
            var newFieldName = e.NewValue as string;
            host.Populate(host.Model, newFieldName);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.RegisterAttached(nameof(Model), typeof(object), typeof(Point2D), new FrameworkPropertyMetadata(OnModelChanged));
        public object Model
        {
            get => (FormField)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.RegisterAttached(nameof(FieldName), typeof(string), typeof(Point2D), new FrameworkPropertyMetadata(OnFieldNameChanged));

        public event PropertyChangedEventHandler PropertyChanged;

        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }
    }
}
