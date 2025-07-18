﻿using CommunityToolkit.Mvvm.ComponentModel;
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
using TileEditor.EventEditorWindow;
using TileEditor.EventSelectorWindow;

using static TileEditor.ResizeLayerWindowViewModel;

namespace TileEditor;

public class DrawTool
{
    public required string Tooltip { get; init; }

    public required string Content { get; init; }

    public static readonly DrawTool SINGLE = new()
    {
        Content = "\ue20d",
        Tooltip = "Single placement"
    };

    public static readonly DrawTool ROW = new()
    {
        Content = "\uE147",
        Tooltip = "Row placement"
    };

    public static readonly DrawTool COLUMN = new()
    {
        Content = "\uE145",
        Tooltip = "Column placement"
    };

    public static readonly DrawTool SQUARE = new()
    {
        Content = "\uE138",
        Tooltip = "Square placement"
    };

    public static readonly DrawTool MOVE = new()
    {
        Content = "\ue20e",
        Tooltip = "Move layer"
    };
}

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

    [ObservableProperty]
    private string? _initialEventKey = "free_camera_event";
}

public partial class MainWindowViewModel : ObservableObject
{
    // TODO: except for free cam others should be based on the the events added to the level
    [ObservableProperty]
    private string[] _initialEventKeys = ["free_camera_event", "outro_event"];

    [ObservableProperty]
    private ObservableCollection<LevelEvent> _levelEvents = [];

    [ObservableProperty]
    private LevelEvent? _selectedLevelEvent;

    [RelayCommand]
    private void RemoveEvent(LevelEvent @event)
    {
        var result = new EditorMessageBox("Are you sure?", "Removing event", Buttons.OK_CANCEL).ShowDialog();
        if (result == CustomDialogResult.OK)
        {
            _levelEvents.Remove(@event);
        }
    }

    [RelayCommand]
    private void RemoveGameObject(GameObject gameObject)
    {
        if (DefaultLayer == null)
            return;

        DefaultLayer.Tiles.First(t => t.GameObject == gameObject).GameObject = null;
        OnPropertyChanged(nameof(GameObjects));
    }

    [RelayCommand]
    private void OpenEventEditorWindow(LevelEvent @event)
    {
        var vm = new EventEditorViewModel(@event);
        var wnd = new EventEditorWindow.EventEditorWindow(vm);
        vm.OnRequestClose += (_, _) => wnd.Close();
        wnd.Show();
    }

    [RelayCommand]
    private void OpenEventSelectorWindow(LevelEvent @event)
    {
        var vm = new EventSelectorWindowViewModel();
        var wnd = new EventSelectorWindow.EventSelectorWindow(vm);
        vm.OnRequestClose += (_, _) => wnd.Close();
        wnd.Show();
    }

    [RelayCommand]
    private void OpenEntityEditorWindow(GameObject gameObject)
    {
        var vm = new EntityEditorWindowViewModel(gameObject, GameObjects.Select(x => x.Name).Where(x => x is not null)!);
        var wnd = new EntityEditorWindow(vm);
        vm.OnRequestClose += (_, _) => wnd.Close();
        wnd.Show();
    }

    private static readonly JsonSerializerOptions serializeOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [ObservableProperty]
    private DrawTool[] _tools =
    [
        DrawTool.SINGLE,
        DrawTool.ROW,
        DrawTool.COLUMN,
        DrawTool.SQUARE,
        DrawTool.MOVE
    ];

    [ObservableProperty]
    private DrawTool _tool = DrawTool.SINGLE;

    [ObservableProperty]
    private string _windowTitle = "TileEditor";

