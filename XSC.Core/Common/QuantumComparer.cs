using System.Collections.Generic;

namespace XSC
{
    using Core;

    public class QuantumComparer : IEqualityComparer<Quantum>
    {
        public bool Equals(Quantum x, Quantum y)
        {
            return x.Hash == y.Hash;
        }

        public int GetHashCode(Quantum obj)
        {
            return obj.GetHashCode();
        }
    }
}
