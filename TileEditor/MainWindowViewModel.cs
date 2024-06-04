using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

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

    [ObservableProperty]
    private ObservableCollection<Layer> _layers = [];

    [ObservableProperty]
    private Layer? _selectedLayer;

    [ObservableProperty]
    private Layer? _defaultLayer;

    [ObservableProperty]
    private LevelProperties _levelProperties = new();

    public MainWindowViewModel()
    {
        // Add one default empty layer on init
        AddLayer();
        SelectedLayer = Layers[0];
        DefaultLayer = Layers[0];

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
            var result = new EditorMessageBox("The dimensions provided are smaller than the current size. This can result in losing tiles. Do you want to proceed?",
                "Warning", Buttons.OK_CANCEL).ShowDialog();

            if (result == CustomDialogResult.CANCEL)
                return;
        }

        ResizeLayers(message.Value.Width, message.Value.Height);
    }

    /// <summary>
    /// Places the selected tile or entity to the place of the incoming parameter. 
    /// In tile edit mode no-op if the parameter is not found in the tiles array
    /// </summary>
    /// <param name="clickedTile">The tile that was clicked on</param>
    public void PlaceSelected(int x, int y)
    {
        // Only the default layer can have game objects
        if (SelectedLayer == null || SelectedLayer != DefaultLayer)
            return;

        if (TabIndex == 0) // tile edit mode
        {
            if (SelectedTile != null)
            {
                SelectedLayer.Tiles[y * LayerWidth + x] = SelectedTile.Clone();
            }
        }
        else if (TabIndex == 1) // game object mode
        {
            if (SelectedGameObject == null)
                return;

            if (SelectedGameObject is StartGameObject)
            {
                // remove any existing start objects as only one can be present on a level
                var existingStart = SelectedLayer.Tiles.Where(t => t.GameObject?.Type == "start").FirstOrDefault();
                if (existingStart != null)
                    existingStart.GameObject = null;
            }
            else if (SelectedGameObject is EndGameObject) // TODO: make multiple end objects possible (needs game engine support)
            {
                var existingEnd = SelectedLayer.Tiles.Where(t => t.GameObject?.Type == "end").FirstOrDefault();
                if (existingEnd != null)
                    existingEnd.GameObject = null;
            }

            var clickedTile = SelectedLayer.GetTile(x, y);
            if (clickedTile != null)
                clickedTile.GameObject = SelectedGameObject.Clone();
        }
    }

    [RelayCommand]
    private async Task OnSave()
    {
        // Must have an existing default layer before save
        if (DefaultLayer == null)
        {
            new EditorMessageBox("Error: No default layer selected", "Error", Buttons.OK).ShowDialog();
            return;
        }

        var layers = new List<LayerEntity>();
        foreach (var item in Layers)
        {
            var tiles = item.Tiles.Select((t, i) =>
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

            layers.Add(new LayerEntity
            {
                Tiles = tiles!
            });
        }

        var start = new StartEntity();
        var levelEnd = new LevelEndEntity();

        var gameObjects = DefaultLayer.Tiles.Select((t, i) =>
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
            Layers = [.. layers],
            LevelEnd = levelEnd,
            Start = start
        };

        var levelName = LevelProperties.Name;
        await using var fileStream = File.Create(Path.Combine(GamePath, "levels", levelName));
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
        new EditorMessageBox("Not implemented", "Error", Buttons.OK).ShowDialog();
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

    private void ResizeLayers(int resizedX, int resizedY)
    {
        for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
        {
            // Create a new empty temp layer
            var newLayer = new List<Tile>(resizedX * resizedY);
            for (int i = 0; i < resizedX * resizedY; i++)
            {
                newLayer.Add(new Tile());
            }

            // Copy old data to the new temp layer
            for (int x = 0; x < resizedX; x++)
            {
                for (int y = 0; y < resizedY; y++)
                {
                    if (x < LayerWidth && y < LayerHeight)
                    {
                        var tile = Layers[layerIndex].GetTile(x, y);
                        newLayer[y * resizedX + x] = tile ?? new Tile();
                    }
                }
            }

            Layers[layerIndex].Tiles.Clear();
            Layers[layerIndex].Width = resizedX;
            Layers[layerIndex].Height = resizedY;

            foreach (var t in newLayer)
            {
                Layers[layerIndex].Tiles.Add(t);
            }
        }


        LayerWidth = resizedX;
        LayerHeight = resizedY;
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

    [RelayCommand]
    private void AddLayer()
    {
        var layer = new Layer();
        for (int i = 0; i < LayerWidth * LayerHeight; i++)
        {
            layer.Tiles.Add(new Tile());
        }
        Layers.Add(layer);
        if (Layers.Count == 1)
        {
            layer.IsDefault = true;
            DefaultLayer = layer;
        }
    }

    [RelayCommand]
    private void RemoveSelectedLayer()
    {
        var result = new EditorMessageBox("Remove layer?", "Warning", Buttons.OK_CANCEL).ShowDialog();
        if (result == CustomDialogResult.OK)
        {
            if (SelectedLayer == DefaultLayer)
                DefaultLayer = null;

            if (SelectedLayer != null)
                Layers.Remove(SelectedLayer);
        }
    }

    [RelayCommand]
    private void MakeSelectedLayerDefault()
    {
        if (SelectedLayer == null)
            return;

        if (SelectedLayer != null)
        {
            foreach (var layer in Layers)
            {
                layer.IsDefault = false;
            }
            DefaultLayer = SelectedLayer;
            DefaultLayer.IsDefault = true;
        }
    }
}
