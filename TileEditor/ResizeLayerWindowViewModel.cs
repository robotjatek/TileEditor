using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using System.Globalization;
using System.Windows.Controls;

namespace TileEditor;

// 32 & 18 are based on the Environment.ts in the engine.
// This is not a hard constraint of the engine, but the screen is in these dimensions. So it makes sense to have a default value like this in the editor
class LayerWidthValidationRule : IntValidator
{
    private const int MIN_VALUE = 32;

    public LayerWidthValidationRule() : base(MIN_VALUE, int.MaxValue, $"Width must be an integer and greater than {MIN_VALUE}") { }
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return base.Validate(value, cultureInfo);
    }
}

class LayerHeightValidationRule : IntValidator
{
    private const int MIN_VALUE = 18;

    public LayerHeightValidationRule() : base(MIN_VALUE, int.MaxValue, $"Height must be an integer and greater than {MIN_VALUE}") { }
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return base.Validate(value, cultureInfo);
    }
}

public partial class ResizeLayerWindowViewModel : ObservableObject
{
    public event EventHandler? OnRequestClose;

    [ObservableProperty]
    private int _width = 32;

    [ObservableProperty]
    private int _height = 18;

    [RelayCommand]
    private void Execute()
    {
        WeakReferenceMessenger.Default.Send(new LayerResizeMessage(new LayerSizeProperties
        {
            Height = _height,
            Width = _width
        }));

        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    public class LayerSizeProperties
    {
        public int Width { get; init; }
        public int Height { get; init; }
    }

    public class LayerResizeMessage(LayerSizeProperties value)
        : ValueChangedMessage<LayerSizeProperties>(value)
    { }

}
