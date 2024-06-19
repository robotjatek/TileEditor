using CommunityToolkit.Mvvm.ComponentModel;

namespace TileEditor.Domain;

public partial class LevelEvent : ObservableObject
{
    [ObservableProperty]
    public string _type = "";

    [ObservableProperty]
    public Dictionary<string, object> _props = [];
}
