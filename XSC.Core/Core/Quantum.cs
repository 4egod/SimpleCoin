using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XSC.Core
{
    using LiteDB;

    public class Quantum
    {
        public Quantum()
        {
            Hash = Crypto.GetRandomHash();
        }

        public Quantum(ulong amount) : this()
        {
            Amount = amount;
        }

        public Quantum(decimal amount) : this()
        {
            Amount = (ulong)(amount * Config.DecimalPoint);
        }

        [BsonId]
        [DataMember]
        public string Hash { get; set; }

        [DataMember]
        public ulong Amount { get; set; }

        [DataMember]
        public Quantum Output { get; set; }

        public static ulong Sum(IEnumerable<Quantum> values)
        {
            ulong res = 0;
            foreach (var item in values)
            {
                res += item.Amount;
            }

            return res;
        }
    }
}
