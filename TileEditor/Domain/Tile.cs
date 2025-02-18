using CommunityToolkit.Mvvm.ComponentModel;

using System.Windows.Media;

namespace TileEditor.Domain;

public partial class Tile : ObservableObject
{
    [ObservableProperty]
    protected string? _texturePath = null;
    [ObservableProperty]
    protected GameObject? _gameObject = null;

    public ImageSource? Texture => ImageCache.GetImage(_texturePath);

    public Tile() { }

    public virtual Tile Clone()
    {
        return new Tile
        {
            TexturePath = _texturePath
        };
    }
}
