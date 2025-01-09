using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TileEditor.Domain;

namespace TileEditor.EntityEditorWindow;

public partial class EntityEditorWindowViewModel(GameObject gameObject) : ObservableObject
{

    [ObservableProperty]
    private string? _name = gameObject.Name;

    public event EventHandler? OnRequestClose;

    [RelayCommand]
    private void OkClicked()
    {
        // TODO: validate?
        // TODO: name must be unique validation
        // TODO: validator for the input field
        gameObject.Name = _name;

        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void CancelClicked()
    {
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
