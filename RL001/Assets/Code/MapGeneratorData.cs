using UnityEngine;
using System;

namespace Code
{
    [Serializable]
    public class MapGeneratorData
    {
        public TextAsset textAsset;
        public bool rotate;
        public bool mirror;
        public int maxTimesUsable;

        public Map[] Maps
        {
            get;
            protected set;
        }

        public int TimesUsed
        {
            get;
            set;
        }

        public bool IsUsable()
        {
            return maxTimesUsable == -1 || TimesUsed < maxTimesUsable;
        }

        public void LoadMap()
        {
            int numberOfMaps = 1;
            int index = 0;

            if(rotate)
                numberOfMaps *= 4;
            if(mirror)
                numberOfMaps *= 2;

            Maps = new Map[numberOfMaps];

            Maps[index] = new Map();
            Maps[index].ReadGlyphs(textAsset.text);
            ++index;

            if(rotate)
            {
                for(int i = 0; i < 3; ++i)
                {
                    Maps[index] = new Map(Maps[index - 1]);
                    Maps[index].Rotate();
                    ++index;
                }
            }

            if(mirror)
            {
                int numberMapsToMirror = index;
                for(int i = 0; i < numberMapsToMirror; ++i)
                {
                    Maps[index] = new Map(Maps[i]);
                    Maps[index].Mirror();
                    ++index;
                }
            }
        }
    }
}
