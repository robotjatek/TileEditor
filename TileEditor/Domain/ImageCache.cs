using System.Collections.Concurrent;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TileEditor.Domain;

public static class ImageCache
{
    private static readonly ConcurrentDictionary<string, BitmapImage> _cache = [];

    public static ImageSource? GetImage(string? path)
    {
        if (path == null)
            return null;

        if (!_cache.TryGetValue(path, out BitmapImage? value))
        {
            var image = new BitmapImage(new Uri(path));
            value = image;
            _cache[path] = value;
        }
        return value;
    }
}
