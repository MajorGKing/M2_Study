using GameServer;
using Google.Protobuf.Protocol;
using Server.Data;

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
            SkillData skillData = monsterData.SkillMap[ESkillSlot.Main];
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
                    //TODO 현재스킬의 쿨타임
                    UpdateTick = (int)(Owner.MonsterData.SkillMap[ESkillSlot.Main].Cooltime * 1000);
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
                return _attacker;

            // 범위 밖에서 누가 때린 경우
            if (_attacker != null)
                return _attacker;

            return FindTargetForMonster();
        }

        Creature FindTargetForMonster()
        {
            List<Hero> heroes = Owner.Room.FindAdjacentHeroes(Owner.CellPos, hero =>
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
                int skillRange = Owner.SkillComp.GetNextUseSkillDistance(hero.ObjectId);

                if (dist <= skillRange)
                    return hero;

                List<Vector2Int> path = Owner.Room?.Map.FindPath(Owner, Owner.CellPos, hero.CellPos);
                if (path == null || path.Count < 2 || path.Count > _chaseCellDist)
                    continue;

                return hero;
            }

            return null;
        }

        protected override Vector2Int GetSpawnPos()
        {
            return Owner.SpawnPosition;
        }

        protected override int GetSpawnRange()
        {
            return Owner.SpawnRange;
        }

        public override void Reset()
        {
            base.Reset();
            _target = null;
            _attacker = null;
            _patrolDest = null;
        }
    }
}
