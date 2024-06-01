using System.Windows;

namespace TileEditor;

/// <summary>
/// Interaction logic for EditLevelPropertiesWindow.xaml
/// </summary>
public partial class EditLevelPropertiesWindow : Window
{
    public EditLevelPropertiesWindow()
    {
        InitializeComponent();
        this.Owner = Application.Current.MainWindow;
        this.DataContext = Application.Current.MainWindow.DataContext;
    }
}
