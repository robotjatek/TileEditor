using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.ComponentModel;

using TileEditor.Domain;
using TileEditor.EventEditorWindow;

namespace TileEditor;

public partial class EntityEditorWindowViewModel : ObservableObject, IDataErrorInfo
{
    [ObservableProperty]
    private GameObject? _gameObject;

    [ObservableProperty]
    private IEnumerable<string>? _objectNames;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrors))]
    private string? _name;

    public ObservableCollection<string> ExistingStrings { get; private set; }

    [ObservableProperty]
    private ObservableCollection<KvPair> _ps = [];

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
                    && !GameObject!.Label.Equals(Name))
                    return "Name must be unique";
            }
            return string.Empty;
        }
    }

    public bool HasErrors => !string.IsNullOrEmpty(this[nameof(Name)]);

    public event EventHandler? OnRequestClose;

    public EntityEditorWindowViewModel(GameObject gameObject, IEnumerable<string> objectNames)
    {
        this.GameObject = gameObject;
        this.ObjectNames = objectNames;
        this.Name = gameObject.Label;
        this.ExistingStrings = new ObservableCollection<string>(objectNames);

        foreach (var item in gameObject.Props)
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
        GameObject!.Name = _name;

        var d = new Dictionary<string, object>();
        foreach (var item in _ps)
        {
            d.Add(item.Key, item.Value);
        }
        GameObject.Props = d;

        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void CancelClicked()
    {
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
