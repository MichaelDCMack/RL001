using System;
using UnityEngine;

namespace Code
{
    [Serializable]
    public class Tile
    {
        [SerializeField]
        TileType t;

        public TileType TileType
        {
            get{ return t; }
            set{ t = value; }
        }

        [SerializeField]
        MaterialType m;

        public MaterialType MaterialType
        {
            get{ return m; }
            set{ m = value; }
        }

        public void Set(Tile t)
        {
            TileType = t.TileType;
            MaterialType = t.MaterialType;
        }
    };
}
