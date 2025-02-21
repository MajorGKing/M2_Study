using GameServer.Game.GameLogic.Quest;
using Google.Protobuf.Protocol;
using Server.Data;

namespace GameServer.Game
{
    public class Quest : IBroadcastEventListener
    {
        public List<QuestTask> QuestTasks { get; private set; } = new List<QuestTask>();

        public QuestInfo Info { get; } = new QuestInfo();
        public QuestData QuestData { get; private set; }
        public Hero Owner { get; set; }

        private QuestTask _questTask;
        public QuestTask CurrentTask
        {
            get { return _questTask; }
            set
            {
                PrevTask = _questTask;
                _questTask = value;
            }
        }

        public QuestTask PrevTask { get; set; }
    }
}
