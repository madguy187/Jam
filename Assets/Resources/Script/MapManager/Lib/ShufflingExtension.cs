using System;
using System.Collections.Generic;
using System.Linq;

namespace Map 
{
    /// Provides extension methods for shuffling, selecting random elements, and retrieving elements from list
    /// Used here to randomize node selection and map layout
    /// Reference: http://stackoverflow.com/questions/273313/randomize-a-listt/1262619#1262619 
    public static class ShufflingExtension 
    {
        private static System.Random rng = new System.Random();

        /// Shuffles the elements of the list in place using the Fisher-Yates algorithm
        public static void Shuffle<T>(this IList<T> list) 
        {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// Returns a random element from the list
        public static T Random<T>(this IList<T> list) {
            return list[rng.Next(list.Count)];
        }

        /// Returns the last element of the list
        public static T Last<T>(this IList<T> list) 
        {
            return list[list.Count - 1];
        }

        /// Returns a list containing a specified number of random elements from the original list.
        /// If the requested count exceeds the list size, all elements are returned in random order
        public static List<T> GetRandomElements<T>(this List<T> list, int elementsCount) 
        {
            return list.OrderBy(arg => Guid.NewGuid()).Take(list.Count < elementsCount ? list.Count : elementsCount)
                .ToList();
        }
    }
}
