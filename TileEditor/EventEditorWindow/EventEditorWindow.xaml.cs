using System.Windows;

namespace TileEditor.EventEditorWindow;

/// <summary>
/// Interaction logic for EventEditorWindow.xaml
/// </summary>
public partial class EventEditorWindow : Window
{
    public EventEditorWindow(EventEditorViewModel vm)
    {
        InitializeComponent();
        Owner = App.Current.MainWindow;
        DataContext = vm;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
