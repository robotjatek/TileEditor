using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;

namespace TileEditor.Domain;

public partial class Layer : ObservableObject
{
    [ObservableProperty]
    private int _width;

    [ObservableProperty]
    private int _height;

    [ObservableProperty]
    private ObservableCollection<Tile> _tiles = [];

    [ObservableProperty]
    private bool _isDefault = false;

    [ObservableProperty]
    private double _parallaxOffsetFactorX = 0.05;

    [ObservableProperty]
    private double _parallaxOffsetFactorY = 0.1;

    [ObservableProperty]
    private double _layerOffsetX = 0;

    [ObservableProperty]
    private double _layerOffsetY = 0;

    public Tile? GetTile(int x, int y)
    {
        var index = y * Width + x;
        if (index >= Tiles.Count || x > Width || y > Height || x < 0 || y < 0)
            return null;

        return Tiles[index];
    }
}
