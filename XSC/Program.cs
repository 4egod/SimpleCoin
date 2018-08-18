using System;

namespace XSC
{
    using Core;
    using DB;
    using Properties;

    class Program
    {
        static Node node;

        static void Main(string[] args)
        {
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("ru");
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru");

            Console.WindowWidth = 125;
            Console.WriteLine();

            try
            {
                if (args.Length == 0)
                {
                    node = new Node();
                    node.CreateStarter();
                    node.Start();
                }

                if (args.Length == 1)
                {
                    if (args[0].Contains(Database.Extension))
                    {
                        args[0].Replace(Database.Extension, "");
                    }

                    bool started = false;

                    if (args[0].Length == 22)
                    {
                        node = new Node(args[0]);
                        node.Start();
                        started = true;
                    }

                    if (args[0].Length == 208)
                    {
                        node = new Node(args[0].FromHex());
                        node.CreateStarter();
                        node.Start();
                        started = true;
                    }

                    if (!started)
                    {
                        string s = string.Format(Resources.ArgumentException, args[0]);
                        throw new ArgumentException(s);
                    }

                }

                StartShell();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(Resources.Exception, ex.GetType(), ex.Message);
                //Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key for exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        static void StartShell()
        {
            Console.WriteLine();
            Console.WriteLine(Resources.ShellWelcome);

            while (true)
            {
                string cmd = Console.ReadLine();

                try
                {
                    switch (cmd)
                    {
                        case "help": Console.WriteLine("\n" + Resources.ShellHelp + "\n"); break;
                        case "exit": Environment.Exit(0); break;

                        case "address": Console.WriteLine(node.Wallet.ToJson()); break;
                        case "balance":
                            var balance = node.GetBalance();
                            Console.WriteLine($"Confirmed: {balance.Available.ToString(Config.BalanceFormatWithTicker)}," +
                                $" Locked: {balance.Locked.ToString(Config.BalanceFormatWithTicker)}, " +
                                $"Total: {balance.Total.ToString(Config.BalanceFormatWithTicker)}");
                            break;
    
                        case "geth": Console.WriteLine(node.Database.Height); break;
                        case "getp":
                            var pl = node.Tracker.Peers;
                            foreach (var item in pl)
                            {
                                Console.WriteLine(item.ToString());
                            }
                            break;
                        case "getm":
                            var tl = node.TXPool.Values;
                            foreach (var item in tl)
                            {
                                Console.WriteLine(item.Hash);
                            }
                            break;
                        default: break;
                    }

                    if (cmd.Contains("gett"))
                    {
                        cmd = cmd.Replace("gett ", "");
                        if (cmd.Length != 64)
                        {
                            throw new ArgumentException(cmd);
                        }
                        else Console.WriteLine(node.Database.GetTransaction(cmd).ToJson());
                    }

                    if (cmd.Contains("getb"))
                    {
                        cmd = cmd.Replace("getb ", "");
                        if (cmd.Length != 64)
                        {
                            if (!ulong.TryParse(cmd, out ulong height))
                            {
                                throw new ArgumentException(cmd);
                            }
                            else Console.WriteLine(node.Database.GetBlock(height).ToJson());
                            
                        }
                        else Console.WriteLine(node.Database.GetBlock(cmd).ToJson());
                    }

                    if (cmd.Contains("transfer"))
                    {
                        cmd = cmd.Replace("transfer ", "");
                        string address = cmd.Substring(0, 22);
                        cmd = cmd.Replace(address + " ", "");

                        if (!decimal.TryParse(cmd, out decimal amount))
                        {
                            throw new ArgumentException(cmd);
                        }
                        else
                        {
                            Transaction tx = node.Transfer(address, amount);
                            Logger.WriteLine($"TX -{amount.ToString(Config.BalanceFormatWithTicker)} ({tx.Hash})");
                        }
                            
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLine($"Exception ({ex.GetType()}): {ex.Message}");
                }
            }

        }
    }
}
