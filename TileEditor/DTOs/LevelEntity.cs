﻿namespace TileEditor.DTOs;

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
}

public class LevelEndEntity
{
    public int XPos { get; set; }

    public int YPos { get; set; }
}

public class StartEntity
{
    public int XPos { get; set; }

    public int YPos { get; set; }
}

public class LayerEntity
{
    public TileEntity[] Tiles { get; set; } = [];
}

public class LevelEntity
{
    public string Background { get; set; }

    public string Music { get; set; }

    public LayerEntity[] Layers { get; set; }

    public GameObjectEntity[] GameObjects { get; set; } = [];

    public LevelEndEntity LevelEnd { get; set; }

    public StartEntity Start { get; set; }

    public string NextLevel { get; set; }
}