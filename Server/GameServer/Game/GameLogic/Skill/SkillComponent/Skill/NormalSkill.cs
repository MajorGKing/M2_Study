using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameServer.Game
{
    public class NormalSkill : Skill
    {
        public NormalSkill(int templatedId, Creature owner) : base(templatedId, owner)
        {
        }

        public override bool CanUseSkill(SkillContext skillContext)
        {
            throw new NotImplementedException();
        }

        public override void UseSkill(SkillContext skillContext)
        {
            throw new NotImplementedException();
        }
    }
}
