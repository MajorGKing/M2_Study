using GameServer;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public class MonsterAIController : FSMController<Monster, Creature>
    {
        public MonsterAIController(Monster owner) : base(owner)
        {
            DataManager.MonsterDict.TryGetValue(owner.TemplateId, out MonsterData monsterData);

            _searchCellDist = monsterData.SearchCellDist;
            _chaseCellDist = monsterData.ChaseCellDist;
            _patrolCellDist = monsterData.PatrolCellDist;

            // 
            //SkillData skillData = monsterData.MainSkill;
            SkillData skillData = monsterData.SkillMap[ESkillSlot.Main];
            _mainSkillRange = skillData.SkillRange;
            _skillTemplateId = skillData.TemplateId;
        }

        public override void SetState(EObjectState state)
        {
            base.SetState(state);

            // TODO : 틱 조절.
            switch (state)
            {
                case EObjectState.Idle:
                    UpdateTick = 1000;
                    break;
                case EObjectState.Move:
                    float speed = Owner.MoveSpeed;
                    float distance = Owner.GetActualDistance();
                    float time = distance / speed;
                    UpdateTick = (int)(time * 1000);
                    break;
                case EObjectState.Skill:
                    UpdateTick = 1000;
                    break;
                case EObjectState.Dead:
                    UpdateTick = 1000;
                    break;
            }
        }

        protected override Creature FindTarget()
        {
            if (Owner.Room == null)
                return null;

            // 비선공몹은 검색X
            if (Owner.MonsterData.IsAggressive == false)
                return _target;

            return FindTargetForMonster();
        }

        Creature FindTargetForMonster()
        {
            List<Hero> heroes = Owner.Room.FindAdjacents<Hero>(Owner.CellPos, hero =>
            {
                if (hero.IsValid() == false)
                    return false;

                return hero.GetDistance(Owner) <= _searchCellDist;
            });

            heroes.Sort((a, b) =>
            {
                int aDist = a.GetDistance(Owner);
                int bDist = b.GetDistance(Owner);
                return aDist - bDist;
            });

            foreach (Hero hero in heroes)
            {
                // 기본 스킬 사용 가능하면 그냥 그 타겟으로 설정
                int dist = hero.GetDistance(Owner);
                if (dist <= _mainSkillRange)
                    return hero;

                List<Vector2Int> path = Owner.Room?.Map.FindPath(Owner, Owner.CellPos, hero.CellPos);
                if (path == null || path.Count < 2 || path.Count > _chaseCellDist)
                    continue;

                return hero;
            }

            return null;
        }
    }
}
