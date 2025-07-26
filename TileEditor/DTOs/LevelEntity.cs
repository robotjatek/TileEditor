namespace TileEditor.DTOs;

public class TileEntity
{
    public int XPos { get; set; }

    public int YPos { get; set; }

    public string Texture { get; set; }
}

public class GameObjectEntity
{
    public string Type { get; set; }

    public int XPos { get; set; }

    public int YPos { get; set; }

    public string Name { get; set; }

    public Dictionary<string, object> Props { get; set; } = [];
}

public class StartEntity
{
    public int XPos { get; set; }

    public int YPos { get; set; }
}

public class LayerEntity
{
    public string Name { get; set; } = "New layer";
    public TileEntity[] Tiles { get; set; } = [];
    public double ParallaxOffsetFactorX { get; set; }
    public double ParallaxOffsetFactorY { get; set; }
    public double LayerOffsetX { get; set; }
    public double LayerOffsetY { get; set; }
}

public class EventEntity
{
    public string Type { get; set; }
    
    public Dictionary<string, object> Props { get; set; } = [];
}

public class LevelEntity
{
    public string Background { get; set; }

    public string Music { get; set; }

    public LayerEntity[] Layers { get; set; }

    public int DefaultLayer { get; set; }

    public GameObjectEntity[] GameObjects { get; set; } = [];

    public StartEntity? Start { get; set; }

    public string NextLevel { get; set; }

    public EventEntity[] Events { get; set; } = [];

    public string? InitialEventKey { get; set; }
}