using System;
using UnityEngine;

namespace Code
{
    [Serializable]
    public class TileSpriteMapData
    {
        public Sprite sprite;
        public TileType[] tileTypes;
        public MaterialType[] materialTypes;
    }
}