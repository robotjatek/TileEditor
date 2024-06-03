using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;

using TileEditor.Domain;
using TileEditor.DTOs;

using static TileEditor.ResizeLayerWindowViewModel;

namespace TileEditor;

public partial class LevelProperties : ObservableObject
{
    [ObservableProperty]
    private string _name = "level1.json";

    [ObservableProperty]
    private string _backgroundPath = "textures/bg.jpg";

    [ObservableProperty]
    private string _musicPath = "audio/level.mp3";

    [ObservableProperty]
    private string _nextLevel = "levels/level1.json";
}

// TODO: layer properties
// TODO: multi layer support
// TODO: layer domain object and layer DTO
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _gamePath = "D:\\Dev\\webgl-engine";

    private static readonly JsonSerializerOptions serializeOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [ObservableProperty]
    private int _tabIndex = 0;

    [ObservableProperty]
    private int _layerWidth = 32;

    [ObservableProperty]
    private int _layerHeight = 18;

    [ObservableProperty]
    private Tile? _selectedTile;

    [ObservableProperty]
    private GameObject? _selectedGameObject;

    // TODO: selected layer

    [ObservableProperty]
    private ObservableCollection<Tile> _layerTiles = []; // TODO: multi layer support

    [ObservableProperty]
    private LevelProperties _levelProperties = new();

    public MainWindowViewModel()
    {
        for (int i = 0; i < LayerWidth * LayerHeight; i++)
        {
            LayerTiles.Add(new Tile
            {
                TexturePath = null,
            });
        }

        WeakReferenceMessenger.Default.Register<TileSelectedChangeMessage>(this, (r, m) => ReceiveSelectedTile(m));
        WeakReferenceMessenger.Default.Register<EntitySelectedChange>(this, (r, m) => ReceiveSelectedGameObject(m));
        WeakReferenceMessenger.Default.Register<LayerResizeMessage>(this, (r, m) => ReceiveLayerResizeMessage(m));
    }

    public void ReceiveSelectedTile(ValueChangedMessage<Tile> message)
    {
        SelectedTile = message.Value;
    }

    private void ReceiveSelectedGameObject(ValueChangedMessage<GameObject> message)
    {
        SelectedGameObject = message.Value;
    }

    private void ReceiveLayerResizeMessage(ValueChangedMessage<LayerSizeProperties> message)
    {
        if (message.Value.Height < LayerHeight || message.Value.Width < LayerWidth)
        {
            // TODO: proper warn window with editor style instead of this messagebox
            var result = MessageBox.Show("The dimensions provided are smaller than the current size. This can result in losing tiles. Do you want to proceed?",
                "Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
                return;
        }

        ResizeLayer(message.Value.Width, message.Value.Height);
    }

    public Tile? GetTile(int x, int y)
    {
        var index = y * LayerWidth + x;
        if (index >= LayerTiles.Count || x > LayerWidth || y > LayerHeight || x < 0 || y < 0)
            return null;

        return LayerTiles[index];
    }

    /// <summary>
    /// Places the selected tile or entity to the place of the incoming parameter. 
    /// In tile edit mode no-op if the parameter is not found in the tiles array
    /// </summary>
    /// <param name="clickedTile">The tile that was clicked on</param>
    public void PlaceSelected(int x, int y)
    {
        if (TabIndex == 0) // tile edit mode
        {
            if (SelectedTile != null)
            {
                LayerTiles[y * LayerWidth + x] = SelectedTile.Clone();
            }
        }
        else if (TabIndex == 1) // game object mode
        {
            if (SelectedGameObject == null)
                return;

            if (SelectedGameObject is StartGameObject)
            {
                // remove any existing start objects as only one can be present on a level
                var existingStart = LayerTiles.Where(t => t.GameObject?.Type == "start").FirstOrDefault();
                if (existingStart != null)
                    existingStart.GameObject = null;
            }
            else if (SelectedGameObject is EndGameObject) // TODO: make multiple end objects possible (needs game engine support)
            {
                var existingEnd = LayerTiles.Where(t => t.GameObject?.Type == "end").FirstOrDefault();
                if (existingEnd != null)
                    existingEnd.GameObject = null;
            }

            var clickedTile = GetTile(x, y);
            if (clickedTile != null)
                clickedTile.GameObject = SelectedGameObject.Clone();
        }
    }

    [RelayCommand]
    private async Task OnSave()
    {
        var tiles = LayerTiles.Select((t, i) =>
        {
            if (t.TexturePath == null)
                return null;

            var posX = i % LayerWidth;
            var posY = i / LayerWidth;
            var relativeTexturePath = Path.GetRelativePath(GamePath, t.TexturePath).Replace("\\", "/");

            return new TileEntity
            {
                Texture = relativeTexturePath,
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

        var gameObjects = LayerTiles.Select((t, i) =>
        {
            var gameObject = t.GameObject;
            if (gameObject == null)
                return null;

            var posX = i % LayerWidth;
            var posY = i / LayerWidth;

            // start is not a "GameObject" as the game expects it, but for the level editor it is convenient to handle it like one
            if (gameObject.Type == "start")
            {
                start.XPos = posX;
                start.YPos = posY;
                return null;
            }
            else if (gameObject.Type == "end")
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
            GameObjects = gameObjects!,
            Background = LevelProperties.BackgroundPath,
            Music = LevelProperties.MusicPath,
            NextLevel = LevelProperties.NextLevel,
            Layers = [layer], // TODO: multi layer support
            LevelEnd = levelEnd,
            Start = start
        };

        var levelName = LevelProperties.Name;

        await using var fileStream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), levelName));
        await JsonSerializer.SerializeAsync(fileStream, level, serializeOptions);
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

    [RelayCommand]
    private void PickBackground()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "Select level background",
            InitialDirectory = Path.Combine(GamePath, "textures")
        };

        if (dialog.ShowDialog() == true)
        {
            var relativePath = Path.GetRelativePath(GamePath, dialog.FileName).Replace("\\", "/");
            LevelProperties.BackgroundPath = relativePath;
        }
    }

    [RelayCommand]
    private void PickMusic()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "Select level music",
            InitialDirectory = Path.Combine(GamePath, "audio")
        };

        if (dialog.ShowDialog() == true)
        {
            var relativePath = Path.GetRelativePath(GamePath, dialog.FileName).Replace("\\", "/");
            LevelProperties.MusicPath = relativePath;
        }
    }

    [RelayCommand]
    private void PickNextLevel()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "Select next level",
            InitialDirectory = Path.Combine(GamePath, "levels")
        };

        if (dialog.ShowDialog() == true)
        {
            var relativePath = Path.GetRelativePath(GamePath, dialog.FileName).Replace("\\", "/");
            LevelProperties.NextLevel = relativePath;
        }
    }

    [RelayCommand]
    private void PickGamePath()
    {
        var dialog = new OpenFolderDialog()
        {
            Title = "Set game folder",
            InitialDirectory = GamePath,
        };

        if (dialog.ShowDialog() == true)
        {
            GamePath = dialog.FolderName;
        }
    }

    private void ResizeLayer(int resizedX, int resizedY)
    {
        var newLayer = new List<Tile>(resizedX * resizedY);
        for (int i = 0; i < resizedX * resizedY; i++)
        {
            newLayer.Add(new Tile());
        }

        for (int x = 0; x < resizedX; x++)
        {
            for (int y = 0; y < resizedY; y++)
            {
                if (x < LayerWidth && y < LayerHeight)
                {
                    var tile = GetTile(x, y);
                    newLayer[y * resizedX + x] = tile ?? new Tile();
                }
            }
        }

        LayerWidth = resizedX;
        LayerHeight = resizedY;
        LayerTiles.Clear();
        foreach (var t in newLayer)
        {
            LayerTiles.Add(t);
        }
    }

    [RelayCommand]
    private void OpenResizeLayerWindow()
    {
        var vm = new ResizeLayerWindowViewModel
        {
            Width = LayerWidth,
            Height = LayerHeight
        };

        var window = new ResizeLayerWindow()
        {
            DataContext = vm
        };
        vm.OnRequestClose += (_, _) => window.Close();
        window.ShowDialog();
    }

}
