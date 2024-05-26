using CommunityToolkit.Mvvm.ComponentModel;

using System.ComponentModel;
using TileEditor.Domain;

namespace TileEditor;

internal partial class TileSelectorViewModel : ObservableObject
{
    [ObservableProperty]
    private Tile? selected;

    public BindingList<Tile> Data { get; set; } = new BindingList<Tile>([
        new Tile
        {
            TexturePath = null
        },
        new Tile
        {
            TexturePath = "D:/Dev/webgl-engine/textures/ground0.png"
        }]);
}
