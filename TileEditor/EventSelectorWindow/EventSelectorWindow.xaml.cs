using System.Windows;

namespace TileEditor.EventSelectorWindow;

/// <summary>
/// Interaction logic for EventSelectorWindow.xaml
/// </summary>
public partial class EventSelectorWindow : Window
{
    public EventSelectorWindow(EventSelectorWindowViewModel wm)
    {
        InitializeComponent();
        this.Owner = App.Current.MainWindow;
        DataContext = wm;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
