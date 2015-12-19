using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Code
{
    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "Assets/Scriptable Objects/Map Generators/MapGenerator(New)")]
    [Serializable]
    public class MapGenerator : ScriptableObject
    {
        public enum GenerateMode
        {
            DepthFirst,
            BreadthFirst,
            Random
        }

        public bool seedRandomNumberGenerator;
        public int seed;
        public GenerateMode mode = GenerateMode.DepthFirst;
        public MapGeneratorData[] mapDataArray;

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

            InitMapData();
        }

        #region private

        // Private
        void InitMapData()
        {
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

        bool PlaceMap(Map parentMap, Room room, TileType[] types)
        {
            List<Map> randomizedMaps = GetRandomMaps();

            foreach(Map map in randomizedMaps)
            {
                List<Point> matchingPoints = FindTileMatchPoints(parentMap, room, map, types);
                foreach(Point point in matchingPoints)
                {
                    if(PlaceMapHelper(parentMap, map, point, room))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool PlaceMapHelper(Map parentMap, Map map, Point point, Room room)
        {
            if(parentMap.CheckStamp(map, point))
            {
                Room newRoom = new Room();

                newRoom.Anchor = point;
                newRoom.Map = map;
                newRoom.ParentMap = parentMap;

                parentMap.StampMap(map, point);
                parentMap.rooms.Add(newRoom);

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

        List<Point> FindTileMatchPoints(Map parentMap, Room room, Map map, TileType[] types)
        {
            List<Point> roomPoints = new List<Point>();

            for(int i = 0; i < types.Length; ++i)
            {
                roomPoints.AddRange(room.Map.GetPointsListByType(types[i]));
            }

            roomPoints.RemoveAll(point => 
                parentMap.CardinalNeighborCount(point + room.Anchor, TileType.Empty, false) == 0);

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
        public void BuildRandomJoinTile(Map parentMap)
        {
            int x = UnityEngine.Random.Range(0, parentMap.sizeX);
            int y = UnityEngine.Random.Range(0, parentMap.sizeY);

            Map map = new Map(1, 1);

            map[0, 0].TileType = TileType.Join;
            map.FindLinkPoints();

            PlaceMapHelper(parentMap, map, new Point(x, y), null);


        }

        public void BuildNextRandomSubMap(Map parentMap)
        {   
            List<Room> rooms = new List<Room>(parentMap.rooms);

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
                if(PlaceMap(parentMap, room, new []{ TileType.Join }))
                {
                    return;
                }
            }           
        }

        //Rough
        public void PostProcessMap(Map parentMap)
        {
            //first pass
            for(int y = 0; y < parentMap.sizeY; ++y)
            {
                for(int x = 0; x < parentMap.sizeX; ++x)
                {                
                
                    if(parentMap[x, y].TileType == TileType.Join)
                    {
                        parentMap[x, y].TileType = TileType.Floor;
                    }
                    if(parentMap[x, y].TileType == TileType.Floor)
                    {
                        if(parentMap.PointIsMapEdge(new Point(x, y)) ||
                           parentMap.CardinalNeighborCount(new Point(x, y), TileType.Empty, false) > 0)
                        {
                            parentMap[x, y].TileType = TileType.Solid;
                        }
                    }
                }
            }

            //second pass
            for(int y = 0; y < parentMap.sizeY; ++y)
            {
                for(int x = 0; x < parentMap.sizeX; ++x)
                {
                
                    if(parentMap[x, y].TileType == TileType.Solid &&
                       parentMap.CardinalNeighborCount(new Point(x, y), TileType.Floor, true) == 0)
                    {
                        parentMap[x, y].TileType = TileType.Empty;
                    }
                    if(parentMap[x, y].TileType == TileType.Empty &&
                       parentMap.CardinalNeighborCount(new Point(x, y), TileType.Floor, true) > 0)
                    {
                        parentMap[x, y].TileType = TileType.Solid;
                    }
                }
            }
        }

        public void ClearMap(Map parentMap)
        {
            parentMap.Clear();
            ResetUsedCounts();
        }

        public void ResetUsedCounts()
        {
            foreach(MapGeneratorData mgd in mapDataArray)
            {
                mgd.TimesUsed = 0;
            }
        }

        public void SaveMap(Map parentMap)
        {
            using(StreamWriter sw = new StreamWriter("Assets/Maps/saved-masterMap-(" + seed + ").json", false))
            {
                string json = JsonUtility.ToJson(parentMap, true);

                sw.Write(json);

                sw.Flush();
                sw.Close();
            }
        }

        public void LoadMap(Map parentMap)
        {
            using(StreamReader sr = new StreamReader("Assets/Maps/saved-masterMap-(" + seed + ").json"))
            {
                string json = sr.ReadToEnd();

                parentMap = JsonUtility.FromJson<Map>(json);

                sr.Close();
            }
        }

        #endregion public
    }
}