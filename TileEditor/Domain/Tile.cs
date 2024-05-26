using CommunityToolkit.Mvvm.ComponentModel;

namespace TileEditor.Domain;

public partial class Tile : ObservableObject
{
    [ObservableProperty]
    protected string? _texturePath = null;
    [ObservableProperty]
    protected GameObject? _gameObject = null;

    public Tile() { }

    public virtual Tile Clone()
    {
        return new Tile
        {
            TexturePath = _texturePath
        };
    }
}
