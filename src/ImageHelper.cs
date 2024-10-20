using System.Reflection.Metadata;
using GBX.NET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
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

    public static Dictionary<Rgba32, Ident> ColorToIdent = new Dictionary<Rgba32, Ident>(){
        {new Rgba32(52, 101, 46), new Ident("colors\\green1.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(33, 119, 24), new Ident("colors\\green2.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(53, 172, 74), new Ident("colors\\green3.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(101, 182, 74), new Ident("colors\\green4.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(0, 0, 0), new Ident("colors\\black.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(55, 55, 55), new Ident("colors\\grey1.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(160, 160, 160), new Ident("colors\\grey2.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(220, 220, 220), new Ident("colors\\grey3.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(37, 189, 0), new Ident("colors\\green5.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(220, 20, 0), new Ident("colors\\red.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(0, 116, 247), new Ident("colors\\blue1.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(220, 200, 0), new Ident("colors\\yellow.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(33, 102, 162), new Ident("colors\\blue2.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(0, 174, 255), new Ident("colors\\blue3.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(123, 214, 255), new Ident("colors\\blue4.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(242, 129, 0), new Ident("colors\\orange.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(102, 85, 63), new Ident("colors\\brown.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(255, 255, 255), new Ident("colors\\white.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(89, 7, 33), new Ident("colors\\magenta.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(23, 19, 41), new Ident("colors\\purple1.Item.Gbx", "Stadium", "feor")},
        {new Rgba32(51, 63, 92), new Ident("colors\\purple2.Item.Gbx", "Stadium", "feor")}
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
            Rgba32 closest = ColorToIdent.Keys.First();
            foreach (Rgba32 color in ColorToIdent.Keys)
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
        Ident closestIdent = new Ident("colors\\blue1.Item.Gbx", "Stadium", "feor");
        double minDiff = -1;
        foreach (KeyValuePair<Rgba32, Ident> entry in ColorToIdent)
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

