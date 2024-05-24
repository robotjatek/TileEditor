using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TileEditor;

/// <summary>
/// Interaction logic for LayerEditor.xaml
/// </summary>
public partial class LayerEditor : UserControl
{
    private bool _isScrolling = false;
    private Point _scrollStartPosition;
    private double _zoom = 1.0;
    private static readonly double _minZoom = 0.5;

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
        var panel = scrollView!.Content as StackPanel;
        var scale = new ScaleTransform(_zoom, _zoom);

        panel!.LayoutTransform = scale;
        e.Handled = true;
    }
}
