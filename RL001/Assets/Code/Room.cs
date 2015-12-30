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

        Map map;

        public Map Map
        {
            get
            {
                if(map != null)
                {
                    return map;
                }
                else //patch map
                {
                    return ParentMap;
                }
            }
            set{ map = value; }
        }

        public Map ParentMap
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

        public Vector2 PatchAnchor
        {
            get;
            set;
        }

        public Vector2 PatchSize
        {
            get;
            set;
        }

        public bool IsPatch
        {
            get
            {
                if(map == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Room()
        {
            AttachedRooms = new List<Room>();
            DebugColor = Color.red;
        }
    }
}