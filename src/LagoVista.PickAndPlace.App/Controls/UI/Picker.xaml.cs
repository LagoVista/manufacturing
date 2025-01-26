using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.PickAndPlace.App.Controls.PickAndPlace;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace LagoVista.PickAndPlace.App.Controls.UI
{
    /// <summary>
    /// Interaction logic for Picker.xaml
    /// </summary>
    public partial class Picker : UserControl
    {
        public Picker()
        {
            InitializeComponent();
        }

        List<EntityHeader> _options;

        private void Populate(object model, string fieldName)
        {
            if (model != null && !string.IsNullOrEmpty(fieldName))
            {
                var property = model.GetType().GetProperty(fieldName);

                var value = property.GetValue(model) as EntityHeader;
                var attr = property.GetCustomAttribute<FormFieldAttribute>();
                var formField = FormField.Create(property.Name, attr, property);

                _options = formField.Options.Select(opt => EntityHeader.Create(opt.Id, opt.Key, opt.Label)).ToList();
                _options.Insert(0, EntityHeader.Create("-1", "-1", formField.Watermark));

                Label.Text = formField.Label;
                Value.DisplayMemberPath = "Text";
                Value.ItemsSource = _options;

                if (value != null)
                {
                    Value.SelectedItem = _options.Where(opt => opt.Id == value.Id).FirstOrDefault();
                }
                else
                {
                    Value.SelectedIndex = 0;
                }
            }
        }


        private static void OnModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var host = dependencyObject as Picker;
            var model = e.NewValue;

            host.Populate(model, host.FieldName);
        }

        private static void OnFieldNameChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var host = dependencyObject as Picker;
            var newFieldName = e.NewValue as string;
            host.Populate(host.Model, newFieldName);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.RegisterAttached(nameof(Model), typeof(object), typeof(Picker), new FrameworkPropertyMetadata(OnModelChanged));
        public object Model
        {
            get => (FormField)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.RegisterAttached(nameof(FieldName), typeof(string), typeof(Picker), new FrameworkPropertyMetadata(OnFieldNameChanged));
        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }
    }
}
