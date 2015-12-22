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

        // Use this for initialization
        void OnEnable()
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
            }
            else
            {
                map = new Map(10, 10);
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