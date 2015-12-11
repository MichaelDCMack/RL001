using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Code
{
    [Serializable]
    public class Map
    {
        public static char[] DebugGlyphs = new []
        {
            ' ',
            '~',
            '.',
            '#',
            'J',
        };
        
        public int sizeX = 100;
        public int sizeY = 100;

        public List<MapType> mapTypes;

        [NonSerialized]
        public Dictionary<TileType, List<Point>> tilePointLists;
        [NonSerialized]
        public List<Room> rooms;

        [SerializeField]
        Tile[] tiles;

        public Tile this[int x, int y]
        {
            get{ return tiles[x * sizeY + y]; }
            set{ tiles[x * sizeY + y] = value; }
        }

        // Init
        public Map()
        {
            tiles = new Tile[sizeX * sizeY];
            for(int i = 0; i < sizeX * sizeY; ++i)
            {
                tiles[i] = new Tile();
            }
        
            rooms = new List<Room>();
            mapTypes = new List<MapType>();

            mapTypes.Add(MapType.Default);
        
            InitTilePointLists();
        }

        public Map(int x, int y)
        {
            sizeX = x;
            sizeY = y;
        
            tiles = new Tile[sizeX * sizeY];
            for(int i = 0; i < sizeX * sizeY; ++i)
            {
                tiles[i] = new Tile();
            }
        
            rooms = new List<Room>();
            mapTypes = new List<MapType>();

            mapTypes.Add(MapType.Default);

            InitTilePointLists();
        }

        public Map(Map map)
        {
            sizeX = map.sizeX;
            sizeY = map.sizeY;
        
            tiles = (Tile[])map.tiles.Clone();
        
            rooms = new List<Room>(map.rooms);      
            mapTypes = new List<MapType>(map.mapTypes);
        
            InitTilePointLists();
        }

        // Private
        void InitTilePointLists()
        {
            tilePointLists = new Dictionary<TileType, List<Point>>();

            for(TileType i = TileType.Error; i < TileType.NumberOfTypes; ++i)
            {
                tilePointLists.Add(i, new List<Point>());
            }
        }

        // Public
        public void WriteGlyphs(string filePath)
        {
            StreamWriter sw = new StreamWriter(filePath, false);
            sw.WriteLine(sizeX);
            sw.WriteLine(sizeY);
        
            for(int y = 0; y < sizeY; ++y)
            {
                string output = "";

                for(int x = 0; x < sizeX; ++x)
                {
                    output += DebugGlyphs[(int)this[x, y].TileType];
                }

                output += "\n";

                sw.Write(output);
            }
        
            sw.Flush();
            sw.Close();
        
            Debug.Log("Current state of map (in glyphs) written to : " + filePath);
        }

        public void ReadGlyphs(string text)
        {
            StringReader sr = new StringReader(text);
            string sizeXString = sr.ReadLine();
            string sizeYString = sr.ReadLine();
        
            sizeX = int.Parse(sizeXString);
            sizeY = int.Parse(sizeYString);
        
            tiles = new Tile[sizeX * sizeY];
        
            for(int y = 0; y < sizeY; ++y)
            {
                char[] glyphsArray = sr.ReadLine().ToCharArray();
                for(int x = 0; x < sizeX; ++x)
                {
                    this[x, y] = new Tile();
                    this[x, y].TileType = (TileType)Array.FindIndex(DebugGlyphs, g => g == glyphsArray[x]);
                }
            }
        
            sr.Close();
        }

        public bool CheckStamp(Map subMap, Point offset)
        {
            if(offset.x + subMap.sizeX > sizeX ||
               offset.y + subMap.sizeY > sizeY ||
               offset.x < 0 ||
               offset.y < 0)
            {
                return false;
            }

            if(IdenticalAlreadyOnMap(subMap, offset))
            {
                return false;
            }

            for(int y = 0; y < subMap.sizeY; ++y)
            {
                for(int x = 0; x < subMap.sizeX; ++x)
                {
                    Tile mapTile = this[x + offset.x, y + offset.y];
                    Tile subMapTile = subMap[x, y];
                
                    if(!TilesCanCombine(mapTile, subMapTile))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IdenticalAlreadyOnMap(Map subMap, Point offset)
        {
            bool allEmpty = true;

            for(int y = 0; y < subMap.sizeY; ++y)
            {
                for(int x = 0; x < subMap.sizeX; ++x)
                {
                    TileType mapTileType = this[x + offset.x, y + offset.y].TileType;
                    TileType subMapTileType = subMap[x, y].TileType;
                
                    if(mapTileType != TileType.Empty)
                    {
                        allEmpty = false;
                    }

                    if(subMapTileType != TileType.Empty)
                    {
                        if(mapTileType != subMapTileType)
                        {
                            return false;
                        }
                    }
                }
            }

            return !allEmpty;
        }

        public bool TilesCanCombine(Tile t1, Tile t2)
        {
            if(t1.TileType == TileType.Empty || t2.TileType == TileType.Empty)
            {
                return true;
            }
        
            return t1.TileType == t2.TileType;
        }

        public bool TileIsJoinTile(Tile t)
        {
            return t.TileType == TileType.Join;
        }

        public Tile CombineTiles(Tile t1, Tile t2)
        {
            return t1.TileType == TileType.Empty ? t2 : t1;
        }

        public void StampMap(Map map, Point offset)
        {
            for(int y = 0; y < map.sizeY; ++y)
            {
                for(int x = 0; x < map.sizeX; ++x)
                {
                    this[x + offset.x, y + offset.y].Set(CombineTiles(this[x + offset.x, y + offset.y], map[x, y]));
                }
            }
        }

        public int CardinalNeighborCount(Point location, TileType type, bool interCardinal)
        {
            int count = 0;

            if(location.x > 0 && this[location.x - 1, location.y].TileType == type)
                ++count;
            if(location.x + 1 < sizeX && this[location.x + 1, location.y].TileType == type)
                ++count;
            if(location.y > 0 && this[location.x, location.y - 1].TileType == type)
                ++count;
            if(location.y + 1 < sizeY && this[location.x, location.y + 1].TileType == type)
                ++count;

            if(interCardinal)
            {
                if(location.x > 0 && location.y > 0 &&
                   this[location.x - 1, location.y - 1].TileType == type)
                    ++count;
                if(location.x + 1 < sizeX && location.y + 1 < sizeY &&
                   this[location.x + 1, location.y + 1].TileType == type)
                    ++count;
                if(location.x + 1 < sizeX && location.y > 0 &&
                   this[location.x + 1, location.y - 1].TileType == type)
                    ++count;
                if(location.x > 0 && location.y + 1 < sizeY &&
                   this[location.x - 1, location.y + 1].TileType == type)
                    ++count;
            }

            return count;
        }

        public bool PointIsMapEdge(Point location)
        {
            if(location.x + 1 >= sizeX || location.x <= 0 ||
               location.y + 1 >= sizeY || location.y <= 0)
            {
                return true;
            }

            return false;
        }

        public void Rotate()
        {
            int newSizeX = sizeY;
            int newSizeY = sizeX;
            Tile[] newTiles = new Tile[newSizeX * newSizeY];
        
            for(int i = 0; i < newSizeX; ++i)
            {
                for(int j = 0; j < newSizeY; ++j)
                {
                    newTiles[i * newSizeY + j] = this[(sizeX - 1) - j, i];
                }
            }
        
            sizeX = newSizeX;
            sizeY = newSizeY;
            tiles = newTiles;
        }

        public void Mirror()
        {
            for(int i = 0; i < sizeX / 2; ++i)
            {
                for(int j = 0; j < sizeY; ++j)
                {
                    Tile temp = this[i, j];
                    this[i, j] = this[(sizeX - 1) - i, j];
                    this[(sizeX - 1) - i, j] = temp;
                }
            }
        }

        public void FindLinkPoints()
        {
            for(int i = 0; i < sizeX; ++i)
            {
                for(int j = 0; j < sizeY; ++j)
                {
                    if(TileIsJoinTile(this[i, j]))
                    {
                        tilePointLists[this[i, j].TileType].Add(new Point(i, j));
                    }
                }
            }
        }

        public List<Point> GetPointsListByType(TileType type)
        {
            return tilePointLists[type];
        }
    }
}