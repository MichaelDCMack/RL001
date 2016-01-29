using UnityEngine;

namespace Code
{
    [ExecuteInEditMode]
    public class MapRenderer : MonoBehaviour
    {
        public GameObject tilePrefab;
        public SpriteMapper spriteMapper;
        public int sizeX;
        public int sizeY;

        public bool drawRegions;

        int oldSizeX;
        int oldSizeY;

        CurrentMap currentMap;

        GameObject[] tiles;

        // Use this for initialization
        void Start()
        {
            deleteTiles();
            cloneTiles();
            currentMap = GetComponent<CurrentMap>();
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
	
        // Update is called once per frame
        void Update()
        {
            if(sizeX != oldSizeX || sizeY != oldSizeY)
            {
                deleteTiles();
                cloneTiles();
            }

            foreach(GameObject go in tiles)
            {
                float x = go.transform.position.x + sizeX / 2.0f - 0.5f;
                float y = go.transform.position.y + sizeY / 2.0f - 0.5f;

                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

                if(x < 0 || x >= currentMap.map.size.x)
                {
                    sr.sprite = null;
                    continue;
                }

                if(y < 0 || y >= currentMap.map.size.y)
                {
                    sr.sprite = null;
                    continue;
                }

                if(spriteMapper != null)
                {
                    sr.sprite = spriteMapper.MapToSprite(currentMap.map, (int)x, (int)y);
                }
            }

            if(drawRegions)
            {

                Vector3 offset = new Vector3(-sizeX / 2, -sizeY / 2, 0);

                DrawDebugRectangle(offset,
                                   new Vector3(0, currentMap.map.size.y, 0) + offset,
                                   new Vector3(currentMap.map.size.x, currentMap.map.size.y, 0) + offset,
                                   new Vector3(currentMap.map.size.x, 0, 0) + offset,
                                   Color.blue);

                foreach(Region region in currentMap.map.regions)
                {
                    Vector3 a = new Vector3(region.Anchor.x, region.Anchor.y, 0) + offset;
                    Vector3 h = new Vector3(0, region.Size.y, 0);
                    Vector3 w = new Vector3(region.Size.x, 0, 0);

                    DrawDebugRectangle(a, 
                                       a + h, 
                                       a + h + w, 
                                       a + w, 
                                       region.DebugColor);
                }
            }
        }

        public void DrawDebugRectangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            Debug.DrawLine(p0, p1, color);
            Debug.DrawLine(p1, p2, color);
            Debug.DrawLine(p2, p3, color);
            Debug.DrawLine(p3, p0, color);
        }

        public Vector2 WorldPosToMapPos(Vector3 pos)
        {
            Vector2 r;

            r.x = pos.x + sizeX / 2.0f;
            r.y = pos.y + sizeY / 2.0f;

            return r;
        }
    }
}