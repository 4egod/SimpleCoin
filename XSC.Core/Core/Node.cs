using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace XSC.Core
{
    using DB;
    using P2P;
    using Properties;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Node : IPeerService
    {
        //private bool isSynchronized;
        private List<Block> pendingBlocks = new List<Block>();
        
        public Node()
        {
            Wallet = Wallet.Generate();
            Logger.Start(Wallet.Address);
            Logger.WriteLine(Resources.LogInitialized, Logger.Path);
            Wallet.Export();
            Logger.WriteLine(Resources.WalletCreated, Config.WalletsDirectory + Wallet.Address + Wallet.Extension);
            Database = Database.Create(Wallet.Address);
        }

        public Node(string address)
        {
            Wallet = Wallet.Import(address);
            Logger.Start(Wallet.Address);
            Logger.WriteLine(Resources.LogInitialized, Logger.Path);
            Logger.WriteLine(Resources.WalletOpened, Config.WalletsDirectory + Wallet.Address + Wallet.Extension);
            Database = Database.Open(Wallet.Address);
        }

        public Node(byte[] privateKey)
        {
            Wallet = Wallet.Import(privateKey);
            Logger.Start(Wallet.Address);
            Logger.WriteLine(Resources.LogInitialized, Logger.Path);
            Wallet.Export();
            Logger.WriteLine(Resources.WalletCreated, Config.WalletsDirectory + Wallet.Address + Wallet.Extension);
            Database = Database.Create(Wallet.Address);
        }

        public bool IsSynchronized { get; set; }

        public Wallet Wallet { get; private set; }

        public Database Database { get; private set; }

        public Tracker Tracker { get; private set; } = new Tracker();

        public SortedList<string, Transaction> TXPool { get; private set; } = new SortedList<string, Transaction>();

        public void Start()
        {
            Tracker.StartRegistering();

            StartPeerService();

            Tracker.StartResolving();

            StartSynchronizing();

            StartMining();

            //Emulate();
        }

        public void StartMining()
        {
            Thread th = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    try
                    {
                        Block b = GetBlockTemplate(Wallet.Address);
                        Crypto.CalculateHash(b);
                        Broadcast(b);
                        Process(b);
                    }
                    catch (Exception ex) 
                    {
                        Logger.WriteLine(Resources.Exception, ex.GetType(), ex.Message);
                    }
                }
            });

            th.Start();

            Logger.WriteLine(Resources.NodeStartMining);
        }

        public Balance GetBalance()
        {
            Balance res = new Balance();
            ulong available = Quantum.Sum(GetUnspents());
            ulong locked = Quantum.Sum(GetLocked());

            res.Available = (decimal)(available / Config.DecimalPoint);
            res.Locked = (decimal)(locked / Config.DecimalPoint);
            return res;
        }

        public List<Quantum> GetUnspents()
        {
            List<Quantum> res = new List<Quantum>();
            var inputs = Database.GetInputs();
            bool valid;
            foreach (var input in inputs)
            {
                valid = true;

                foreach (var tx in TXPool)
                {
                    if (tx.Value.Inputs.AsQueryable<Quantum>().Contains<Quantum>(input, new QuantumComparer()))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid) res.Add(input);
            }

            return res;
        }

        public List<Quantum> GetLocked()
        {
            List<Quantum> res = new List<Quantum>();
            foreach (var tx in TXPool)
            {
                if (tx.Value.Recipient == Wallet.Address)
                {
                    if (tx.Value.Reward != null)
                    {
                        res.Add(tx.Value.Reward);
                    }

                    if (tx.Value.Output != null)
                    {
                        res.Add(tx.Value.Output);
                    }
                    
                }

                if (Wallet.GetAddress(tx.Value.Key) == Wallet.Address)
                {
                    if (tx.Value.Change != null)
                    {
                        res.Add(tx.Value.Change);
                    }
                }
            }

            return res;
        }

        public Transaction Transfer(string recipientAddress, decimal amount)
        {
            List<Quantum> validInputs = new List<Quantum>();

            var inputs = Database.GetInputs();
            var output = new Quantum(amount);

            ulong sum = 0;
            bool valid;
            foreach (var input in inputs)
            {
                valid = true;

                foreach (var tx in TXPool)
                {
                    if (tx.Value.Inputs.AsQueryable<Quantum>().Contains<Quantum>(input, new QuantumComparer()))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    validInputs.Add(input);
                    sum = Quantum.Sum(validInputs);
                    if (sum >= output.Amount) break;
                }
            }

            if (sum < output.Amount)
            {
                throw new Exception(Resources.ExceptionNotEnoughFunds);
            }

            Transaction transaction = new Transaction(Wallet.PublicKey, recipientAddress,
                validInputs, output);
            transaction.Sign(Wallet.PrivateKey);

            //txPool.Add(transaction.Hash, transaction);
            Broadcast(transaction);
            Process(transaction);

            return transaction;
        }

        public void Process(Block block)
        {
            if (block == null) return;
            if (!block.Check()) return;
            if (!CheckSpends(block)) return;
            if (block.PreviousHash != Database.LastBlock.Hash)
            {
                pendingBlocks.Add(block);
                return;
            }

            foreach (var item in block.Transactions)
            {
                TXPool.Remove(item.Hash);
            }

            Database.Add(block);

            Logger.WriteLine($"BLOCK {block.Height}+ ({block.Hash}) by {block.Transactions.Find(x => x.Reward != null).Recipient}");
        }

        public void Process(Transaction transaction)
        {
            //Logger.WriteLine(transaction.Hash + " added to tx pool...");
            //TODO Check TX

            if (transaction.Recipient == Wallet.Address)
            {
                decimal amount = 0;

                if (transaction.Reward != null)
                {
                    amount = (decimal)(transaction.Reward.Amount / Config.DecimalPoint);
                }

                if (transaction.Output != null)
                {
                    amount = (decimal)(transaction.Output.Amount / Config.DecimalPoint);
                }

                Logger.WriteLine($"TX +{amount.ToString(Config.BalanceFormatWithTicker)} ({transaction.Hash})");
            }

            TXPool.Add(transaction.Hash, transaction);
        }

        public ulong GetHeight()
        {
            try
            {
                return Database.Height;
            }
            catch (Exception ex)
            {
                Logger.WriteLine(Resources.Exception, ex.GetType(), ex.Message);
            }

            return 0;
        }

        public Block GetBlock(ulong height)
        {
            try
            {
                return Database.GetBlock(height);
            }
            catch (Exception ex)
            {
                Logger.WriteLine(Resources.Exception, ex.GetType(), ex.Message);
            }

            return null;
        }

        public void Broadcast(Block block)
        {
            Thread th = new Thread(() =>
            {
                foreach (var peer in Tracker.Peers)
                {
                    try
                    {
                        CreatePeerService(peer).Process(block);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLine(Resources.P2PException, $"[{peer}] " + ex.Message);
                    }
                    
                }
            });

            th.Start();
        }

        public void Broadcast(Transaction transaction)
        {
            Thread th = new Thread(() =>
            {
                foreach (var peer in Tracker.Peers)
                {
                    try
                    {
                        CreatePeerService(peer).Process(transaction);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLine(Resources.P2PException, $"[{peer}] " + ex.Message);
                    }

                }
            });

            th.Start();
        }

        private void StartSynchronizing()
        {
            Logger.WriteLine(Resources.NodeStartSynchronizing);

            bool sended = false;
            while (Tracker.Peers.Count() == 0)
            {
                if (!sended)
                {
                    Logger.WriteLine(Resources.NodeWaitingPeers);
                    sended = true;
                }

                Thread.Sleep(500);
            }

            ulong maxHeight = GetHeight();
            ulong selectedHeight = 0;
            IPEndPoint selectedPeer = null;
            IPeerService service;
            foreach (var peer in Tracker.Peers)
            {
                service = CreatePeerService(peer);
                ulong height = service.GetHeight();
                if (height > maxHeight)
                {
                    selectedPeer = peer;
                    selectedHeight = height;
                }
            }

            if (selectedPeer != null)
            {
                service = CreatePeerService(selectedPeer);
                ulong from = GetHeight() + 1;
                for (ulong i = from; i <= selectedHeight; i++)
                {
                    //if (i >= 100) break;
                    Block block = service.GetBlock(i);
                    Process(block);
                }
            }

            if (pendingBlocks.Count() != 0)
            {
                foreach (var block in pendingBlocks)
                {
                    Process(block);
                }
            }

            Logger.WriteLine(Resources.NodeSynchronized);
        }

        public Block GetBlockTemplate(string minerAddress)
        {
            Block res = new Block(minerAddress, Database.LastBlock, TXPool.Values);
            return res;
        }

        public void CreateStarter()
        {
            string s = $"xsc.exe {Wallet.Address}";
            File.WriteAllText(Config.BaseDirectory + Wallet.Address + ".cmd", s);
        }

        private bool CheckSpends(Block block)
        {
            return true;
        }

        private void StartPeerService()
        {
            Uri uri = new Uri(string.Format(Config.P2PUri, "0.0.0.0", Tracker.Port));
            ServiceHost host = new ServiceHost(this, uri);

            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
            host.Description.Behaviors.Add(behavior);
            host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            host.AddServiceEndpoint(typeof(IPeerService), binding, uri);
            host.Open();
            Logger.WriteLine(Resources.NodeStartService, uri);
        }

        private IPeerService CreatePeerService(IPEndPoint peer)
        {
            EndpointAddress ep = new EndpointAddress(string.Format(Config.P2PUri, peer.Address, peer.Port));
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            var factory = new ChannelFactory<IPeerService>(binding);
            IPeerService peerService = factory.CreateChannel(ep);
            return peerService;
        }

        private void Emulate()
        {
            Thread th = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var tx = Transfer(Wallet.Address, 10);
                        Logger.WriteLine($"TX -{10.ToString(Config.BalanceFormatWithTicker)} ({tx.Hash})");
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLine(Resources.Exception, ex.GetType(), ex.Message);
                    }
                }
            });

            th.Start();

            Logger.WriteLine("Emualting");
        }
    }
}
