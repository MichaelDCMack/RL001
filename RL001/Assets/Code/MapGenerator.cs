using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Code
{
    [ExecuteInEditMode]
    [CreateAssetMenu]
    [Serializable]
    public class MapGenerator : ScriptableObject
    {
        public enum GenerateMode
        {
            DepthFirst,
            BreadthFirst,
            Random
        }

        public int sizeX = 100;
        public int sizeY = 100;
        public bool seedRandomNumberGenerator;
        public int seed;
        public GenerateMode mode = GenerateMode.DepthFirst;
        public MapGeneratorData[] mapDataArray;

        public Map MasterMap
        {
            get;
            set;
        }

        // Init
        public void Init()
        {
            if(seedRandomNumberGenerator)
            {
                UnityEngine.Random.seed = seed;
            }
            else
            {
                seed = UnityEngine.Random.seed;
            }

            Debug.Log("Random Number Generator Seeded to " + seed);

            InitMaps();
        }

        #region private

        // Private
        void InitMaps()
        {
            MasterMap = new Map(sizeX, sizeY);

            foreach(MapGeneratorData mapData in mapDataArray)
            {
                mapData.LoadMap();
            }
        }

        List<Map> GetRandomMaps()
        {
            List<Map> maps = new List<Map>();

            foreach(MapGeneratorData mapData in mapDataArray)
            {
                if(mapData.IsUsable())
                {
                    maps.AddRange(mapData.Maps);
                }
            }

            maps.Shuffle();

            return maps;
        }

        Map GetRandomMap()
        {
            List<Map> maps = GetRandomMaps();

            if(maps != null && maps.Count > 0)
            {
                return maps[0];
            }

            return null;
        }

        bool PlaceMap(Room room, TileType[] types)
        {
            List<Map> randomizedMaps = GetRandomMaps();

            foreach(Map map in randomizedMaps)
            {
                List<Point> matchingPoints = FindTileMatchPoints(room, map, types);
                foreach(Point point in matchingPoints)
                {
                    if(PlaceMapHelper(map, point, room))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool PlaceMapHelper(Map map, Point point, Room room)
        {
            if(MasterMap.CheckStamp(map, point))
            {
                Room newRoom = new Room();

                newRoom.Anchor = point;
                newRoom.Map = map;
                newRoom.ParentMap = MasterMap;

                MasterMap.StampMap(map, point);
                MasterMap.rooms.Add(newRoom);

                if(room != null)
                {
                    newRoom.AttachedRooms.Add(room);
                    room.AttachedRooms.Add(newRoom);
                }

                foreach(MapGeneratorData mapData in mapDataArray)
                {
                    if(Array.FindIndex(mapData.Maps, m => m == map) != -1)
                    {
                        ++mapData.TimesUsed;
                        break;
                    }
                }

                return true;
            }

            return false;
        }

        List<Point> FindTileMatchPoints(Room room, Map map, TileType[] types)
        {
            List<Point> roomPoints = new List<Point>();

            for(int i = 0; i < types.Length; ++i)
            {
                roomPoints.AddRange(room.Map.GetPointsListByType(types[i]));
            }

            roomPoints.RemoveAll(point => 
                MasterMap.CardinalNeighborCount(point + room.Anchor, TileType.Empty, false) == 0);

            List<Point> mapPoints = new List<Point>();

            for(int i = 0; i < types.Length; ++i)
            {
                mapPoints.AddRange(map.GetPointsListByType(types[i]));
            }

            List<Point> matchingPoints = new List<Point>();

            foreach(Point roomPoint in roomPoints)
            {
                foreach(Point mapPoint in mapPoints)
                {
                    Point point = roomPoint + room.Anchor - mapPoint;

                    matchingPoints.Add(point);
                }
            }

            return matchingPoints;
        }

        #endregion private

        #region public

        // Public
        public void BuildRandomJoinTile()
        {
            int x = UnityEngine.Random.Range(0, sizeX);
            int y = UnityEngine.Random.Range(0, sizeY);

            Map map = new Map(1, 1);

            map[0, 0].TileType = TileType.Join;
            map.FindLinkPoints();

            PlaceMapHelper(map, new Point(x, y), null);


        }

        public void BuildNextRandomSubMap()
        {   
            List<Room> rooms = new List<Room>(MasterMap.rooms);

            switch(mode)
            {
                case GenerateMode.BreadthFirst:
                    break;
                case GenerateMode.DepthFirst:
                    rooms.Reverse();
                    break;
                case GenerateMode.Random:
                    rooms.Shuffle();
                    break;
            }
            foreach(Room room in rooms)
            {
                if(PlaceMap(room, new []{ TileType.Join }))
                {
                    return;
                }
            }           
        }

        //Rough
        public void PostProcessMap()
        {
            //first pass
            for(int i = 0; i < MasterMap.sizeX; ++i)
            {                
                for(int j = 0; j < MasterMap.sizeY; ++j)
                {
                    if(MasterMap[i, j].TileType == TileType.Join)
                    {
                        MasterMap[i, j].TileType = TileType.Floor;
                    }
                    if(MasterMap[i, j].TileType == TileType.Floor)
                    {
                        if(MasterMap.PointIsMapEdge(new Point(i, j)) || MasterMap.CardinalNeighborCount(new Point(i, j), TileType.Empty, false) > 0)
                        {
                            MasterMap[i, j].TileType = TileType.Solid;
                        }
                    }
                }
            }

            //second pass
            for(int i = 0; i < MasterMap.sizeX; ++i)
            {
                for(int j = 0; j < MasterMap.sizeY; ++j)
                {
                    if(MasterMap[i, j].TileType == TileType.Solid &&
                       MasterMap.CardinalNeighborCount(new Point(i, j), TileType.Floor, true) == 0)
                    {
                        MasterMap[i, j].TileType = TileType.Empty;
                    }
                    if(MasterMap[i, j].TileType == TileType.Empty &&
                       MasterMap.CardinalNeighborCount(new Point(i, j), TileType.Floor, true) > 0)
                    {
                        MasterMap[i, j].TileType = TileType.Solid;
                    }
                }
            }
        }

        public void SaveMap()
        {
            using(StreamWriter sw = new StreamWriter("Assets/Maps/saved-masterMap-(" + seed + ").json", false))
            {
                string json = JsonUtility.ToJson(MasterMap, true);

                sw.Write(json);

                sw.Flush();
                sw.Close();
            }
        }

        public void LoadMap()
        {
            using(StreamReader sr = new StreamReader("Assets/Maps/saved-masterMap-(" + seed + ").json"))
            {
                string json = sr.ReadToEnd();

                MasterMap = JsonUtility.FromJson<Map>(json);

                sr.Close();
            }
        }

        #endregion public
    }
}