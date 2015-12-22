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

        public Room()
        {
            AttachedRooms = new List<Room>();
        }
    }
}