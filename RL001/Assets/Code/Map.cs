using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Code
{
    public class Map
    {
        public static char[] DebugGlyphs = {
            ' ',
            '~',
            '.',
            '#',
            'J',
        };
        
        public int sizeX = 100;
        public int sizeY = 100;

        public List<MapType> mapTypes;

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
        }

        public Map(Map map)
        {
            sizeX = map.sizeX;
            sizeY = map.sizeY;
        
            tiles = (Tile[])map.tiles.Clone();
        
            rooms = new List<Room>(map.rooms);      
            mapTypes = new List<MapType>(map.mapTypes);
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

        public bool CheckStamp(Map subMap, Vector2 offset)
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
                    Tile mapTile = this[x + (int)offset.x, y + (int)offset.y];
                    Tile subMapTile = subMap[x, y];
                
                    if(!TilesCanCombine(mapTile, subMapTile))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IdenticalAlreadyOnMap(Map subMap, Vector2 offset)
        {
            bool allEmpty = true;

            for(int y = 0; y < subMap.sizeY; ++y)
            {
                for(int x = 0; x < subMap.sizeX; ++x)
                {
                    TileType mapTileType = this[x + (int)offset.x, y + (int)offset.y].TileType;
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

        public void StampMap(Map map, Vector2 offset)
        {
            for(int y = 0; y < map.sizeY; ++y)
            {
                for(int x = 0; x < map.sizeX; ++x)
                {
                    if(x < sizeX && y < sizeY)
                    {
                        this[x + (int)offset.x, y + (int)offset.y].Set(CombineTiles(this[x + (int)offset.x, y + (int)offset.y], map[x, y]));
                    }
                }
            }
        }

        public int CardinalNeighborCount(Vector2 location, TileType type, bool interCardinal)
        {
            int count = 0;

            if(location.x > 0 && this[(int)location.x - 1, (int)location.y].TileType == type)
                ++count;
            if(location.x + 1 < sizeX && this[(int)location.x + 1, (int)location.y].TileType == type)
                ++count;
            if(location.y > 0 && this[(int)location.x, (int)location.y - 1].TileType == type)
                ++count;
            if(location.y + 1 < sizeY && this[(int)location.x, (int)location.y + 1].TileType == type)
                ++count;

            if(interCardinal)
            {
                if(location.x > 0 && location.y > 0 &&
                   this[(int)location.x - 1, (int)location.y - 1].TileType == type)
                    ++count;
                if(location.x + 1 < sizeX && location.y + 1 < sizeY &&
                   this[(int)location.x + 1, (int)location.y + 1].TileType == type)
                    ++count;
                if(location.x + 1 < sizeX && location.y > 0 &&
                   this[(int)location.x + 1, (int)location.y - 1].TileType == type)
                    ++count;
                if(location.x > 0 && location.y + 1 < sizeY &&
                   this[(int)location.x - 1, (int)location.y + 1].TileType == type)
                    ++count;
            }

            return count;
        }

        public bool PointIsMapEdge(Vector2 location)
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

            room.DebugColor = Color.green;
            room.Map = this;

            int firstX = -1;
            int firstY = -1;
            int lastX = -1;
            int lastY = -1;

            for(int y = 0; y < sizeY; ++y)
            {
                for(int x = 0; x < sizeX; ++x)
                {
                    if(this[x, y].TileType != TileType.Empty)
                    {
                        if(firstX == -1 || firstX > x)
                        {
                            firstX = x;
                        }

                        if(firstY == -1 || firstY > y)
                        {
                            firstY = y;  
                        }

                        if(lastX < x)
                        {
                            lastX = x;
                        }

                        if(lastY < y)
                        {
                            lastY = y;
                        }
                    }
                }
            }

            if(firstX == -1)
            {
                firstX = firstY = 0;
                lastX = sizeX - 1;
                lastY = sizeY - 1;
            }

            room.Anchor = new Vector2(firstX, firstY);
            room.Size = new Vector2(lastX + 1 - firstX, lastY + 1 - firstY);

            rooms.Add(room);
        }

        public List<Vector2> GetPointsListByType(TileType type)
        {
            List<Vector2> points = new List<Vector2>();

            for(int y = 0; y < sizeY; ++y)
            {
                for(int x = 0; x < sizeX; ++x)
                {
                    if(this[x, y].TileType == type)
                    {
                        points.Add(new Vector2(x, y));
                    }
                }
            }

            return points;
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