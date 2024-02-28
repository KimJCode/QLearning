using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AI.Util {

    // a static utility class, which allows us to easily select a 
    // random entry from a given generic list
    static class RandomList<T> {

        public static T RandomEntry(List<T> list) {
            int s = Rand.Next() % list.Count;
            return list[s];          
        }

        static Random Rand = new Random();
    }
}
