using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using System.Windows.Controls;
using TileEditor.Domain;

namespace TileEditor;

public class EntitySelectedChange : ValueChangedMessage<GameObject>
{
    public EntitySelectedChange(GameObject value) : base(value)
    {
    }
}

/// <summary>
/// Interaction logic for EntitySelectorControl.xaml
/// </summary>
public partial class EntitySelectorControl : UserControl
{
    public EntitySelectorControl()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = (sender as ListBox)!.SelectedItem as GameObject;
        WeakReferenceMessenger.Default.Send(new EntitySelectedChange(selected!));
    }
}
