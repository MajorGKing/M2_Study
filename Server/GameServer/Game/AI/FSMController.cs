using GameServer;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public abstract class FSMController<OwerType, TargetType> : BaseAIController<OwerType> where OwerType : Creature where TargetType : BaseObject
    {
        protected int _searchCellDist = 5;
        protected int _chaseCellDist = 10;
        protected int _patrolCellDist = 5;

        protected TargetType _target;
        protected Vector2Int? _patrolDest;
        protected Random _rand = new Random();

        protected int _skillTemplateId = 1;
        protected int _mainSkillRange = 1;

        public FSMController(OwerType owner) : base(owner)
        {
        }

        protected abstract TargetType FindTarget();

        protected override void UpdateIdle()
        {
            _target = FindTarget();

            // 1. 주변에 타겟이 있으면 추적.
            if(_target != null)
            {
                _patrolDest = null;
                SetState(EObjectState.Move);
                return;
            }

            // 2. 타겟이 없으면, 주기적으로 주변 정찰.
            int randValue = _rand.Next(0,5);
            if(randValue == 0 && _patrolCellDist > 0)
            {
                _patrolDest = GetPatrolPos();
                if(_patrolDest != null)
                {
                    SetState(EObjectState.Move);
                    return;
                }
            }
        }

        protected override void UpdateMoving()
        {
            // 1. 정찰 도중에도 타겟 탐색.
            if (_patrolDest.HasValue)
                _target = FindTarget();

            // 2. 타겟 추적 모드.
            if (_target != null)
            {
                _patrolDest = null;

                // 2-1. 따라갈 수 있는 거리인지 확인.
                int dist = Owner.GetDistance(_target);
                if(dist == 0 || dist > _chaseCellDist)
                {
                    _target = null;
                    SetState(EObjectState.Idle);
                    return;
                }

                // 2-2. 거리가 멀면 이동.
                if (dist > _mainSkillRange)
                {
                    Console.WriteLine("Find Path and move");
                    FindPathAndMove(_target.CellPos);
                    return;
                }

                // 2-3. 스킬로 넘어감.
                {
                    Console.WriteLine("Change Skill");
                    SetState(EObjectState.Skill);
                    return;
                }
            }
            else if (_patrolDest.HasValue)
            {
                // 3. 정찰 모드.
                FindPathAndMove(_patrolDest.Value);
                return;
            }
            else
            {
                // 4. 타겟도 없고 정찰도 아니면 이동 상태 종료.
                SetState(EObjectState.Idle);
                return;
            }
        }

        bool FindPathAndMove(Vector2Int destPos)
        {
            
            // 1. 길찾기.
            List<Vector2Int> path = Owner.Room?.Map.FindPath(Owner, Owner.CellPos, destPos);

            // 2. 길을 못찾았거나 너무 멀면 포기.
            if(path == null || path.Count < 2 || path.Count > _chaseCellDist)
            {
                _patrolDest = null;
                _target = null;
                SetState(EObjectState.Idle);
                return false;
            }

            Console.WriteLine($"{Owner.ObjectId} : Do Find Path {path.Count}");

            // 3. 이동 실행.
            Owner.Room.Map.ApplyMove(Owner, path[1]);
            Owner.BroadcastMove();

            return true;
        }

        protected override void UpdateSkill()
        {
            // 1. 유효한 타겟인지.
            if(_target.IsValid() == false)
            {
                _target = null;
                SetState(EObjectState.Move);
                return;
            }

            // 2. 거리가 충분히 가까운지.
            int dist = Owner.GetDistance(_target);
            if(dist > _mainSkillRange)
            {
                // 타겟은 날리지 말고 그냥 따라가게 만든다
                SetState(EObjectState.Move);
                return;
            }

            // 3. 스킬 사용.
            {
                Vector2Int cellPos = Owner.GetClosestBodyCellPointToTarget(_target);

                SkillContext context = new SkillContext();
                context.PosX = cellPos.x;
                context.PosY = cellPos.y;
                context.TargetId = _target.ObjectId;

                Owner.Room?.UseSkill(Owner, _skillTemplateId, context);
            }
        }

        protected override void UpdateDead()
        {
        }

        public override void OnDamaged(BaseObject attacker, float damage)
        {
            attacker = attacker.GetOwner();

            base.OnDamaged(attacker, damage);

            if(Owner.State == EObjectState.Idle)
            {
                if(attacker is TargetType)
                    _target = (TargetType)attacker;

                SetState(EObjectState.Move);
            }
            else if(Owner.State == EObjectState.Move)
            {
                if(attacker is TargetType)
                    _target = (TargetType)attacker;
            }
        }

        public override void OnDead(BaseObject attacker)
        {
            // AI Job 취소
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }
        }

        // TODO : 더 효율적으로 개선.
        protected virtual Vector2Int? GetPatrolPos()
        {
            if (Owner.Room == null)
                return null;

            Vector2Int patrolPos = Owner.CellPos;
            for (int i = 0; i < 10; i++)
            {
                patrolPos.x += _rand.Next(-_patrolCellDist, _patrolCellDist + 1);
                patrolPos.y += _rand.Next(-_patrolCellDist, _patrolCellDist + 1);
                if (Owner.Room.Map.CanGo(Owner, patrolPos, checkObjects: true))
                {
                    return patrolPos;
                }
            }

            return null;
        }
    }
}
