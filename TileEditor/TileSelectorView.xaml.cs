using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using System.Windows.Controls;
using TileEditor.Domain;

namespace TileEditor;

class TileSelectedChangeMessage : ValueChangedMessage<Tile>
{
    public TileSelectedChangeMessage(Tile value) : base(value)
    {
    }
}


/// <summary>
/// Interaction logic for TileSelectorView.xaml
/// </summary>
public partial class TileSelectorView : UserControl
{
    public TileSelectorView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = ((sender as ListBox)!.SelectedItem as Tile)!;
        WeakReferenceMessenger.Default.Send(new TileSelectedChangeMessage(selected));
    }
}
