using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;


namespace TileEditor;

// TODO: canvas based tile rendering instead of UniformGrid
// TODO: tile placement preview
// TODO: select multiple tiles
// TODO: place the selected tiles at once (batch copy)
// TODO: batch tile placement preview
// TODO: zoom to mouse position
/// <summary>
/// Interaction logic for LayerEditor.xaml
/// </summary>
public partial class LayerEditor : UserControl
{
    private static readonly double _minZoom = 0.25;

    private bool _isScrolling = false;
    private Point _scrollStartPosition;
    private double _zoom = 1.0;

    private Vector? _lastTilePosition = null;
    private Vector? _layerMovePosition = null;

    public LayerEditor()
    {
        InitializeComponent();
    }

    private void ScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right && e.RightButton == MouseButtonState.Pressed)
        {
            var scrollView = sender as ScrollViewer;
            _scrollStartPosition = e.GetPosition(scrollView);
            _isScrolling = true;
            scrollView!.CaptureMouse();
        }
    }

    private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Right && e.RightButton != MouseButtonState.Pressed)
        {
            var scrollView = sender as ScrollViewer;
            _isScrolling = false;
            scrollView!.ReleaseMouseCapture();
        }
    }

    private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isScrolling)
        {
            var scrollView = sender as ScrollViewer;
            var currentMousePosition = e.GetPosition(scrollView);
            var offset = currentMousePosition - _scrollStartPosition;

            scrollView!.ScrollToVerticalOffset(scrollView.VerticalOffset - offset.Y);
            scrollView!.ScrollToHorizontalOffset(scrollView.HorizontalOffset - offset.X);

            _scrollStartPosition = e.GetPosition(scrollView);
        }
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // TODO: Zoom to mouse position
        if (e.Delta > 0)
        {
            _zoom += 0.1;
        }
        else
        {
            if (_zoom > _minZoom)
                _zoom -= 0.1;
        }

        var scrollView = sender as ScrollViewer;
        var panel = scrollView!.Content as FrameworkElement;
        var scale = new ScaleTransform(_zoom, _zoom);

        panel!.LayoutTransform = scale;
        e.Handled = true;
    }

    private void UniformGrid_MouseMove(object sender, MouseEventArgs e)
    {
        var vm = (DataContext as MainWindowViewModel)!;
        var grid = sender as UniformGrid;
        var mousePosition = e.GetPosition(grid);
        var gridIdX = (int)mousePosition.X / 64;
        var gridIdY = (int)mousePosition.Y / 64;
        var currentMousePosition = new Vector(gridIdX, gridIdY);
        vm.CurrentMousePosition = currentMousePosition;

        if (e.LeftButton == MouseButtonState.Pressed && _layerMovePosition.HasValue)
        {
            var dx = gridIdX - (int)_layerMovePosition.Value.X;
            var dy = gridIdY - (int)_layerMovePosition.Value.Y;

            if (dx != 0 || dy != 0)
            {
                dx = Math.Clamp(dx, -1, 1);
                dy = Math.Clamp(dy, -1, 1);
                _layerMovePosition = currentMousePosition;
                // TODO: command + undo command for move tile operation
                vm.MoveTiles(vm.SelectedLayer!, dx, dy);
            }
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!_lastTilePosition.HasValue || currentMousePosition != _lastTilePosition.Value)
            {
                _lastTilePosition = currentMousePosition;
                vm!.PlaceSelected(gridIdX, gridIdY);
            }
            return;
        }
    }

    private void UniformGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (DataContext as MainWindowViewModel)!;
        var grid = sender as UniformGrid;
        var mousePosition = e.GetPosition(grid);
        var gridIdX = (int)mousePosition.X / 64;
        var gridIdY = (int)mousePosition.Y / 64;

        // Handle move tool
        if (vm.Tool == DrawTool.MOVE && e.ChangedButton == MouseButton.Left)
        {
            _layerMovePosition = new Vector(gridIdX, gridIdY);
            Mouse.Capture(grid);
            return;
        }

        // Handle single click "paint"
        if (e.ChangedButton == MouseButton.Left)
        {
            vm.PlaceSelected(gridIdX, gridIdY);
            return;
        }
    }

    private void UniformGrid_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _layerMovePosition = null;
        _lastTilePosition = null;
        Mouse.Capture(null);
    }
}
