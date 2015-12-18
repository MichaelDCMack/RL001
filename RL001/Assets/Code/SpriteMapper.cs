using System;
using UnityEngine;

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
    }
}