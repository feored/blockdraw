using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;

public class MapInfo
{
    public string Author { get; set; } = "";
    public string Name { get; set; } = "";
    public Int3 size { get; set; } = new Int3(0, 0, 0);
}

public class DrawOptions
{
    public Vec3 StartPosition { get; set; }
    public bool Horizontal { get; set; }
}
public class BlockBuilder
{
    private string map_path;
    public CGameCtnChallenge map;

    public BlockBuilder(string path)
    {
        Gbx.LZO = new Lzo();
        map_path = path;
        map = Gbx.ParseNode<CGameCtnChallenge>(map_path);
    }

    static public string GetDefaultMapLocation()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ManiaPlanet", "Maps");
    }

    public MapInfo GetMapInfo()
    {
        return new MapInfo
        {
            Author = map.AuthorLogin ?? "",
            Name = map.MapName,
            size = map.Size
        };
    }

    public void addBlocks(List<Int3> points, string blockName = "StadiumCircuitBase")
    {
        for (int i = 0; i < points.Count; i++)
        {
            map.PlaceBlock(blockName, points[i], Direction.North, false);
        }
    }

    public void addAnchoredObjects(Ident model, List<Int3> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            map.PlaceAnchoredObject(model, points[i], Vec3.Zero);
        }
    }

    public void addAnchordObject(Ident model, Int3 point)
    {
        map.PlaceAnchoredObject(model, point, Vec3.Zero);
    }

    public void DrawPalettizedImage(List<Pixel> pixels, DrawOptions options)
    {
        var scale = 4;
        // default size of a block in TM Stadium is 32x32x8
        Int3 start = new Int3((int)options.StartPosition.X * 32, (int)options.StartPosition.Y * 8, (int)options.StartPosition.Z * 32);
        foreach (Pixel pixel in pixels)
        {
            Int3 pos = new Int3();
            if (options.Horizontal)
            {
                pos = new Int3(start.X + pixel.coords.X * scale, start.Y, start.Z + pixel.coords.Y * scale);
            }
            else
            {
                pos = new Int3(pixel.coords.X * scale, start.Y - (pixel.coords.Y * scale), 0);
            }
            var block = ImageHelper.ColorToIdent[pixel.color];
            addAnchordObject(block, pos);
        }
    }

    public void Save(string path = "")
    {
        if (path != "")
        {
            map_path = path;
        }
        map.Save(map_path);
    }

    public void WipePalette(List<Ident> palette)
    {
        if (palette.Count == 0 || map.AnchoredObjects == null || map.AnchoredObjects.Count == 0)
        {
            return;
        }
        foreach (CGameCtnAnchoredObject anchoredObject in map.AnchoredObjects.ToList())
        {
            if (palette.Contains(anchoredObject.ItemModel))
            {
                map.RemoveAnchoredObject(anchoredObject);
            }
        }
    }
}
