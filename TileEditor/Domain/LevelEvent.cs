using CommunityToolkit.Mvvm.ComponentModel;

namespace TileEditor.Domain;

public partial class LevelEvent : ObservableObject
{
    [ObservableProperty]
    public string _type = "";

    [ObservableProperty]
    public Dictionary<string, object> _props = [];
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

        /*
           { "eventLayerId", 2 },
           { "eventLayerStopPosition", 6 },
           { "cameraStopPosition", 4 },
           { "eventLayerSpeed", 0.0022 },
           { "cameraSpeed", 0.002 }
         */
    }
}
