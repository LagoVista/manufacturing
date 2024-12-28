using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace LagoVista.PickAndPlace.App.Views
{
    /// <summary>
    /// Interaction logic for OpenObjectView.xaml
    /// </summary>
    public partial class OpenObjectView : Window, INotifyPropertyChanged
    {
        private readonly IRestClient _restClient;

        public ObservableCollection<SummaryData> Items 
        { 
            get; 
            set; 
        }


        public string ObjectName { get; set; }

        public OpenObjectView(IRestClient restClient)
        {
            _restClient = restClient;
            InitializeComponent();
            Loaded += OpenObjectView_Loaded;
            DataContext = this;
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        SummaryData _selectedItem;

        public SummaryData SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if(_selectedItem != null) 
                {
                    this.DialogResult = true;
                    this.Close();
                }
                
            }
        }

        private async void OpenObjectView_Loaded(object sender, RoutedEventArgs e)
        {
            var jobs = await _restClient.GetListResponseAsync<SummaryData>("/api/mfg/pnpjobs", ListRequest.CreateForAll());

            Items = new ObservableCollection<SummaryData>(jobs.Model);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
        }
    }
}
