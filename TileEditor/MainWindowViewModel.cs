using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;

using TileEditor.Domain;
using TileEditor.DTOs;

namespace TileEditor;

public class LevelProperties
{
    public required string BackgroundPath { get; init; }
    public required string MusicPath { get; init; }
    public required string NextLevel { get; init; }
    public required string Name { get; init; }
}

// TODO: menu for custom level properties: bg, music, name, next level
// TODO: layer properties
// TODO: multi layer support
// TODO: layer domain object and layer DTO
public partial class MainWindowViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions serializeOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [ObservableProperty]
    private int _tabIndex = 0;

    // TODO: resize layer
    // TODO: configurable bigger numbers
    [ObservableProperty]
    private int _layerWidth = 32; // TODO: this is the minimum width based on Environment.ts

    [ObservableProperty]
    private int _layerHeight = 18; // TODO: this is the minimum height based on Environment.ts

    [ObservableProperty]
    private Tile? _selectedTile;

    [ObservableProperty]
    private GameObject? _selectedGameObject;

    // TODO: selected layer

    [ObservableProperty]
    private ObservableCollection<Tile> _layerTiles = []; // TODO: multi layer support


    public MainWindowViewModel()
    {
        for (int i = 0; i < _layerWidth * _layerHeight; i++)
        {
            _layerTiles.Add(new Tile
            {
                TexturePath = null,
            });
        }

        WeakReferenceMessenger.Default.Register<TileSelectedChangeMessage>(this, (r, m) =>
        {
            ReceiveSelectedTile(m);
        });
        WeakReferenceMessenger.Default.Register<EntitySelectedChange>(this, (r, m) =>
        {
            ReceiveSelectedGameObject(m);
        });
    }

    public void ReceiveSelectedTile(ValueChangedMessage<Tile> message)
    {
        SelectedTile = message.Value;
    }

    private void ReceiveSelectedGameObject(ValueChangedMessage<GameObject> message)
    {
        SelectedGameObject = message.Value;
    }

    public Tile GetTile(int x, int y)
    {
        var index = y * _layerWidth + x;
        return _layerTiles[index];
    }

    /// <summary>
    /// Places the selected tile or entity to the place of the incoming parameter. 
    /// In tile edit mode no-op if the parameter is not found in the tiles array
    /// </summary>
    /// <param name="clickedTile">The tile that was clicked on</param>
    public void PlaceSelected(Tile clickedTile)
    {
        if (TabIndex == 0) // tile edit mode
        {
            if (SelectedTile != null)
            {
                var tileIndexInArray = _layerTiles.IndexOf(clickedTile);
                if (tileIndexInArray >= 0)
                    _layerTiles[tileIndexInArray] = SelectedTile!.Clone();
            }
        }
        else if (TabIndex == 1) // game object mode
        {
            if (SelectedGameObject == null)
                return;

            if (SelectedGameObject is StartGameObject)
            {
                // remove any existing start objects as only one can be present on a level
                var existingStart = _layerTiles.Where(t => t.GameObject?.Type == "start").FirstOrDefault();
                if (existingStart != null)
                    existingStart.GameObject = null;
            } 
            else if (SelectedGameObject is EndGameObject) // TODO: make multiple end objects possible (needs game engine support)
            {
                var existingEnd = _layerTiles.Where(t => t.GameObject?.Type == "end").FirstOrDefault();
                if (existingEnd != null)
                    existingEnd.GameObject = null;
            }

            clickedTile.GameObject = SelectedGameObject.Clone();
        }
    }

    [RelayCommand]
    private void OnSave()
    {
        var tiles = _layerTiles.Select((t, i) =>
        {
            if (t.TexturePath == null)
                return null;

            var posX = i % _layerWidth;
            var posY = i / _layerWidth;
            var ingamePath = Path.Combine("textures", Path.GetFileName(t.TexturePath)!); // TODO: game relative path
            // TODO: GameRootFolder
            // TODO: Path.GetRelativePath

            return new TileEntity
            {
                Texture = ingamePath,
                XPos = posX,
                YPos = posY
            };
        }).Where(t => t != null).ToArray();

        var layer = new LayerEntity
        {
            Tiles = tiles!
        };

        var start = new StartEntity();
        var levelEnd = new LevelEndEntity();

        var gameObjects = _layerTiles.Select((t, i) =>
        {
            var gameObject = t.GameObject;
            if (gameObject == null)
                return null;

            var posX = i % _layerWidth;
            var posY = i / _layerWidth;

            // start is not a "GameObject" as the game expects it, but for the level editor it is convenient to handle it like one
            if (gameObject.Type == "start")
            {
                start.XPos = posX;
                start.YPos = posY;
                return null;
            }
            else if(gameObject.Type == "end")
            {
                levelEnd.XPos = posX;
                levelEnd.YPos = posY;
                return null; // TODO: handle level end as a proper game object (Needs engine support)
            }

            return new GameObjectEntity
            {
                Type = gameObject.Type!,
                XPos = posX,
                YPos = posY
            };
        }).Where(g => g != null).ToArray();

        var level = new LevelEntity
        {
            Background = "textures/bg.jpg", // TODO: selectable in editor
            GameObjects = gameObjects!,
            Music = "audio/level.mp3", // TODO: selectable in editor
            Layers = [layer], // TODO: multi layer support
            LevelEnd = levelEnd,
            Start = start,
            NextLevel = "levels/level2.json" // TODO: make it configurable
        };

        var levelName = "level1.json"; // TODO: custom level name
        var levelJson = JsonSerializer.Serialize(level, serializeOptions); // TODO: async serialize
        File.WriteAllText(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), levelName),
            levelJson); // TODO: async save
    }

    [RelayCommand]
    private void OnEdit()
    {
        var _editLevelPropertiesWindow = new EditLevelPropertiesWindow();
        _editLevelPropertiesWindow.ShowDialog();
    }

    [RelayCommand]
    private void NotImplemented()
    {
        MessageBox.Show("Not implemented", "Err", MessageBoxButton.OK);
    }
}
