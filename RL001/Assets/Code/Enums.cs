namespace Code
{
    public enum TileType
    {
        Error = -1,
        Empty = 0,
        Liquid,
        Floor,
        Solid,
        Join,

        NumberOfTypes,
    };

    public enum MaterialType
    {
        Error = -1,
        Dirt = 0,
        Stone,
        Wood,
        Metal,

        NumberOfTypes,
    };

    public enum MapType
    {
        Default,

        NumberOfTypes,
    };
}
