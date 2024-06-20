using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using TileEditor.Domain;

namespace TileEditor.EventSelectorWindow;

public class EventSelectedMessage(LevelEvent value) : ValueChangedMessage<LevelEvent>(value) { }

public partial class EventSelectorWindowViewModel : ObservableObject
{
    private static readonly LevelEvent[] events = [new EscapeEvent(2, 6, 4, 0.0022, 0.002)];

    public event EventHandler? OnRequestClose;

    [ObservableProperty]
    private LevelEvent[] _availableEvents = [.. events];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAccept))]
    private LevelEvent? _selectedEvent;

    public bool CanAccept => _selectedEvent != null;

    [RelayCommand]
    private void OkClicked()
    {
        if (_selectedEvent is not null)
        {
            WeakReferenceMessenger.Default.Send(new EventSelectedMessage(SelectedEvent!));
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void CancelClicked()
    {
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
