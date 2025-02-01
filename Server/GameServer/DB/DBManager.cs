using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf.Protocol;
using System.Collections.Concurrent;

namespace GameServer
{
    public class DBJobQueue
    {
        public Queue<Action> _jobQueue { get; } = new Queue<Action>();
        public bool Processing { get; private set; }

        object _lock = new object();

        public void Enqueue(Action job)
        {
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
            }
        }

        public Action TryDequeue()
        {
            lock (_lock)
            {
                if (Processing)
                    return null;

                if (_jobQueue.TryDequeue(out Action result))
                {
                    Processing = true;
                    return result;
                }
            }

            return null;
        }

        public void FinishProcessing()
        {
            lock (_lock)
            {
                Processing = false;
            }
        }
    }

    public partial class DBManager
    {
        public static ConcurrentDictionary<int/*heroDbId*/, DBJobQueue> _jobQueueDic = new ConcurrentDictionary<int, DBJobQueue>();
        public static ConcurrentQueue<int/*heroDbId*/> _executeQueue = new ConcurrentQueue<int>();

        #region JobQuleue
        public static void Push(int heroDbId, Action action)
        {
            if (_jobQueueDic.ContainsKey(heroDbId) == false)
            {
                _jobQueueDic.TryAdd(heroDbId, new DBJobQueue());
                _executeQueue.Enqueue(heroDbId);
            }

            _jobQueueDic[heroDbId].Enqueue(() => { action.Invoke(); FinishProcessing(heroDbId); });
        }

        public static Action TryPop(int heroDbId)
        {
            if (_jobQueueDic.TryGetValue(heroDbId, out DBJobQueue jobQueue) == false)
                return null;

            return jobQueue.TryDequeue();
        }

        public static void Clear(int heroDbId)
        {
            _jobQueueDic.TryRemove(heroDbId, out DBJobQueue jobQueue);
        }

        private static void FinishProcessing(int heroDbId)
        {
            if (_jobQueueDic.TryGetValue(heroDbId, out DBJobQueue jobQueue) == false)
                return;

            jobQueue.FinishProcessing();
        }

        private static bool ContainsKey(int heroDbId)
        {
            return _jobQueueDic.ContainsKey(heroDbId);
        }
        #endregion

        static int _threadCount = 0;

        public static void LaunchDBThreads(int threadCount)
        {
            _threadCount = threadCount;

            for (int i = 0; i < threadCount; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(DBThreadJob));
                t.Name = $"DBThread_{i}";
                t.Start(i);
            }
        }

        static public void DBThreadJob(object arg)
        {
            int threadId = (int)arg;

            while (true)
            {
                if (_executeQueue.TryDequeue(out int heroDbId) == false)
                    continue;

                if (ContainsKey(heroDbId) == false)
                    continue;

                Action action = TryPop(heroDbId);
                if (action != null)
                    action.Invoke();

                _executeQueue.Enqueue(heroDbId);

                Thread.Sleep(0);
            }
        }

    //    public static DBManager Instance { get; } = new DBManager();

    //    public static List<HeroDb> LoadHeroDb(long accountDbId)
    //    {
    //        using(GameDbContext db = new GameDbContext())
    //        {
    //            List<HeroDb> heroDbs = db.Heroes.Where(h => h.AccountDbId == accountDbId).Include(h => h.Items).ToList();
    //            return heroDbs;
    //        }
    //    }

    //    public static HeroDb CreateHeroDb(long accountDbId, C_CreateHeroReq reqPacket)
    //    {
    //        using(GameDbContext db = new GameDbContext())
    //        {
    //            HeroDb heroDb = db.Heroes.Where(h => h.Name == reqPacket.Name).FirstOrDefault();
    //            if (heroDb != null)
    //                return null;

    //            heroDb = new HeroDb()
    //            {
    //                AccountDbId = accountDbId,
    //                Name = reqPacket.Name,
    //                Gender = reqPacket.Gender,
    //                ClassType = reqPacket.ClassType,
    //                Level = 1,
    //                Hp = -1,
    //                Mp = -1,
    //            };

    //            db.Heroes.Add(heroDb);

    //            if (db.SaveChangesEx())
    //                return heroDb;

    //            return null;
    //        }
    //    }

    //    public static bool DeleteHeroDb(int heroDbId)
    //    {
    //        using (GameDbContext db = new GameDbContext())
    //        {
    //            HeroDb heroDb = db.Heroes.Where(h => h.HeroDbId == heroDbId).FirstOrDefault();
    //            if (heroDb == null)
    //                return false;

    //            db.Heroes.Remove(heroDb);

    //            if (db.SaveChangesEx())
    //                return true;
    //        }

    //        return true;
    //    }
    }
}
