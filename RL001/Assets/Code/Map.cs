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

        public Vector2 size;

        public List<MapType> mapTypes;

        public List<Region> regions;

        Tile[] tiles;

        public Tile this[int x, int y]
        {
            get{ return tiles[y * (int)size.x + x]; }
            set{ tiles[y * (int)size.x + x] = value; }
        }

        // Init
        public Map()
        {
            tiles = new Tile[(int)size.x * (int)size.y];
            for(int i = 0; i < (int)size.x * (int)size.y; ++i)
            {
                tiles[i] = new Tile();
            }
        
            regions = new List<Region>();
            mapTypes = new List<MapType>();

            mapTypes.Add(MapType.Default);
        }

        public Map(int x, int y)
        {
            size.x = x;
            size.y = y;
        
            tiles = new Tile[(int)size.x * (int)size.y];
            for(int i = 0; i < (int)size.x * (int)size.y; ++i)
            {
                tiles[i] = new Tile();
            }
        
            regions = new List<Region>();
            mapTypes = new List<MapType>();

            mapTypes.Add(MapType.Default);
        }

        public Map(Map map)
        {
            size.x = map.size.x;
            size.y = map.size.y;
        
            tiles = (Tile[])map.tiles.Clone();
        
            regions = new List<Region>(map.regions);  //TODO investigate this    
            mapTypes = new List<MapType>(map.mapTypes);
        }

        // Public
        public void ReadGlyphs(string text)
        {
            StringReader sr = new StringReader(text);
            size = sr.ReadLine().ParseToVector2();
        
            tiles = new Tile[(int)size.x * (int)size.y];
        
            for(int y = 0; y < (int)size.y; ++y)
            {
                char[] glyphsArray = sr.ReadLine().ToCharArray();
                for(int x = 0; x < (int)size.x; ++x)
                {
                    this[x, y] = new Tile();
                    this[x, y].TileType = (TileType)Array.FindIndex(DebugGlyphs, g => g == glyphsArray[x]);
                }
            }

            string regionCountString = sr.ReadLine();
            int regionCount = int.Parse(regionCountString);

            for(int i = 0; i < regionCount; ++i)
            {
                Region r = new Region();

                r.Anchor = sr.ReadLine().ParseToVector2();
                r.DebugColor = sr.ReadLine().ParseToColor();
                r.Name = sr.ReadLine();
                r.Size = sr.ReadLine().ParseToVector2();

                regions.Add(r);
            }

            sr.Close();
        }

        public string WriteGlyphs()
        {
            StringWriter sw = new StringWriter();

            sw.WriteLine(size);

            for(int y = 0; y < (int)size.y; ++y)
            {
                for(int x = 0; x < (int)size.x; ++x)
                {
                    sw.Write(DebugGlyphs[(int)this[x, y].TileType]);
                }
                sw.Write('\n');
            }

            sw.WriteLine(regions.Count);

            foreach(Region r in regions)
            {
                sw.Write(r.Anchor);
                sw.Write('\n');
                sw.Write(r.DebugColor);
                sw.Write('\n');
                sw.Write(r.Name);
                sw.Write('\n');
                sw.Write(r.Size);
                sw.Write('\n');
            }

            sw.Flush();

            return sw.ToString();
        }

        public bool CheckStamp(Map subMap, Vector2 offset)
        {
            if(offset.x + subMap.size.x > size.x ||
               offset.y + subMap.size.y > size.y ||
               offset.x < 0 ||
               offset.y < 0)
            {
                return false;
            }

            if(IdenticalAlreadyOnMap(subMap, offset))
            {
                return false;
            }

            for(int y = 0; y < (int)subMap.size.y; ++y)
            {
                for(int x = 0; x < (int)subMap.size.x; ++x)
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

            for(int y = 0; y < subMap.size.y; ++y)
            {
                for(int x = 0; x < subMap.size.x; ++x)
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

        public void StampMap(Map map, Vector2 offset, bool includeRegions = true)
        {
            for(int y = 0; y < map.size.y; ++y)
            {
                for(int x = 0; x < map.size.x; ++x)
                {
                    if(x < size.x && y < size.y)
                    {
                        this[x + (int)offset.x, y + (int)offset.y].Set(CombineTiles(this[x + (int)offset.x, y + (int)offset.y], map[x, y]));
                    }
                }
            }

            if(includeRegions)
            {
                foreach(Region region in map.regions)
                {
                    Region newRegion = new Region(region);

                    newRegion.Anchor += offset;
                    newRegion.ConformToMap(this);

                    if(newRegion.Size.x > 0 && newRegion.Size.y > 0)
                    {
                        regions.Add(newRegion);
                    }
                }
            }
        }

        public int CardinalNeighborCount(Vector2 location, TileType type, bool interCardinal)
        {
            int count = 0;

            if(location.x > 0 && this[(int)location.x - 1, (int)location.y].TileType == type)
                ++count;
            if(location.x + 1 < size.x && this[(int)location.x + 1, (int)location.y].TileType == type)
                ++count;
            if(location.y > 0 && this[(int)location.x, (int)location.y - 1].TileType == type)
                ++count;
            if(location.y + 1 < size.y && this[(int)location.x, (int)location.y + 1].TileType == type)
                ++count;

            if(interCardinal)
            {
                if(location.x > 0 && location.y > 0 &&
                   this[(int)location.x - 1, (int)location.y - 1].TileType == type)
                    ++count;
                if(location.x + 1 < size.x && location.y + 1 < size.y &&
                   this[(int)location.x + 1, (int)location.y + 1].TileType == type)
                    ++count;
                if(location.x + 1 < size.x && location.y > 0 &&
                   this[(int)location.x + 1, (int)location.y - 1].TileType == type)
                    ++count;
                if(location.x > 0 && location.y + 1 < size.y &&
                   this[(int)location.x - 1, (int)location.y + 1].TileType == type)
                    ++count;
            }

            return count;
        }

        public bool PointIsMapEdge(Vector2 location)
        {
            if(location.x + 1 >= size.x || location.x <= 0 ||
               location.y + 1 >= size.y || location.y <= 0)
            {
                return true;
            }

            return false;
        }

        public void Rotate()
        {
            int newSizeX = (int)size.y;
            int newSizeY = (int)size.x;
            Tile[] newTiles = new Tile[newSizeX * newSizeY];

            for(int y = 0; y < newSizeY; ++y)
            {
                for(int x = 0; x < newSizeX; ++x)
                {
                    newTiles[y * newSizeX + x] = this[((int)size.x - 1) - y, x];
                }
            }
        
            size.x = newSizeX;
            size.y = newSizeY;
            tiles = newTiles;
        }

        public void Mirror()
        {
            for(int y = 0; y < size.y; ++y)
            {
                for(int x = 0; x < size.x / 2; ++x)
                {
                    Tile temp = this[x, y];
                    this[x, y] = this[((int)size.x - 1) - x, y];
                    this[((int)size.x - 1) - x, y] = temp;
                }
            }
        }

        public void BuildPatchRegion()
        {
            Region region = new Region();

            region.DebugColor = Color.green;

            int firstX = -1;
            int firstY = -1;
            int lastX = -1;
            int lastY = -1;

            for(int y = 0; y < size.y; ++y)
            {
                for(int x = 0; x < size.x; ++x)
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
                lastX = (int)size.x - 1;
                lastY = (int)size.y - 1;
            }

            region.Anchor = new Vector2(firstX, firstY);
            region.Size = new Vector2(lastX + 1 - firstX, lastY + 1 - firstY);

            regions.Add(region);
        }

        public List<Vector2> GetPointsListByType(TileType type)
        {
            List<Vector2> points = new List<Vector2>();

            for(int y = 0; y < size.y; ++y)
            {
                for(int x = 0; x < size.x; ++x)
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
            for(int y = 0; y < size.y; ++y)
            {
                for(int x = 0; x < size.x; ++x)
                {                
                    this[x, y].TileType = TileType.Empty;
                    this[x, y].MaterialType = MaterialType.Dirt;
                }
            }

            regions.Clear();
        }
    }
}