using UnityEngine;

namespace Code
{
    public class MapRenderer : MonoBehaviour
    {
        public GameObject tilePrefab;
        public int sizeX;
        public int sizeY;

        public Sprite solid;
        public Sprite floor;
        public Sprite join;
        public Sprite empty;
        public Sprite liquid;
        public Sprite error;

        //Debug Hack
        public MapGenerator mapGenerator;

        public Map Map
        {
            get;
            set;
        }

        GameObject[] tiles;

        // Use this for initialization
        void Start()
        {
            tiles = new GameObject[sizeX * sizeY];

            for(int i = 0; i < sizeX; i++)
            {
                for(int j = 0; j < sizeY; j++)
                {
                    tiles[i * sizeY + j] = Instantiate(
                        tilePrefab, 
                        new Vector3(i - sizeX / 2.0f + 0.5f, j - sizeY / 2.0f + 0.5f),
                        Quaternion.identity) as GameObject;
                }
            }
        }
	
        // Update is called once per frame
        void Update()
        {
            if(mapGenerator != null)
            {
                Map = mapGenerator.MasterMap;
            }
            foreach(GameObject go in tiles)
            {
                float x = go.transform.position.x + sizeX / 2.0f - 0.5f;
                float y = go.transform.position.y + sizeY / 2.0f - 0.5f;

                if(x < 0 || x >= Map.sizeX)
                {
                    continue;
                    //Debug.LogError("x out of range " + x);
                }

                if(y < 0 || y >= Map.sizeY)
                {
                    continue;
                    //Debug.LogError("y out of range " + y);
                }

                TileType tileType = Map[(int)x, (int)y].TileType;
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

                switch(tileType)
                {
                    case TileType.Empty:
                        sr.sprite = empty;
                        break;
                    case TileType.Floor:
                        sr.sprite = floor;
                        break;
                    case TileType.Solid:
                        sr.sprite = solid;
                        break;
                    case TileType.Join:
                        sr.sprite = join;
                        break;
                    case TileType.Liquid:
                        sr.sprite = liquid;
                        break;
                    default:
                        sr.sprite = error;
                        break;
                }
            }	
        }
    }
}