using Communication.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;

namespace Communication.Wpf
{
    /// <summary>
    /// Interaction logic for InterfaceSelector.xaml
    /// </summary>
    public partial class InterfaceSelector : UserControl
    {
        /// <summary>
        /// To display only Combobox set this flag to high
        /// </summary>
        public bool IsMinimalUI
        {
            get { return (bool)GetValue(IsMinimalUIProperty); }
            set { SetValue(IsMinimalUIProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMinimalUI.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMinimalUIProperty =
            DependencyProperty.Register("IsMinimalUI", typeof(bool), typeof(InterfaceSelector), new PropertyMetadata(false));

        /// <summary>
        /// To set IP manually for some reason we can set it by enabling this check
        /// </summary>
        public bool IsManual
        {
            get { return (bool)GetValue(IsManualProperty); }
            set { SetValue(IsManualProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsManual.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsManualProperty =
            DependencyProperty.Register("IsManual", typeof(bool), typeof(InterfaceSelector), new PropertyMetadata(false, ManualIPSelectionChanged));

        private static void ManualIPSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InterfaceSelector interfaceSelector)
            {
                interfaceSelector.IsManual = (bool)e.NewValue;
            }
        }

        public AddressInfo SelectedInterface
        {
            get { return (AddressInfo)GetValue(SelectedInterfaceProperty); }
            set { SetValue(SelectedInterfaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedInterface.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedInterfaceProperty =
            DependencyProperty.Register("SelectedInterface", typeof(AddressInfo), typeof(InterfaceSelector), new PropertyMetadata(null, SelectedInterfaceChanged));

        private static void SelectedInterfaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InterfaceSelector interfaceSelector)
            {
                if (e.NewValue is null) return;
                interfaceSelector.SelectedInterface = (AddressInfo)e.NewValue;
            }
        }

        public ObservableCollection<AddressInfo> InterfaceList
        {
            get
            {
                return (ObservableCollection<AddressInfo>)GetValue(InterfaceListProperty);
            }
            set { SetValue(InterfaceListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InterfaceList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InterfaceListProperty =
            DependencyProperty.Register("InterfaceList", typeof(ObservableCollection<AddressInfo>), typeof(InterfaceSelector), new PropertyMetadata(null, InterfacesChanging));

        private static void InterfacesChanging(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InterfaceSelector interfaceSelector)
            {
                if (e.NewValue is null) return;
                interfaceSelector.InterfaceList = (ObservableCollection<AddressInfo>)e.NewValue;
            }
        }

        public InterfaceSelector()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
            SelectedInterface = new AddressInfo("", 1500, 100);
            UpdateInterfaces();
            //NetworkChange.NetworkAddressChanged += AddressChangedCallback;
        }

        //private void AddressChangedCallback(object? sender, EventArgs e)
        //{
        //    Application.Current.Dispatcher.Invoke(new Action(() => UpdateInterfaces()));
        //}

        private void UpdateInterfaces()
        {
            InterfaceList = new ObservableCollection<AddressInfo>(NetworkService.GetAllInterfaces());
            //if (InterfaceList is null)
            //{
            //      InterfaceList = new ObservableCollection<AddressInfo>(NetworkService.GetAllInterfaces());
            //}
            //else //Checking if it is different
            //{
            //    var interfaceList = new ObservableCollection<AddressInfo>(NetworkService.GetAllInterfaces());
            //    var newInterfaces = interfaceList.Except(InterfaceList);
            //    foreach (var newInterface in newInterfaces)
            //    {
            //        InterfaceList.Add(newInterface);
            //    }
            //    var removedInterfaces = InterfaceList.Except(interfaceList);
            //    foreach (var removedInterface in removedInterfaces)
            //    {
            //        InterfaceList.Remove(removedInterface);
            //    }
            //}

            if (InterfaceList.Count > 0 && SelectedInterface is null)
            {
                SelectedInterface = InterfaceList[0];
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (NetworkService.ValidateIPv4(textBox.Text))
                {
                    if (SelectedInterface is null)
                    {
                        SelectedInterface = new AddressInfo(textBox.Text, 1500, 1000);
                    }
                    else
                    {
                        SelectedInterface = SelectedInterface with { IP = textBox.Text };
                    }
                }
            }
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            UpdateInterfaces();
        }
    }
}