using GBX.NET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Transforms;


public class Pixel
{
    public SixLabors.ImageSharp.Color color;
    public Int2 coords;

    public Pixel(int x, int y, SixLabors.ImageSharp.Color color)
    {
        this.color = color;
        this.coords = new Int2(x, y);
    }
}

public class ImageInfo
{

    public string Preview { get; set; }
    public int BlockCount { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public List<Pixel> Pixels { get; set; }

    public static ImageInfo FromBytes(byte[] imageStream)
    {

        Image<Rgba32> image = Image.Load<Rgba32>(imageStream);

        List<Pixel> pixels = ImageHelper.Palettize(ImageHelper.GetPixels(image));

        ImageInfo imageInfo = new ImageInfo();

        imageInfo.Width = image.Width;
        imageInfo.Height = image.Height;
        imageInfo.Pixels = pixels;
        imageInfo.BlockCount = pixels.Count;
        imageInfo.Preview = ImageHelper.GeneratePreview(imageStream, pixels).ToBase64String(PngFormat.Instance);

        return imageInfo;
    }
}
public class ImageHelper
{
    public static List<Rgba32> colors = new List<Rgba32>{
        new Rgba32(18, 80, 150),
        new Rgba32(160, 160, 160),
        new Rgba32(100, 62, 37),
        new Rgba32(52, 101, 46),
        new Rgba32(220, 220, 220),
        new Rgba32(0, 0, 0),
        new Rgba32(220, 20, 0),
        new Rgba32(55, 55, 55),
        new Rgba32(220, 200, 0),
        new Rgba32(242, 129, 0),
        new Rgba32(123, 214, 255),
        new Rgba32(53, 172, 74),
    };
    public static Dictionary<Rgba32, Ident> colorToIdent = new Dictionary<Rgba32, Ident>{
        {new Rgba32(18, 80, 150), new Ident("misc\\blue1.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(160, 160, 160), new Ident("misc\\grey.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(100, 62, 37), new Ident("misc\\brown.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(52, 101, 46), new Ident("misc\\green1.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(220, 220, 220), new Ident("misc\\white.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(0, 0, 0), new Ident("misc\\black.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(220, 20, 0), new Ident("misc\\red.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(55, 55, 55), new Ident("misc\\darkgrey.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(220, 200, 0), new Ident("misc\\yellow.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(242, 129, 0), new Ident("misc\\orange.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(123, 214, 255), new Ident("misc\\lightblue.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(53, 172, 74), new Ident("misc\\green3.Item.Gbx", "Stadium", "feor")},
    };

    public static double ColorDifference(Rgba32 color1, Rgba32 color2)
    {
        // return Math.Pow(color1.R - color2.R, 2) + Math.Pow(color1.G - color2.G, 2) + Math.Pow(color1.B - color2.B, 2);
        double rmean = (color1.R + color2.R) / 2;
        double deltaR = color1.R - color2.R;
        double deltaG = color1.G - color2.G;
        double deltaB = color1.B - color2.B;
        double delta = (2 + rmean / 256) * Math.Pow(deltaR, 2) + 4 * Math.Pow(deltaG, 2) + (2 + (255 - rmean) / 256) * Math.Pow(deltaB, 2);
        return delta;
    }

    public static List<Pixel> Palettize(List<Pixel> pixels)
    {
        Dictionary<Rgba32, Rgba32> colorToClosest = new Dictionary<Rgba32, Rgba32>();
        foreach (Pixel pixel in pixels)
        {
            if (colorToClosest.ContainsKey(pixel.color))
            {
                pixel.color = colorToClosest[pixel.color];
                continue;
            }
            double mindiff = -1;
            Rgba32 closest = colors[0];
            foreach (Rgba32 color in colors)
            {
                double diff = ColorDifference(pixel.color, color);
                if (mindiff == -1 || diff < mindiff)
                {
                    mindiff = diff;
                    closest = color;
                }
            }
            colorToClosest[pixel.color] = closest;
            pixel.color = closest;
        }
        return pixels;
    }

    public static Ident GetClosestIdent(Rgba32 color)
    {
        Ident closestIdent = new Ident("misc\\blue1.Item.Gbx", "Stadium", "feor");
        double minDiff = -1;
        foreach (KeyValuePair<Rgba32, Ident> entry in colorToIdent)
        {
            double diff = ColorDifference(color, entry.Key);
            if (minDiff == -1 || diff < minDiff)
            {
                minDiff = diff;
                closestIdent = entry.Value;
            }
        }
        return closestIdent;
    }

    public static List<Pixel> GetPixels(Image<Rgba32> image)
    {

        List<Pixel> pixels = new List<Pixel>();
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                // pixelRow.Length has the same value as accessor.Width,
                // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                for (int x = 0; x < pixelRow.Length; x++)
                {
                    // Get a reference to the pixel at position x
                    ref Rgba32 pixel = ref pixelRow[x];
                    if (pixel.A == 255)
                    {
                        // Overwrite the pixel referenced by 'ref Rgba32 pixel':
                        pixels.Add(new Pixel(x, y, pixel));
                    }
                }
            }
        });
        return pixels;
    }

    public static Image<Rgba32> GeneratePreview(byte[] originalImage, List<Pixel> pixels)
    {
        Image<Rgba32> original = Image.Load<Rgba32>(originalImage);
        Image<Rgba32> image = new Image<Rgba32>(original.Width, original.Height);
        pixels = Palettize(pixels);
        foreach (Pixel pixel in pixels)
        {
            image[pixel.coords.X, pixel.coords.Y] = pixel.color;
        }
        return image;
    }

}

