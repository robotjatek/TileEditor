using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TileEditor;

partial class LayerPropertiesWindowViewModel(Layer layer) : ObservableObject
{
    public event EventHandler? OnRequestClose;

    [ObservableProperty]
    private string _parallaxOffsetX = layer.ParallaxOffsetFactorX.ToString();

    [ObservableProperty]
    private string _parallaxOffsetY = layer.ParallaxOffsetFactorY.ToString();

    [RelayCommand]
    private void Ok()
    {

        layer.ParallaxOffsetFactorX = double.Parse(_parallaxOffsetX);
        layer.ParallaxOffsetFactorY = double.Parse(_parallaxOffsetY);

        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
