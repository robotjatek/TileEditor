using CommunityToolkit.Mvvm.ComponentModel;

using System.ComponentModel;

namespace TileEditor;

public class Tile
{
    protected string? _texturePath = null;

    public Tile() { }

    public virtual string? TexturePath
    {
        get
        {
            return _texturePath;
        }
        set
        {
            _texturePath = value;
        }
    }

    public virtual Tile Clone()
    {
        return new Tile
        {
            TexturePath = _texturePath
        };
    }
}

public class EmptyTile : Tile
{
    public EmptyTile() : base()
    {
        TexturePath = null;
    }

    public override string? TexturePath
    {
        get => null;
        set => _texturePath = null;
    }

    public override Tile Clone()
    {
        return new EmptyTile();
    }
}

internal partial class TileSelectorViewModel : ObservableObject
{
    [ObservableProperty]
    private Tile? selected;

    public BindingList<Tile> Data { get; set; } = new BindingList<Tile>([
        new EmptyTile(),
        new Tile
        {
            TexturePath = "D:\\Dev\\webgl-engine\\textures\\ground0.png"
        }]);
}
