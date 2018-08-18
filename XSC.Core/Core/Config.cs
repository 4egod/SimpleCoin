using System;
using System.IO;

namespace XSC.Core
{
    public static class Config
    {
        //using static difficulty (number of zeros in hash)
        public const int StaticDifficulty = 4;

        public const string GenesisHash = "0000000000000000000000000000000000000000000000000000000000000000";
        public const ulong GenesisReward = 100000000000000;
        public const ulong BaseReward = 5000000000;
        public const int DecimalPoint = 100000000;
        public const string Ticker = "XSC";
        public const string BalanceFormat = "#,0.00000000";
        public const string BalanceFormatWithTicker = BalanceFormat + " " + Ticker;
        public const string P2PUri = "net.tcp://{0}:{1}/XSC/";

        static Config()
        {
            if (!Directory.Exists(LogsDirectory))
            {
                Directory.CreateDirectory(LogsDirectory);
            }

            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }

            if (!Directory.Exists(WalletsDirectory))
            {
                Directory.CreateDirectory(WalletsDirectory);
            }
        }

        public static string BaseDirectory => GetBaseDirectory();

        public static string LogsDirectory => BaseDirectory + "Logs\\";

        public static string DataDirectory => BaseDirectory + "Data\\";

        public static string WalletsDirectory => BaseDirectory + "Wallets\\";

        private static string GetBaseDirectory()
        {
            string res = AppDomain.CurrentDomain.BaseDirectory;
            if (res[res.Length - 1] != '\\') res += "\\";
            return res;
        }
    }
}
