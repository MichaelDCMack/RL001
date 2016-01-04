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

        bool PlaceMap(Map parentMap, Region region, TileType[] types)
        {
            List<Map> randomizedMaps = GetRandomMaps();

            foreach(Map map in randomizedMaps)
            {
                List<Vector2> matchingPoints = FindTileMatchPoints(parentMap, region, map, types);
                foreach(Vector2 point in matchingPoints)
                {
                    if(PlaceMapHelper(parentMap, map, point, region))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool PlaceMapHelper(Map parentMap, Map map, Vector2 point, Region region)
        {
            if(parentMap.CheckStamp(map, point))
            {
                Region newRegion = new Region();

                newRegion.Anchor = point;
                newRegion.Map = parentMap;
                newRegion.Size = new Vector2(map.sizeX, map.sizeY);

                parentMap.StampMap(map, point);
                parentMap.regions.Add(newRegion);

                if(region != null)
                {
                    newRegion.AttachedRegions.Add(region);
                    region.AttachedRegions.Add(newRegion);
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

        List<Vector2> FindTileMatchPoints(Map parentMap, Region region, Map map, TileType[] types)
        {
            List<Vector2> regionPoints = new List<Vector2>();

            for(int i = 0; i < types.Length; ++i)
            {
                regionPoints.AddRange(region.GetPointsListByType(types[i]));
            }

            regionPoints.RemoveAll(point => 
                parentMap.CardinalNeighborCount(point, TileType.Empty, false) == 0);

            List<Vector2> mapPoints = new List<Vector2>();

            for(int i = 0; i < types.Length; ++i)
            {
                mapPoints.AddRange(map.GetPointsListByType(types[i]));
            }

            List<Vector2> matchingPoints = new List<Vector2>();

            foreach(Vector2 regionPoint in regionPoints)
            {
                foreach(Vector2 mapPoint in mapPoints)
                {
                    Vector2 point = regionPoint - mapPoint;

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

            PlaceMapHelper(parentMap, map, new Vector2(x, y), null);


        }

        public void BuildNextRandomSubMap(Map parentMap)
        {   
            List<Region> regions = new List<Region>(parentMap.regions);

            switch(mode)
            {
                case GenerateMode.BreadthFirst:
                    break;
                case GenerateMode.DepthFirst:
                    regions.Reverse();
                    break;
                case GenerateMode.Random:
                    regions.Shuffle();
                    break;
            }

            foreach(Region region in regions)
            {
                if(PlaceMap(parentMap, region, new []{ TileType.Join }))
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
                        if(parentMap.PointIsMapEdge(new Vector2(x, y)) ||
                           parentMap.CardinalNeighborCount(new Vector2(x, y), TileType.Empty, false) > 0)
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
                       parentMap.CardinalNeighborCount(new Vector2(x, y), TileType.Floor, true) == 0)
                    {
                        parentMap[x, y].TileType = TileType.Empty;
                    }
                    if(parentMap[x, y].TileType == TileType.Empty &&
                       parentMap.CardinalNeighborCount(new Vector2(x, y), TileType.Floor, true) > 0)
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