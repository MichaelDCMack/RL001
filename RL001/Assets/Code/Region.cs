using System.Collections.Generic;
using UnityEngine;
using System;

namespace Code
{
    public class Region
    {
        public string Name
        {
            get;
            set;
        }

        public Vector2 Anchor
        {
            get;
            set;
        }

        public Vector2 Size
        {
            get;
            set;
        }

        public Color DebugColor
        {
            get;
            set;
        }

        public void ConformToMap(Map map)
        {
            Vector2 newAnchor;

            newAnchor.x = Math.Max(Anchor.x, 0);
            newAnchor.y = Math.Max(Anchor.y, 0);
            newAnchor.x = Math.Min(Anchor.x, map.size.x);
            newAnchor.y = Math.Min(Anchor.y, map.size.y);
            Anchor = newAnchor;

            Vector2 newSize;

            newSize.x = Math.Min(Size.x, map.size.x - Anchor.x);
            newSize.y = Math.Min(Size.y, map.size.y - Anchor.y);
            Size = newSize;
        }

        public bool ContainsPoint(Vector2 point)
        {
            if(point.x >= Anchor.x && point.x < Anchor.x + Size.x &&
               point.y >= Anchor.y && point.y < Anchor.y + Size.y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<Vector2> GetPointsListByType(Map map, TileType type)
        {
            List<Vector2> newList = new List<Vector2>();
            List<Vector2> pointList = map.GetPointsListByType(type);

            foreach(Vector2 v in pointList)
            {
                if(ContainsPoint(v))
                {
                    newList.Add(v);
                }
            }

            return newList;
        }

        public Region()
        {
            DebugColor = Color.red;
            Name = "unnamed";
        }

        public Region(Region region)
        {
            Anchor = region.Anchor;
            DebugColor = region.DebugColor;
            Name = region.Name;
            Size = region.Size;
        }
    }
}