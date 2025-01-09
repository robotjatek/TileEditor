using CommunityToolkit.Mvvm.ComponentModel;

namespace TileEditor.Domain;

public partial class GameObject : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    private string? _type;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    private string? _name;
    // TODO: props

    public string Label
    {
        get => string.IsNullOrEmpty(_name) ? _type ?? string.Empty : _name;
    }

    public GameObject Clone()
    {
        return new GameObject
        {
            Type = _type,
            Name = _name
        };
    }
}
