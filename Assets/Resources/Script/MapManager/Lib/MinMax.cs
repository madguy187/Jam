using UnityEngine;

namespace Map 
{
    /// Represents a range between two float values and provides a method to get a 
    /// random value within that range
    [System.Serializable]
    public class FloatMinMax 
    {
        public float min;
        public float max;

        public float GetValue() 
        {
            return Random.Range(min, max);
        }
    }
}

namespace Map 
{
    ///  Represents a range between two integer values and provides a method to get a 
    ///  random value within that range
    [System.Serializable]
    public class IntMinMax 
    {
        public int min;
        public int max;

        public int GetValue() 
        {
            return Random.Range(min, max + 1);
        }
    }
}
