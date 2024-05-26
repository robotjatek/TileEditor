using CommunityToolkit.Mvvm.ComponentModel;

using System.ComponentModel;

using TileEditor.Domain;

namespace TileEditor;

public partial class EntitySelectorViewModel : ObservableObject
{
    [ObservableProperty]
    private GameObject? _selected;

    public BindingList<GameObject> Data {  get; set; } = new BindingList<GameObject>([
        new GameObject
        {
            Type = "coin"
        },
        new GameObject
        {
            Type = "health"
        }
    ]);
}
