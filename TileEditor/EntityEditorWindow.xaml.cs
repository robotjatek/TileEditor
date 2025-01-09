using System.Windows;

namespace TileEditor;

/// <summary>
/// Interaction logic for EntityEditorWindow.xaml
/// </summary>
public partial class EntityEditorWindow : Window
{
    public EntityEditorWindow(EntityEditorWindowViewModel vm)
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
