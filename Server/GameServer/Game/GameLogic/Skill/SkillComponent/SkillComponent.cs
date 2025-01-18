using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using System.Xml;

namespace GameServer.Game
{
    public class SkillComponent
    {
        public Creature Owner { get; private set; }
        public EGameObjectType OwnerType { get { return Owner.ObjectType; } }

        Dictionary<int/*templateId*/, Skill> _skills = new Dictionary<int, Skill>();

        Random _rand = new Random();

        public SkillComponent(Creature owner)
        {
            Owner = owner;
        }

        #region 스킬 사용
        public Skill GetSkill(int templatedId)
        {
            //if(Owner.ObjectType == EGameObjectType.Hero)
            //    Console.WriteLine($"Skill : {templatedId}" );

            if(_skills.TryGetValue(templatedId, out Skill skill))
                return skill;

            return null;
        }

        public bool CanUseSkill(int templatedId)
        {
            Skill skill = GetSkill(templatedId);
            if (skill == null)
                return false;

            return skill.CheckCooltimeAndState();
        }

        public void UseSkill(int templateId, int targetId)
        {
            Skill skill = GetSkill(templateId);
            if (skill == null)
                return;

            skill.UseSkill(targetId);
        }
        #endregion

        #region 스킬 등록 & 쿨타임 관리
        public bool RegisterSkill(int templatedId)
        {
            if (_skills.ContainsKey(templatedId))
                return false;
            if(DataManager.SkillDict.TryGetValue(templatedId, out SkillData skillData) == false)
                return false;

            Skill skill = null;
            if (skillData.Projectile != null)
                skill = new ProjectileSkill(templatedId, Owner);
            else
                skill = new NormalSkill(templatedId, Owner);

            _skills.Add(templatedId, skill);
            return true;
        }

        public bool CheckCooltime(int templateId)
        {
            Skill skill = GetSkill(templateId);
            if (skill == null)
                return false;

            return skill.CheckCooltimeAndState(); // TODO : CheckCooltime
        }

        public List<SkillCoolTime> GetRemainingTicks()
        {
            List<SkillCoolTime> cooltimes = new List<SkillCoolTime>();

            foreach(Skill skill in _skills.Values)
            {
                cooltimes.Add(new SkillCoolTime()
                {
                    SkillId = skill.TemplateId,
                    RemainingTicks = (int)skill.GetRemainingCooltimeInTicks()
                });
            }

            return cooltimes;
        }

        public void UpdateCooltime(int templateId)
        {
            Skill skill = GetSkill(templateId);
            if (skill == null)
                return;

            skill.UpdateCooltime();
        }
        #endregion

        public void Clear()
        {
            _skills.Clear();
        }
    }
}
