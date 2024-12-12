using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Server.Data;
using Server.Game;
using ServerCore;

namespace Server
{
    // 1. Recv (N개)     서빙
    // 2. GameLogic (1)  요리사
    // 3. Send (1개)     서빙
    class Program
    {
        static Listener _listener = new Listener();
        static Connector _connector = new Connector();

        static void GameLogicTask()
        {
            while (true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0);
            }
        }

        static void GameDbTask()
        {
            while (true)
            {
                DBManager.Instance.Flush();
                Thread.Sleep(100);
            }
        }

        static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            IPAddress ipAddr = IPAddress.Parse(ConfigManager.Config.ip);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, ConfigManager.Config.port);
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Console.WriteLine("Listening...");

            // GameDbTask
            {
                Thread t = new Thread(GameDbTask);
                t.Name = "GameDB";
                t.Start();
            }

            // DB Test
            DBManager.Instance.Push(DBManager.TestDB);

            // GameLogic
            Thread.CurrentThread.Name = "GameLogic";
            GameLogicTask();
        }
    }
}