    [ObservableProperty]
    private string _gamePath = "D:\\Dev\\webgl-engine";

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
    [NotifyPropertyChangedFor(nameof(GameObjects))]
    private Layer? _selectedLayer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GameObjects))]
    private Layer? _defaultLayer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private LevelProperties? _levelProperties;

    [ObservableProperty]
    private Vector _currentMousePosition = new(0, 0);

    public bool CanSave
    {
        get => LevelProperties is not null;
    }

    public List<GameObject> GameObjects
    {
        get
        {
            if (DefaultLayer == null)
                return [];

            return DefaultLayer.Tiles.Select(t => t.GameObject).Where(o => o is not null).ToList()!;
        }
    }

    public MainWindowViewModel()
    {
        SetDefaults();
        WeakReferenceMessenger.Default.Register<TileSelectedChangeMessage>(this, (r, m) => ReceiveSelectedTile(m));
        WeakReferenceMessenger.Default.Register<EntitySelectedChange>(this, (r, m) => ReceiveSelectedGameObject(m));
        WeakReferenceMessenger.Default.Register<LayerResizeMessage>(this, (r, m) => ReceiveLayerResizeMessage(m));
        WeakReferenceMessenger.Default.Register<EventSelectedMessage>(this, (r, m) => ReceiveSelectedEvent(m));
    }

    private void ReceiveSelectedEvent(ValueChangedMessage<LevelEvent> message)
    {
        if (!LevelEvents.Any(e => e.Type == message.Value.Type))
            LevelEvents.Add(message.Value);
    }

    private void ReceiveSelectedTile(ValueChangedMessage<Tile> message)
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
        // Early return when there is no selected layer
        // Only the default layer can have game objects
        if (SelectedLayer == null || (TabIndex == 1 && SelectedLayer != DefaultLayer))
            return;

        if (TabIndex == 0) // tile edit mode
        {
            if (SelectedTile != null)
            {
                if (Tool == DrawTool.SINGLE)
                {
                    SelectedLayer.Tiles[y * LayerWidth + x] = SelectedTile.Clone();
                }
                else if (Tool == DrawTool.ROW)
                {
                    for (int i = 0; i < LayerWidth; i++)
                    {
                        SelectedLayer.Tiles[y * LayerWidth + i] = SelectedTile.Clone();
                    }
                }
                else if (Tool == DrawTool.COLUMN)
                {
                    for (int i = 0; i < LayerHeight; i++)
                    {
                        SelectedLayer.Tiles[i * LayerWidth + x] = SelectedTile.Clone();
                    }
                }
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

            var clickedTile = SelectedLayer.GetTile(x, y);
            if (clickedTile != null)
            {
                var toAdd = SelectedGameObject.Clone();
                toAdd.Name = GenerateUniqueName(toAdd.Type!);
                clickedTile.GameObject = toAdd;
            }
        }

        OnPropertyChanged(nameof(GameObjects));
    }

    private string GenerateUniqueName(string type)
    {
        int count = GameObjects.Count;
        int uniqueId = count;

        var existingNames = GameObjects.Select(x => x.Name).ToHashSet();
        while (existingNames.Contains($"{type}-{uniqueId}"))
        {
            uniqueId++;
        }

        return $"{type}-{uniqueId}";
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
                Name = item.Name,
                Tiles = tiles!,
                ParallaxOffsetFactorX = item.ParallaxOffsetFactorX,
                ParallaxOffsetFactorY = item.ParallaxOffsetFactorY,
                LayerOffsetX = item.LayerOffsetX,
                LayerOffsetY = item.LayerOffsetY,
            });
        }

        StartEntity? start = null;

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
                start = new StartEntity
                {
                    XPos = posX,
                    YPos = posY,
                };
                return null;
            }

            return new GameObjectEntity
            {
                // TODO: props
                Type = gameObject.Type!,
                XPos = posX,
                YPos = posY,
                Name = gameObject.Label
            };
        }).Where(g => g != null).ToArray();

        if (start is null)
        {
            var result = new EditorMessageBox("Level start is not set. Levels like this can be saved, but the game wont work with incomplete levels. Do you want to proceed?",
                "Warning", Buttons.OK_CANCEL).ShowDialog();
            if (result != CustomDialogResult.OK)
                return;
        }

        var nonDefaultLayerHasGameObjects = Layers.Where(l => l != DefaultLayer).SelectMany(l => l.Tiles).Any(t => t.GameObject != null);
        if (nonDefaultLayerHasGameObjects)
        {
            var result = new EditorMessageBox("Layers other than the default have entities defined. Those will be lost on export. Do you want to proceed?",
                "Warning", Buttons.OK_CANCEL).ShowDialog();
            if (result != CustomDialogResult.OK)
                return;
        }

        var events = LevelEvents.Select(e => new EventEntity
        {
            Type = e.Type,
            Props = e.Props
        });

        var level = new LevelEntity
        {
            GameObjects = gameObjects!,
            Background = LevelProperties!.BackgroundPath, // Save is disabled when level props is null so no null deference here
            Music = LevelProperties.MusicPath,
            NextLevel = LevelProperties.NextLevel,
            Layers = [.. layers],
            Start = start,
            DefaultLayer = Layers.IndexOf(DefaultLayer),
            Events = [.. events],
            InitialEventKey = LevelProperties.InitialEventKey
        };

        var levelName = LevelProperties.Name;
        await using var fileStream = File.Create(Path.Combine(GamePath, "levels", levelName));
        await JsonSerializer.SerializeAsync(fileStream, level, serializeOptions);
    }

    [RelayCommand]
    private void OnEdit()
    {
        LevelProperties ??= new LevelProperties();

        var _editLevelPropertiesWindow = new EditLevelPropertiesWindow();
        _editLevelPropertiesWindow.ShowDialog();
        WindowTitle = $"TileEditor - {LevelProperties.Name}";
    }

    [RelayCommand]
    private static void NotImplemented()
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
            LevelProperties!.BackgroundPath = relativePath; // Level props is initialized when the edit window is opened so no null dereference here
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
            LevelProperties!.MusicPath = relativePath; // Level props is initialized when the edit window is opened so no null dereference here
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
            LevelProperties!.NextLevel = relativePath; // Level props is initialized when the edit window is opened so no null dereference here
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

    public void MoveTiles(Layer layer, int dirX, int dirY)
    {
        var w = layer.Width;
        var h = layer.Height;
        var newTiles = new Tile[w * h];

        // Only move the layer if the tiles do not move out of bounds
        if (dirY < 0 && !IsRowEmpty(layer, 0)) return;
        if (dirY > 0 && !IsRowEmpty(layer, h - 1)) return;
        if (dirX < 0 && !IsColumnEmpty(layer, 0)) return;
        if (dirX > 0 && !IsColumnEmpty(layer, w - 1)) return;

        for (var x = 0; x < layer.Width; x++)
        {
            for (var y = 0; y < layer.Height; y++)
            {
                var srcX = x - dirX;
                var srcY = y - dirY;
                var currentTile = layer.GetTile(srcX, srcY)!;
                if (srcY >= 0 && srcY < h && srcX >= 0 && srcX < w)
                    newTiles[y * w + x] = currentTile;
                else
                    newTiles[y * w + x] = new Tile();

            }
        }

        layer.Tiles = new ObservableCollection<Tile>(newTiles);
        // TODO: ezt itthagyom még kicsit mert nem tudom hogy milyen mellékhatása lehet máshol hogy ha a layer.Tilest felülírom
        //layer.Tiles.Clear();
        //foreach (var tile in newTiles)
        //{
        //    layer.Tiles.Add(tile);
        //}
    }

    private bool IsRowEmpty(Layer layer, int row)
    {
        for (var i = 0; i < layer.Width; i++)
        {
            if (!layer.GetTile(i, row)!.IsEmpty)
                return false;
        }
        return true;
    }

    private bool IsColumnEmpty(Layer layer, int column)
    {
        for (var i = 0; i < layer.Height; i++)
        {
            if (!layer.GetTile(column, i)!.IsEmpty)
                return false;
        }
        return true;
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
        var layer = new Layer
        {
            Width = LayerWidth,
            Height = LayerHeight
        };
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

    [RelayCommand]
    private void MoveLayerUp(Layer layer)
    {
        var layerId = Layers.IndexOf(layer);
        if (layerId - 1 >= 0)
        {
            Layer t = Layers[layerId - 1];
            Layers[layerId - 1] = layer;
            Layers[layerId] = t;
        }

        SelectedLayer = layer;
    }

    [RelayCommand]
    private void MoveLayerDown(Layer layer)
    {
        var layerId = Layers.IndexOf(layer);
        if (layerId + 1 < Layers.Count)
        {
            Layer t = Layers[layerId + 1];
            Layers[layerId + 1] = layer;
            Layers[layerId] = t;
        }

        SelectedLayer = layer;
    }

    [RelayCommand]
    private void NewLevel()
    {
        SetDefaults();
        var props = new LevelProperties();
        LevelProperties = props;
        new EditLevelPropertiesWindow().ShowDialog();
        AddLayer();
        WindowTitle = $"TileEditor - {LevelProperties.Name}";
    }

    [RelayCommand]
    private async Task OpenLevel()
    {
        CurrentMousePosition = new Vector(0, 0);
        ImageCache.Clear();
        var dialog = new OpenFileDialog()
        {
            Filter = "JSON|*.json",
            InitialDirectory = Path.Combine(GamePath, "levels"),
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var tileFilenames = new HashSet<string>();

                var filename = dialog.FileName; // Full path to file
                var json = File.OpenRead(filename);
                var levelEntity = (await JsonSerializer.DeserializeAsync<LevelEntity>(json, serializeOptions))!;

                var gamePath = Directory.GetParent(Path.GetDirectoryName(filename)!)!.ToString().Replace("\\", "/");
                var props = new LevelProperties
                {
                    Name = Path.GetFileName(filename),
                    BackgroundPath = levelEntity.Background,
                    MusicPath = levelEntity.Music,
                    NextLevel = levelEntity.NextLevel,
                    InitialEventKey = levelEntity.InitialEventKey
                };
                LevelProperties = props;

                var layerTiles = levelEntity.Layers.SelectMany(l => l.Tiles).ToArray();
                var layerWidth = layerTiles.Length > 0 ? layerTiles.Select(t => t.XPos).Max() + 1 : 0;
                var layerHeight = layerTiles.Length > 0 ? layerTiles.Select(t => t.YPos).Max() + 1 : 0;

                var layers = levelEntity.Layers.Select((layerEntity, i) =>
                {
                    var tiles = new List<Tile>(layerWidth * layerHeight);
                    for (int index = 0; index < layerWidth * layerHeight; index++)
                    {
                        tiles.Add(new Tile()); // Fill layer with empty tiles
                    }

                    foreach (var item in layerEntity.Tiles)
                    {
                        var tileIdx = item.YPos * layerWidth + item.XPos;
                        var texturePath = Path.Combine(gamePath, item.Texture);
                        tileFilenames.Add(texturePath);

                        var tile = new Tile
                        {
                            TexturePath = texturePath
                        };
                        tiles[tileIdx] = tile;
                    }

                    var currentLayer = new Layer
                    {
                        Name = layerEntity.Name,
                        IsDefault = i == levelEntity.DefaultLayer,
                        Tiles = [.. tiles],
                        Height = layerHeight,
                        Width = layerWidth,
                        ParallaxOffsetFactorX = layerEntity.ParallaxOffsetFactorX,
                        ParallaxOffsetFactorY = layerEntity.ParallaxOffsetFactorY,
                        LayerOffsetX = layerEntity.LayerOffsetX,
                        LayerOffsetY = layerEntity.LayerOffsetY
                    };

                    return currentLayer;
                });

                Layers.Clear();
                foreach (var layer in layers)
                {
                    Layers.Add(layer);
                }

                var defaultLayerWidth = Layers[levelEntity.DefaultLayer].Width;
                if (levelEntity.Start != null)
                {
                    var startIndex = levelEntity.Start.YPos * defaultLayerWidth + levelEntity.Start.XPos;
                    Layers[levelEntity.DefaultLayer].Tiles[startIndex].GameObject = new StartGameObject();
                }

                // Attach game objects to tiles
                foreach (var go in levelEntity.GameObjects)
                {
                    var index = go.YPos * defaultLayerWidth + go.XPos;
                    Layers[levelEntity.DefaultLayer].Tiles[index].GameObject = new GameObject()
                    {
                        Type = go.Type,
                        Name = go.Name
                        // TODO: read props from entity
                    };
                }

                LayerWidth = Layers.Select(l => l.Width).Max();
                LayerHeight = Layers.Select(l => l.Height).Max();

                SelectedLayer = Layers[levelEntity.DefaultLayer];
                DefaultLayer = Layers[levelEntity.DefaultLayer];

                var events = levelEntity.Events.Select(e => new LevelEvent
                {
                    Type = e.Type,
                    Props = e.Props
                });
                LevelEvents = [.. events];

                WindowTitle = $"TileEditor - {LevelProperties.Name}";

                WeakReferenceMessenger.Default.Send(new LevelLoadedMessage(tileFilenames));
            }
            catch (Exception ex)
            {
                new EditorMessageBox(ex.Message, "Error", Buttons.OK).ShowDialog();
                SetDefaults();
            }
        }

    }

    [RelayCommand]
    private static void EditLayerProperties(Layer layer)
    {
        var wm = new LayerPropertiesWindowViewModel(layer);
        var window = new LayerPropertiesWindow
        {
            DataContext = wm
        };
        wm.OnRequestClose += (_, _) => window.Close();
        window.ShowDialog();
    }

    private void SetDefaults()
    {
        WindowTitle = "TileEditor";
        GamePath = "D:\\Dev\\webgl-engine";
        TabIndex = 0;
        LayerWidth = 32;
        LayerHeight = 18;
        SelectedTile = null;
        SelectedLayer = null;
        SelectedGameObject = null;
        Layers = [];
        LevelProperties = null;
        LevelEvents = [];
        WeakReferenceMessenger.Default.Send(new LevelLoadedMessage([]));
    }

    public class LevelLoadedMessage(HashSet<string> value) : ValueChangedMessage<HashSet<string>>(value) { }
}
