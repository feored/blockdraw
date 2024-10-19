using GBX.NET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


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
public class ImageHelper
{
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

    public static double colorDifference(Rgba32 color1, Rgba32 color2)
    {
        // return Math.Pow(color1.R - color2.R, 2) + Math.Pow(color1.G - color2.G, 2) + Math.Pow(color1.B - color2.B, 2);
        double rmean = (color1.R + color2.R) / 2;
        double deltaR = color1.R - color2.R;
        double deltaG = color1.G - color2.G;
        double deltaB = color1.B - color2.B;
        double delta = (2 + rmean / 256) * Math.Pow(deltaR, 2) + 4 * Math.Pow(deltaG, 2) + (2 + (255 - rmean) / 256) * Math.Pow(deltaB, 2);
        return delta;
    }

    public static Ident getClosestIdent(Rgba32 color)
    {
        Ident closestIdent = new Ident("misc\\blue1.Item.Gbx", "Stadium", "feor");
        double minDiff = -1;
        foreach (KeyValuePair<Rgba32, Ident> entry in colorToIdent)
        {
            double diff = colorDifference(color, entry.Key);
            if (minDiff == -1 || diff < minDiff)
            {
                minDiff = diff;
                closestIdent = entry.Value;
            }
        }
        return closestIdent;
    }

    public static List<Pixel> GetPixels(Stream imageStream)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(imageStream);

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
}

