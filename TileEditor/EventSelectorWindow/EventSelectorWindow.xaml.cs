using System.Windows;

namespace TileEditor.EventSelectorWindow;

/// <summary>
/// Interaction logic for EventSelectorWindow.xaml
/// </summary>
public partial class EventSelectorWindow : Window
{
    public EventSelectorWindow(EventSelectorWindowViewModel vm)
    {
        InitializeComponent();
        this.Owner = App.Current.MainWindow;
        DataContext = vm;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
