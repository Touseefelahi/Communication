This is simple UI control to select the Network Interface in single/multi network device

Add nuget package and use this namespace in xaml file

    xmlns:comm="clr-namespace:Communication.Wpf;assembly=Communication.Wpf"

And use it like this control

    <comm:InterfaceSelector            
            InterfaceList="{Binding InterfaceList, Mode=TwoWay}"
            IsManual="{Binding IsManual, Mode=TwoWay}"
            SelectedInterface="{Binding SelectedInterface, Mode=TwoWay}" />

For Just a combo box set IsMinimalUI = True

    <comm:InterfaceSelector
        IsMinimalUI="True"
        InterfaceList="{Binding InterfaceList, Mode=TwoWay}"
        IsManual="{Binding IsManual, Mode=TwoWay}"
        SelectedInterface="{Binding SelectedInterface, Mode=TwoWay}" />