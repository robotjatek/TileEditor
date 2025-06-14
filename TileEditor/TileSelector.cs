using System.Windows;
using System.Windows.Controls;

using TileEditor.Domain;

namespace TileEditor;

internal class TileSelector : DataTemplateSelector
{
    public required DataTemplate EmptyTile { get; set; }

    public required DataTemplate NormalTile { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        Tile tile = (Tile)item;
        if (tile.TexturePath is null)
            return EmptyTile;
        else
            return NormalTile;
    }
}
