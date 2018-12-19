using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XSC.Core
{
    using DB;
    using LiteDB;
    using Newtonsoft.Json;

    internal class TransactionData
    {
        public string Recipient { get; set; }

        public Quantum Reward { get; set; }

        public List<Quantum> Inputs { get; set; }

        public Quantum Output { get; set; }

        public Quantum Change { get; set; }

        public string Key { get; set; }
    }

    [DataContract]
    public class Transaction 
    {
        [JsonConstructor]
        private Transaction()
        {
        }

        public Transaction(byte[] publicKey, string recipient, List<Quantum> inputs, Quantum output)
        {
            Key = publicKey.ToHex();
            Recipient = recipient;
            Inputs = inputs;
            Output = output;

            ulong sum = Quantum.Sum(inputs);
            if (sum != output.Amount)
            {
                Change = new Quantum(sum - output.Amount);
            }

            Hash = Crypto.CalculateHash(ExtractData());
        }

        [BsonId]
        [JsonProperty(Required = Required.Always)]
        [DataMember]
        public string Hash { get; private set; }

        [JsonProperty(Required = Required.Always)]
        [DataMember]
        public string Recipient { get; private set; }

        [JsonProperty(Required = Required.AllowNull)]
        [BsonRef(Database.InputsCollectionName)]
        [DataMember]
        public Quantum Reward { get; private set; }

        [JsonProperty(Required = Required.AllowNull)]
        [BsonRef(Database.InputsCollectionName)]
        [DataMember]
        public List<Quantum> Inputs { get; private set; }

        [JsonProperty(Required = Required.AllowNull)]
        [BsonRef(Database.OutputsCollectionName)]
        [DataMember]
        public Quantum Output { get; private set; }

        [JsonProperty(Required = Required.AllowNull)]
        [BsonRef(Database.InputsCollectionName)]
        [DataMember]
        public Quantum Change { get; private set; }

        [JsonProperty(Required = Required.AllowNull)]
        [DataMember]
        public string Key { get; private set; }

        [JsonProperty(Required = Required.AllowNull)]
        [DataMember]
        public string Signature { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public ulong FromBlock { get; set; }

        public static Transaction CreateGenesisTransaction()
        {
            Transaction res = new Transaction();
            res.Hash = Config.GenesisHash;
            res.Recipient = "PL1xVuL6jWLsqhnZde6mBq";
            res.Reward = new Quantum()
            {
                Hash = Config.GenesisHash,
                Amount = Config.GenesisReward
            };

            return res;
        }

        public static Transaction CreateRewardTransaction(string recipient)
        {
            Transaction res = new Transaction();
            res.Hash = Crypto.GetRandomHash();
            res.Recipient = recipient;
            res.Reward = new Quantum(Config.BaseReward);

            return res;
        }

        public void Sign(byte[] privateKey)
        {
            Signature = Crypto.SignHash(Hash, privateKey);
        }

        public bool Check()
        {
            if (Hash == null) return false;

            if (Recipient == null) return false;

            if (Reward == null && Inputs == null && Output == null && Change == null) return false;

            if (Reward != null)
            {
                if (Inputs != null || Output != null && Change != null) return false;

                if (Hash == Config.GenesisHash)
                {
                    if (Reward.Amount != Config.GenesisReward) return false;
                }
                else
                {
                    if (Reward.Amount != Config.BaseReward) return false;
                }                
            }
            else
            {
                if (Inputs == null || Output == null) return false;

                if (string.IsNullOrEmpty(Key)) return false;

                if (string.IsNullOrEmpty(Signature)) return false;

                if (!Crypto.VerifyHash(Hash, Key, Signature)) return false;
               
            }

            return true;
        }

        internal TransactionData ExtractData()
        {
            TransactionData data = new TransactionData
            {
                Recipient = Recipient,
                Reward = Reward,
                Inputs = Inputs,
                Output = Output,
                Change = Change,
                Key = Key
            };

            return data;
        }
    }
}
