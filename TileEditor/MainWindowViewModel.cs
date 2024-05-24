using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

using TileEditor.DTOs;

namespace TileEditor;

// TODO: RelayCommand attribute should substitute this
public class CommandImpl : ICommand
{
    private readonly Action<Tile> _execute;

    public event EventHandler? CanExecuteChanged;

    public CommandImpl(Action<Tile> execute)
    {
        _execute = execute;
    }

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        _execute((parameter as Tile)!);
    }
}


public class LevelProperties
{

}

// TODO: menu for custom level properties: bg, music, name, next level, start position, end position(s)
// TODO: layer properties
// TODO: multi layer support
// TODO: layer domain object and layer DTO
public partial class MainWindowViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions serializeOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    // TODO: configurable bigger numbers
    [ObservableProperty]
    private int _layerWidth = 32; // TODO: minimum width based on Environment.ts

    [ObservableProperty]
    private int _layerHeight = 18; // TODO: minimum height based on Environment.ts

    [ObservableProperty]
    private Tile? _selectedTile;

    [ObservableProperty]
    private BindingList<Tile> _layerTiles = []; // TODO:  multi layer support

    [ObservableProperty]
    private ICommand? _onClick;

    [ObservableProperty]
    private ICommand? _onSave;

    [ObservableProperty]
    private ICommand? _notImplemented;

    public MainWindowViewModel()
    {
        _onClick = new CommandImpl(OnClickMethod);
        _onSave = new CommandImpl(_ => OnSaveMethod());
        _notImplemented = new CommandImpl(_ => MessageBox.Show("Not implemented", "Err", MessageBoxButton.OK));

        for (int i = 0; i < _layerWidth * _layerHeight; i++)
        {
            _layerTiles.Add(new EmptyTile());
            //_layerTiles.Add(new Tile
            //{
            //    TexturePath = "D:\\Dev\\webgl-engine\\textures\\ground0.png"
            //});
        }

        WeakReferenceMessenger.Default.Register<SelectedChangeMessage>(this, (r, m) =>
        {
            ReceiveSelectedTile(m);
        });
    }

    public void ReceiveSelectedTile(ValueChangedMessage<Tile> message)
    {
        SelectedTile = message.Value;
    }

    private void OnClickMethod(Tile o)
    {
        if (SelectedTile != null)
        {
            var tileIndexInArray = _layerTiles.IndexOf(o);
            _layerTiles[tileIndexInArray] = SelectedTile!.Clone();
        }
    }

    private void OnSaveMethod()
    {
        var tiles = _layerTiles.Select((t, i) =>
        {
            if (t is EmptyTile)
                return null;

            var posX = i % _layerWidth;
            var posY = i / _layerWidth;
            var ingamePath = Path.Combine("textures", Path.GetFileName(t.TexturePath)!); // TODO: game relative path

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

        var levelEnd = new LevelEndEntity
        {
            XPos = 5,
            YPos = 5
        };

        var start = new StartEntity
        {
            XPos = 0,
            YPos = 0
        };

        var level = new LevelEntity
        {
            Background = "textures/bg.jpg", // TODO: selectable in editor
            GameObjects = [],
            Music = "audio/level.mp3", // TODO: selectable in editor
            Layers = [layer], // TODO: multi layer support
            LevelEnd = levelEnd, // TODO: place game objects
            Start = start, // TODO: place game objects
            NextLevel = "levels/level2.json" // TODO: make it configurable
        };

        var levelName = "level1.json"; // TODO: custom level name
        var levelJson = JsonSerializer.Serialize(level, serializeOptions); // TODO: async serialize
        File.WriteAllText(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), levelName),
            levelJson); // TODO: async save
    }
}
