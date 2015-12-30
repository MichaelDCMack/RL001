using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class Room
    {
        public List<Room> AttachedRooms
        {
            get;
            protected set;
        }

        public Map Map
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

        public List<Vector2> GetPointsListByType(TileType type)
        {
            List<Vector2> newList = new List<Vector2>();
            List<Vector2> pointList = Map.GetPointsListByType(type);

            foreach(Vector2 v in pointList)
            {
                if(ContainsPoint(v))
                {
                    newList.Add(v);
                }
            }

            return newList;
        }

        public Room()
        {
            AttachedRooms = new List<Room>();
            DebugColor = Color.red;
        }
    }
}