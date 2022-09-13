using Communication.Core;
using System.Collections.ObjectModel;
using System.Windows;

namespace Communication.Wpf.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsManual { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public AddressInfo SelectedInterface { get; set; }
        public ObservableCollection<AddressInfo> InterfaceList { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}