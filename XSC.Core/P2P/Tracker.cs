using System;
using System.Collections.Generic;
using System.Net;
using System.Net.PeerToPeer;
using System.Net.Sockets;
using System.Threading;

namespace XSC.P2P
{
    public class Tracker
    {
        private PeerName peerName = new PeerName("XSC", PeerNameType.Unsecured);

        private List<IPEndPoint> peers = new List<IPEndPoint>();

        private object resolverLock = new object();

        private static PeerNameRegistration peer;

        public ushort Port { get; private set; }

        public List<IPEndPoint> Peers
        {
            get
            {
                lock (resolverLock)
                {
                    return peers;
                }
            }
        }

        public void StartResolving()
        {
            Thread th = new Thread(() =>
            {
                while (true)
                {
                    List<IPEndPoint> res = new List<IPEndPoint>();

                    PeerNameResolver resolver = new PeerNameResolver();
                    PeerNameRecordCollection results = resolver.Resolve(peerName, Cloud.AllLinkLocal);

                    foreach (var peer in results)
                    {
                        foreach (var item in peer.EndPointCollection)
                        {
                            if (item.AddressFamily == AddressFamily.InterNetwork)
                            {
                                res.Add(item);
                                break;
                            }
                        }
                    }

                    lock (resolverLock)
                    {
                        peers = res;
                    }

                    Thread.Sleep(10000);
                }
            });

            th.Start();
        }

        public void StartRegistering()
        {
            Port = GetRandomPort();
            peer = new PeerNameRegistration(peerName, Port, Cloud.AllLinkLocal);
            peer.Start();

            Thread th = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10000);
                    peer.Update();
                }
            });

            th.Start();
        }

        private static ushort GetRandomPort()
        {
            Random r = new Random();

            int port;
            while (true)
            {
                port = r.Next(5000, 60000);

                TcpListener listener = new TcpListener(IPAddress.Any, port);

                try
                {
                    listener.Start();
                    listener.Stop();
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return (ushort)port;
        }
    }
}
