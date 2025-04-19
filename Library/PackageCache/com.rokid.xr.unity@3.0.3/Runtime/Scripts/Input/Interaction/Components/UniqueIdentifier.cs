using System;
using System.Collections.Generic;

namespace Rokid.UXR.Interaction
{
    public class UniqueIdentifier
    {
        public int ID { get; private set; }


        private UniqueIdentifier(int identifier)
        {
            ID = identifier;
        }

        private static System.Random Random = new System.Random();
        private static HashSet<int> _identifierSet = new HashSet<int>();

        public static UniqueIdentifier Generate()
        {
            while (true)
            {
                int identifier = Random.Next(Int32.MaxValue);
                if (_identifierSet.Contains(identifier)) continue;
                _identifierSet.Add(identifier);
                return new UniqueIdentifier(identifier);
            }
        }

        public static void Release(UniqueIdentifier identifier)
        {
            _identifierSet.Remove(identifier.ID);
        }
    }
}
