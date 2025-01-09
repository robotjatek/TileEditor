using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.ComponentModel;

using TileEditor.Domain;

namespace TileEditor;

public partial class EntityEditorWindowViewModel(GameObject gameObject, IEnumerable<string> objectNames) : ObservableObject, IDataErrorInfo
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrors))]
    private string? _name = gameObject.Name;

    public ObservableCollection<string> ExistingStrings { get; } = new(objectNames);

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            if (columnName == nameof(Name))
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return "Name cannot be empty";
                if (ExistingStrings.Contains(Name)
                    // Ignore the validation if the name is the same as the original name
                    && !gameObject.Label.Equals(Name))
                    return "Name must be unique";
            }
            return string.Empty;
        }
    }

    public bool HasErrors => !string.IsNullOrEmpty(this[nameof(Name)]);

    public event EventHandler? OnRequestClose;

    [RelayCommand]
    private void OkClicked()
    {
        gameObject.Name = _name;

        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void CancelClicked()
    {
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
