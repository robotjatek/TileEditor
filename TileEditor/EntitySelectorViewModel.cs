using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;

using TileEditor.Domain;

namespace TileEditor;

public partial class EntitySelectorViewModel : ObservableObject
{
    [ObservableProperty]
    private GameObject? _selected;

    public ObservableCollection<GameObject> Data { get; set; } = new ObservableCollection<GameObject>([
        new GameObject
        {
            Type = "coin"
        },
        new GameObject
        {
            Type = "health"
        },
        new GameObject
        {
            Type = "spike"
        },
        new GameObject
        {
            Type = "cactus"
        },
        new GameObject
        {
            Type = "slime"
        },
        new GameObject
        {
            Type = "dragon"
        },
        new GameObject
        {
            Type = "escape_trigger"
        },
        new GameObject
        {
            Type = "boss_trigger"
        },
        new GameObject
        {
            Type = "lever",
            Props = new Dictionary<string, object>
            {
                { "eventId", string.Empty }
            }
        },

        new StartGameObject(),
        new EndGameObject(),
    ]);
}


public class StartGameObject : GameObject
{
    public StartGameObject()
    {
        Type = "start";
    }
}

public class EndGameObject : GameObject
{
    public EndGameObject()
    {
        Type = "end";
    }
}
