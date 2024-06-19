using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;

using TileEditor.Domain;

namespace TileEditor.EventEditorWindow;

public partial class KvPair: ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
}

public partial class EventEditorViewModel : ObservableObject
{
    public event EventHandler? OnRequestClose;

    [ObservableProperty]
    private LevelEvent _toEdit;

    [ObservableProperty]
    private  ObservableCollection<KvPair> _ps = [];

    public EventEditorViewModel(LevelEvent @event)
    {
        _toEdit = @event;

        foreach(var item in @event.Props)
        {
            Ps.Add(new KvPair
            {
                Key = item.Key,
                Value = item.Value.ToString()!
            });
        }
    }

    [RelayCommand]
    private void OkClicked()
    {
        // TODO: validate fields before send
        var d = new Dictionary<string, object>();
        foreach (var item in _ps)
        {
            d.Add(item.Key, item.Value);
        }
        ToEdit.Props = d;

        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void CancelClicked()
    {
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
