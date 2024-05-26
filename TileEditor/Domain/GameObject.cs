using CommunityToolkit.Mvvm.ComponentModel;

namespace TileEditor.Domain;

public partial class GameObject : ObservableObject
{
    [ObservableProperty]
    public string? _type;

    public GameObject Clone()
    {
        return new GameObject { Type = _type };
    }
}
