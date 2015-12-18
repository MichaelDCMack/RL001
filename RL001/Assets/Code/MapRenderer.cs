﻿using UnityEngine;

namespace Code
{
    [ExecuteInEditMode]
    public class MapRenderer : MonoBehaviour
    {
        public GameObject tilePrefab;
        public SpriteMapper spriteMapper;
        public int sizeX;
        public int sizeY;

        int oldSizeX;
        int oldSizeY;

        //Debug Hack
        public MapGenerator mapGenerator;

        MapGenerator oldMapGenerator;

        public Map Map
        {
            get;
            set;
        }

        GameObject[] tiles;

        // Use this for initialization
        void Start()
        {
            deleteTiles();
            cloneTiles();
            InitMapGenerator();
        }

        void deleteTiles()
        {
            int count = transform.childCount;
            for(int i = count - 1; i >= 0; i--)
            {
                if(Application.isEditor)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
                else
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

            tiles = null;
        }

        void cloneTiles()
        {
            tiles = new GameObject[sizeX * sizeY];
            for(int y = 0; y < sizeY; y++)
            {
                for(int x = 0; x < sizeX; x++)
                {
                    GameObject go = Instantiate(
                                        tilePrefab, 
                                        new Vector3(x - sizeX / 2.0f + 0.5f, y - sizeY / 2.0f + 0.5f),
                                        Quaternion.identity) as GameObject;

                    go.transform.SetParent(transform);
                    go.hideFlags = HideFlags.DontSaveInEditor;

                    tiles[y * sizeX + x] = go;
                }
            }

            oldSizeX = sizeX;
            oldSizeY = sizeY;
        }

        void InitMapGenerator()
        {
            if(mapGenerator != null)
            {
                mapGenerator.Init();
            }

            oldMapGenerator = mapGenerator;
        }
	
        // Update is called once per frame
        void Update()
        {
            if(sizeX != oldSizeX || sizeY != oldSizeY)
            {
                deleteTiles();
                cloneTiles();
            }

            if(mapGenerator != oldMapGenerator)
            {
                InitMapGenerator();
            }

            if(mapGenerator != null)
            {
                Map = mapGenerator.MasterMap;
            }

            foreach(GameObject go in tiles)
            {
                float x = go.transform.position.x + sizeX / 2.0f - 0.5f;
                float y = go.transform.position.y + sizeY / 2.0f - 0.5f;

                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

                if(x < 0 || x >= Map.sizeX)
                {
                    sr.sprite = null;
                    continue;
                }

                if(y < 0 || y >= Map.sizeY)
                {
                    sr.sprite = null;
                    continue;
                }

                if(spriteMapper != null)
                {
                    sr.sprite = spriteMapper.MapToSprite(Map[(int)x, (int)y]);
                }
            }
        }

        public void BuildRandomJoinTile()
        {
            if(mapGenerator != null)
            {
                mapGenerator.BuildRandomJoinTile();
            }
        }

        public void BuildNextRandomSubMap()
        {   

            if(mapGenerator != null)
            {
                mapGenerator.BuildNextRandomSubMap();
            }
        }

        public void PostProcessMap()
        {
            if(mapGenerator != null)
            {
                mapGenerator.PostProcessMap();
            }
        }

        public void SaveMap()
        {
            if(mapGenerator != null)
            {
                mapGenerator.SaveMap();
            }
        }

        public void LoadMap()
        {
            if(mapGenerator != null)
            {
                mapGenerator.LoadMap();
            }
        }
    }
}