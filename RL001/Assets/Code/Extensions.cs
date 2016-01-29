using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Debug.Assert(list != null);

            for(int i = 0; i < list.Count; ++i)
            {
                int j = UnityEngine.Random.Range(0, list.Count);

                T value = list[j];

                list[j] = list[i];
                list[i] = value;
            }
        }

        public static Vector2 ParseToVector2(this string s)
        {
            Vector2 v = new Vector2();

            Debug.Assert(s != null);

            string[] parts = s.Substring(1, s.Length - 2).Split(',');

            Debug.Assert(parts.Length == 2);

            v.x = float.Parse(parts[0]);
            v.y = float.Parse(parts[1]);

            return v;
        }

        public static Color ParseToColor(this string s)
        {

            Debug.Assert(s != null);

            string[] parts = s.Substring(5, s.Length - 6).Split(',');

            Debug.Assert(parts.Length == 4);

            Color c = new Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));

            return c;
        }
    }
}