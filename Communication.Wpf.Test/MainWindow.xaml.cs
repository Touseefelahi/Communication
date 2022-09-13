using Communication.Core;
using System.Collections.ObjectModel;
using System.Linq;
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
            var interfaceList = NetworkService.GetAllInterfaces().ToList();
            if (interfaceList.Count > 0)
            {
                SelectedInterface = interfaceList[0];
            }
        }

        public AddressInfo SelectedInterface { get; set; }
        public ObservableCollection<AddressInfo> InterfaceList { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}