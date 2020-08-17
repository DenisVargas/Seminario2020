using System.Collections.Generic;

namespace IA.Utility
{
    public static class RoulleteSelection
    {
        public static int Roullete(IEnumerable<float> list)
        {
            float TotalSum = 0;
            float RandomIndex = UnityEngine.Random.Range(0f, 1f);
            float Sum2 = 0;
            foreach (var Numero in list)
                TotalSum += Numero;
                
            List<float> newValues = new List<float>();
            foreach (var Numero in list)
                newValues.Add(Numero / TotalSum);

            for (int i = 0; i < newValues.Count; i++)
            {
                Sum2 += newValues[i];
                if (Sum2 > RandomIndex) return i;
            }
            return -1;
        }
    }
}
