﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using QueryMaster;

using _11thLauncher.Configuration;

namespace _11thLauncher.Net
{
    static class Servers
    {
        public static List<string> ServerInfo;
        public static List<string> ServerPlayers;
        public static string ServerMods;

        private static readonly IPAddress Address = null;
        private static readonly ushort[] ServerPorts = { 2303, 2323, 2333 }; //Query port = server port + 1

        /// <summary>
        /// Check if the servers are online and call the form to update stattus
        /// </summary>
        public static void CheckServers()
        {
            for (int i = 0; i < ServerPorts.Length; i++)
            {
                bool status;
                string players = "-/-";

                MainWindow.UpdateForm("UpdateStatusBar", new object[] { "Comprobando servidor " + (i + 1) });

                try
                {
                    Server server = ServerQuery.GetServerInstance(EngineType.Source, GetServerIp().ToString(), ServerPorts[i]);
                    ServerInfo info = server.GetInfo();
                    players = info.Players + "/" + info.MaxPlayers;
                    status = true;
                    server.Dispose();
                }
                catch (SocketException)
                {
                    status = false;
                }

                MainWindow.UpdateForm("UpdateServerStatus", new object[] { i, status, players });
            }
        }

        /// <summary>
        /// Check if the server with the given index is online and call the form to update stattus
        /// </summary>
        /// <param name="indexobj">Server index</param>
        public static void CheckServer(object indexobj)
        {
            int index = (int)indexobj;

            MainWindow.UpdateForm("UpdateStatusBar", new object[] { "Comprobando servidor " + (index + 1) });

            bool status;
            string players = "-/-";

            try
            {
                Server server = ServerQuery.GetServerInstance(EngineType.Source, GetServerIp().ToString(), ServerPorts[index]);
                ServerInfo info = server.GetInfo();
                players = info.Players + "/" + info.MaxPlayers;
                status = true;
                server.Dispose();
            }
            catch (SocketException)
            {
                status = false;
            }

            MainWindow.UpdateForm("UpdateServerStatus", new object[] { index, status, players });
        }

        /// <summary>
        /// Query the server with the given index and callback the form to show its information
        /// </summary>
        /// <param name="indexobj">Server index</param>
        public static void QueryServerInfo(object indexobj)
        {
            int index = (int)indexobj;

            ServerInfo = new List<string>();
            ServerPlayers = new List<string>();
            ServerMods = "";

            MainWindow.UpdateForm("UpdateStatusBar", new object[] { "Solicitando información del servidor " + (index + 1) });

            bool exception = false;

            try
            {
                Server server = ServerQuery.GetServerInstance(EngineType.Source, GetServerIp().ToString(), ServerPorts[index]);

                ServerInfo info = server.GetInfo();
                IReadOnlyCollection<Player> players = server.GetPlayers();
                IReadOnlyCollection<Rule> rules = server.GetRules();
                server.Dispose();

                ServerInfo.Add(info.Name);
                ServerInfo.Add(info.Description);
                ServerInfo.Add(info.Ping.ToString());
                ServerInfo.Add(info.Map);
                ServerInfo.Add(info.Players.ToString());
                ServerInfo.Add(info.MaxPlayers.ToString());
                ServerInfo.Add(info.GameVersion);

                foreach (Player p in players)
                {
                    ServerPlayers.Add(p.Name);
                }

                string mods = "";
                foreach (Rule r in rules)
                {
                    mods += r.Value;
                }

                List<string> split = mods.Split(';').ToList();
                split.RemoveAt(split.Count - 1);
                bool skip = false;
                mods = "";
                foreach (string s in split)
                {
                    if (skip)
                    {
                        skip = false;
                    }
                    else
                    {
                        mods += s + "; ";
                        skip = true;
                    }
                }
                ServerMods = mods;

            }
            catch (SocketException)
            {
                ServerInfo = new List<string> { "-", "-", "-", "-", "-", "-", "-" };
                ServerPlayers = new List<string>();
                ServerMods = "-";
                exception = true;
            }

            MainWindow.UpdateForm("UpdateServerInfo", new object[] { index, exception });
        }
        
        /// <summary>
        /// Compare the local game version with the server version and callback the form to show if it doesn't match
        /// </summary>
        public static void CompareServerVersion()
        {
            foreach (ushort serverPort in ServerPorts)
            {
                try
                {
                    Server server = ServerQuery.GetServerInstance(EngineType.Source, GetServerIp().ToString(), serverPort);

                    ServerInfo info = server.GetInfo();
                    server.Dispose();

                    string localVersion = Settings.GameVersion;
                    string remoteVersion = info.GameVersion;

                    //Version mismatch, show in form
                    if (!string.IsNullOrEmpty(localVersion) && !localVersion.Equals(remoteVersion))
                    {
                        MainWindow.UpdateForm("ShowVersionMismatch", new object[] { remoteVersion });
                    }

                    break;
                }
                catch (SocketException){}
            }
        }

        /// <summary>
        /// Resolve and return the IPv4 address of 11thmeu.es
        /// </summary>
        /// <returns>IPv4 address of the server</returns>
        private static IPAddress GetServerIp()
        {
            IPAddress address = null;
            if (Address == null)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry("www.11thmeu.es");

                //Find IPv4 address
                foreach (IPAddress addr in ipHostInfo.AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        address = addr;
                    }
                }
            } else
            {
                //Address was resolved previously, return it directly
                address = Address;
            }

            return address;
        }
    }
}
