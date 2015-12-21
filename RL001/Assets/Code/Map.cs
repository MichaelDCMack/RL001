using System;
using System.Collections.Generic;
using System.IO;

namespace Code
{
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

        public Dictionary<TileType, List<Point>> tilePointLists;
        public List<Room> rooms;

        Tile[] tiles;

        public Tile this[int x, int y]
        {
            get{ return tiles[y * sizeX + x]; }
            set{ tiles[y * sizeX + x] = value; }
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

        public string WriteGlyphs()
        {
            StringWriter sw = new StringWriter();

            sw.WriteLine(sizeX);
            sw.WriteLine(sizeY);

            for(int y = 0; y < sizeY; ++y)
            {
                for(int x = 0; x < sizeX; ++x)
                {
                    sw.Write(DebugGlyphs[(int)this[x, y].TileType]);
                }
                sw.Write('\n');
            }

            sw.Flush();

            return sw.ToString();
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
                    if(x < sizeX && y < sizeY)
                    {
                        this[x + offset.x, y + offset.y].Set(CombineTiles(this[x + offset.x, y + offset.y], map[x, y]));
                    }
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

            for(int y = 0; y < newSizeY; ++y)
            {
                for(int x = 0; x < newSizeX; ++x)
                {
                    newTiles[y * newSizeX + x] = this[(sizeX - 1) - y, x];
                }
            }
        
            sizeX = newSizeX;
            sizeY = newSizeY;
            tiles = newTiles;
        }

        public void Mirror()
        {
            for(int y = 0; y < sizeY; ++y)
            {
                for(int x = 0; x < sizeX / 2; ++x)
                {
                    Tile temp = this[x, y];
                    this[x, y] = this[(sizeX - 1) - x, y];
                    this[(sizeX - 1) - x, y] = temp;
                }
            }
        }

        public void BuildPatchRoom()
        {
            Room room = new Room();
            room.Anchor = new Point(0, 0);
            room.Map = this;
            room.ParentMap = this;
            rooms.Add(room);
        }

        public void FindLinkPoints()
        {
            for(int y = 0; y < sizeY; ++y)
            {
                for(int x = 0; x < sizeX; ++x)
                {
                    if(TileIsJoinTile(this[x, y]))
                    {
                        tilePointLists[this[x, y].TileType].Add(new Point(x, y));
                    }
                }
            }
        }

        public List<Point> GetPointsListByType(TileType type)
        {
            return tilePointLists[type];
        }

        public void Clear()
        {
            for(int y = 0; y < sizeY; ++y)
            {
                for(int x = 0; x < sizeX; ++x)
                {
                
                    this[x, y].TileType = TileType.Empty;
                    this[x, y].MaterialType = MaterialType.Dirt;
                }
            }

            rooms.Clear();
        }
    }
}