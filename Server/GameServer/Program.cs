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
using GameServer;
using ServerCore;
using Microsoft.Data.SqlClient;

namespace Server
{
    // 1. Recv (N개)     서빙
    // 2. GameLogic (1)  요리사
    // 3. Send (1개)     서빙
    class Program
    {
        static Listener _listener = new Listener();
        static Connector _connector = new Connector();

        static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            //// TEMP 방 하나 파두기
            //GameLogic.Instance.Push(() =>
            //{
            //    GameLogic.Instance.Add(1);
            //});

            IPAddress ipAddr = IPAddress.Parse(ConfigManager.Config.ip);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, ConfigManager.Config.port);
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Console.WriteLine("Listening...");

			// GameLogic
			const int GameThreadCount = 1;
			GameLogic.LaunchGameThreads(GameThreadCount);

			// DB
			const int DbThreadCount = 1;
			DBManager.LaunchDBThreads(DbThreadCount);

            //TestDatabaseConnection();

            // MainThread
            GameLogic.FlushMainThreadJobs();
        }

        public static bool TestDatabaseConnection()
        {
            try
            {
                Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
                Console.WriteLine($"Connection String: {ConfigManager.Config.connectionString}");
                Console.WriteLine($"Current User: {Environment.UserName}");

                using (var connection = new SqlConnection(ConfigManager.Config.connectionString))
                {
                    Console.WriteLine("Created connection object");

                    connection.Open();
                    Console.WriteLine("Opened connection");

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT DB_NAME()";
                        var result = command.ExecuteScalar();
                        Console.WriteLine($"Connected to database: {result}");
                    }

                    return true;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error Number: {ex.Number}");
                Console.WriteLine($"SQL Error State: {ex.State}");
                Console.WriteLine($"SQL Error Message: {ex.Message}");
                Console.WriteLine($"SQL Error Source: {ex.Source}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }


}
