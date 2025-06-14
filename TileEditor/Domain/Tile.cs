using CommunityToolkit.Mvvm.ComponentModel;

using System.Windows.Media;

namespace TileEditor.Domain;

public partial class Tile : ObservableObject
{
    [ObservableProperty]
    private string? _texturePath = null;
    [ObservableProperty]
    private GameObject? _gameObject = null;
    public bool IsEmpty => string.IsNullOrEmpty(_texturePath) && _gameObject == null;

    public ImageSource? Texture => ImageCache.GetImage(_texturePath);

    public Tile() { }

    public virtual Tile Clone()
    {
        return new Tile
        {
            TexturePath = _texturePath,
            GameObject = _gameObject,
        };
    }
}
