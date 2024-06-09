using System.Windows;

namespace TileEditor;

public partial class LayerPropertiesWindow : Window
{
    public LayerPropertiesWindow()
    {
        InitializeComponent();
        this.Owner = Application.Current.MainWindow;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
