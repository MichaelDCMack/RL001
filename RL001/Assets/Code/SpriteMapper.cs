using System;
using UnityEngine;
using UnityEditor;

namespace Code
{
    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "Assets/Scriptable Objects/Sprite Mappers/SpriteMapper(New)")]
    [Serializable]
    public class SpriteMapper : ScriptableObject
    {
        public Sprite errorSprite;

        public TileSpriteMapData[] tileSpriteMapData;

        public Sprite MapToSprite(Tile tile)
        {
            foreach(TileSpriteMapData data in tileSpriteMapData)
            {
                if(Array.IndexOf<TileType>(data.tileTypes, tile.TileType) != -1 &&
                   Array.IndexOf<MaterialType>(data.materialTypes, tile.MaterialType) != -1)
                {
                    return data.sprite;
                }
            }

            return errorSprite;
        }

        public Sprite MapToSprite(Map map, int x, int y)
        {
            TileSpriteMapData data = null;
            foreach(TileSpriteMapData d in tileSpriteMapData)
            {
                if(Array.IndexOf<TileType>(d.tileTypes, map[x, y].TileType) != -1 &&
                   Array.IndexOf<MaterialType>(d.materialTypes, map[x, y].MaterialType) != -1)
                {
                    data = d;
                }
            }

            if(data != null)
            {
                if(data.topSprite != null && y > 0 && map[x, y].TileType == map[x, y - 1].TileType)
                {
                    return data.topSprite;
                }
                else
                {
                    return data.sprite;
                }
            }

            return errorSprite;
        }
    }
}