using System.Windows;

namespace TileEditor;

/// <summary>
/// Interaction logic for ResizeLayerWindow.xaml
/// </summary>
public partial class ResizeLayerWindow : Window
{
    public ResizeLayerWindow()
    {
        InitializeComponent();
        this.Owner = Application.Current.MainWindow;
    }    
}
