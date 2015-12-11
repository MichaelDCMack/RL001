using System.Collections.Generic;

namespace Code
{
    static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            for(int i = 0; i < list.Count; ++i)
            {
                int j = UnityEngine.Random.Range(0, list.Count);
                T value = list[j];
                list[j] = list[i];
                list[i] = value;
            }
        }
    }
}
