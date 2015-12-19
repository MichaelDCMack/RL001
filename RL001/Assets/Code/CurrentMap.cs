using UnityEngine;

namespace Code
{
    [ExecuteInEditMode]
    public class CurrentMap : MonoBehaviour
    {
        public MapGenerator generator;

        public TextAsset asset;

        public Map map;

        //just for tracking, hacky.
        MapGenerator oldGenerator;
        TextAsset oldAsset;

        //this feels tacky/hacky ... for resizing in the custom editor.  Should live in the custom editor.
        public int sizeX;
        public int sizeY;

        // Use this for initialization
        void Start()
        {
            LoadMap();

            InitGenerator();
        }

        void LoadMap()
        {
            if(asset != null)
            {
                map = new Map();
                map.ReadGlyphs(asset.text);
                sizeX = map.sizeX;
                sizeY = map.sizeY;

            }
            else
            {
                map = new Map(sizeX, sizeY);
            }

            map.FindLinkPoints();

            oldAsset = asset;
        }

        void InitGenerator()
        {
            if(generator != null)
            {
                generator.Init();
            }

            oldGenerator = generator;
        }

        void OnValidate()
        {
            if(oldAsset != asset)
            {
                LoadMap();
            }

            if(oldGenerator != generator)
            {
                InitGenerator();
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}