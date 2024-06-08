using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TileEditor.Domain;

namespace TileEditor;

partial class LayerPropertiesWindowViewModel(Layer layer) : ObservableObject
{
    public event EventHandler? OnRequestClose;

    [ObservableProperty]
    private string _parallaxOffsetX = layer.ParallaxOffsetFactorX.ToString();

    [ObservableProperty]
    private string _parallaxOffsetY = layer.ParallaxOffsetFactorY.ToString();

    [ObservableProperty]
    private string _layerOffsetX = layer.LayerOffsetX.ToString();

    [ObservableProperty]
    private string _layerOffsetY = layer.LayerOffsetY.ToString();

    [RelayCommand]
    private void Ok()
    {
        layer.ParallaxOffsetFactorX = double.Parse(_parallaxOffsetX);
        layer.ParallaxOffsetFactorY = double.Parse(_parallaxOffsetY);
        layer.LayerOffsetX = double.Parse(_layerOffsetX);
        layer.LayerOffsetY = double.Parse(_layerOffsetY);

        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        OnRequestClose?.Invoke(this, EventArgs.Empty);
    }
}
