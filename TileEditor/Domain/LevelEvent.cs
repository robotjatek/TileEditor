using CommunityToolkit.Mvvm.ComponentModel;

namespace TileEditor.Domain;

public partial class LevelEvent : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public string _type = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public Dictionary<string, object> _props = [];

    public string Label
    {
        get => string.IsNullOrEmpty(_props["id"].ToString()) ? _type ?? string.Empty : _props["id"].ToString()!;
    }
}

public partial class EscapeEvent : LevelEvent
{
    [ObservableProperty]
    private int _eventLayerId = 2;

    [ObservableProperty]
    private double _eventLayerStopPosition = 6.0;

    [ObservableProperty]
    private double _cameraStopPosition = 4.0;

    [ObservableProperty]
    private double _eventLayerSpeed = 0.0022;

    [ObservableProperty]
    private double _cameraSpeed = 0.002;

    public Dictionary<string, object> RefreshProps()
    {
        Props["eventLayerId"] = EventLayerId;
        Props["eventLayerStopPosition"] = EventLayerStopPosition;
        Props["cameraStopPosition"] = CameraStopPosition;
        Props["eventLayerSpeed"] = EventLayerSpeed;
        Props["cameraSpeed"] = CameraSpeed;

        return Props;
    }

    public EscapeEvent(int eventLayerId, double eventLayerStopPosition, double cameraStopPosition, double eventLayerSpeed, double cameraSpeed)
    {
        Type = "escape_event";
        EventLayerId = eventLayerId;
        EventLayerStopPosition = eventLayerStopPosition;
        CameraStopPosition = cameraStopPosition;
        EventLayerSpeed = eventLayerSpeed;
        CameraSpeed = cameraSpeed;
        
        RefreshProps();
    }

}

public partial class OutroEvent : LevelEvent
{
    public OutroEvent()
    {
        Type = "outro_event";
    }
}

// TODO: ha egy olyan pályát töltök be amiből hiányzik egy mező, pl a health, akkor a props windowban nem jelenik meg a field
public partial class BossEvent : LevelEvent
{

    [ObservableProperty]
    private int _enterWaypointX = 0;

    [ObservableProperty]
    private int _enterWaypointY = 0;

    [ObservableProperty]
    private int _spawnX = 0;

    [ObservableProperty]
    private int _spawnY = 0;

    [ObservableProperty]
    private int _health = 10;


    public BossEvent(int enterX, int enterY, int spawnX, int spawnY, int health) 
    {
        Type = "boss_event";
        EnterWaypointX = enterX;
        EnterWaypointY = enterY;
        SpawnX = spawnX;
        SpawnY = spawnY;
        Health = health;

        Props["enterWaypointX"] = EnterWaypointX;
        Props["enterWaypointY"] = EnterWaypointY;
        Props["spawnX"] = SpawnX;
        Props["spawnY"] = SpawnY;
        Props["health"] = Health;
    }
}

public partial class GateEvent : LevelEvent
{
    [ObservableProperty]
    private string _id = string.Empty;

    public GateEvent(string id)
    {
        Type = "gate_event";
        Id = id;

        Props["id"] = Id;
        // TODO: propsba portcullis positions, enemy positions
    }
}
