using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;

public class MapInfo
{
    public string Author { get; set; } = "";
    public string Name { get; set; } = "";
    public Int3 size { get; set; } = new Int3(0, 0, 0);
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

    public void save_map(string path = "")
    {
        if (path != "")
        {
            map_path = path;
        }
        map.Save(map_path);
    }
}
