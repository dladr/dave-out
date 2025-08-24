using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public static class HelperFunctions
    {
        public static string IntToSymbol(int val, char symbol)
        {
            return val <= 0 ? "" : new string(symbol, val);
        }

        public static bool RandomBool()
        {
            return Random.Range(0, 1) == 1;
        }

        public static T RandomObject<T>(params T[] obs)
        {
            if (obs == null || !obs.Any())
                throw new Exception("You have to give at least one option for it to choose a random one.");
            return obs.GetRandom();
        }
    }
}