using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.App.Controls.PickAndPlace;
using Newtonsoft.Json.Linq;
using System;
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

        object _model;

        List<EntityHeader> _options;
        PropertyInfo _property;

        private void Populate(object model, string fieldName)
        {
            _model = model;

            if (model != null && !string.IsNullOrEmpty(fieldName))
            {
                _property = model.GetType().GetProperty(fieldName);

                var value = _property.GetValue(model) as EntityHeader;
                var attr = _property.GetCustomAttribute<FormFieldAttribute>();
                var formField = FormField.Create(_property.Name, attr, _property);

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
            get => (object)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.RegisterAttached(nameof(FieldName), typeof(string), typeof(Picker), new FrameworkPropertyMetadata(OnFieldNameChanged));
        public string FieldName
        {
            get => (string)GetValue(FieldNameProperty);
            set => SetValue(FieldNameProperty, value);
        }

        private void Value_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOption = Value.SelectedItem as EntityHeader;
            
            var value = Activator.CreateInstance(_property.PropertyType);

            var properties = value.GetType().GetProperties();

            var enumType = _property.PropertyType.GenericTypeArguments.First();
            var enumValues = Enum.GetValues(enumType);
            for (var idx = 0; idx < enumValues.GetLength(0); ++idx)
            {
                var name = enumValues.GetValue(idx).ToString();
                var enumMember = enumType.GetTypeInfo().DeclaredMembers.Where(mbr => mbr.Name == name.ToString()).FirstOrDefault();
                var enumAttr = enumMember.GetCustomAttribute<EnumLabelAttribute>();
                if(enumAttr.Key == selectedOption.Key)
                {
                    properties.Single(prp => prp.Name == nameof(EntityHeader.Id)).SetValue(value, selectedOption.Id);
                    properties.Single(prp => prp.Name == nameof(EntityHeader.Key)).SetValue(value, selectedOption.Key);
                    properties.Single(prp => prp.Name == nameof(EntityHeader.Text)).SetValue(value, selectedOption.Text);
                    properties.Single(prp => prp.Name == nameof(EntityHeader<TapeSizes>.Value)).SetValue(value, enumValues.GetValue(idx));
                    _property.SetValue(_model, value);
                    break;
                }
            }                 
        }
    }
}
