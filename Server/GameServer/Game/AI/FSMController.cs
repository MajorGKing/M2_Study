using GameServer;
using GameServer.Game;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public abstract class FSMController<OwnerType, TargetType> : BaseAIController<OwnerType> where OwnerType : Creature where TargetType : BaseObject
    {
        protected int _searchCellDist;
        protected int _chaseCellDist;
        protected int _patrolCellDist;
        protected int _spawnRange;
		protected TargetType _target { get; set; }
        protected TargetType _attacker { get; set; }

        protected Vector2Int? _patrolDest;
        protected Random _rand = new Random();

        protected bool isGotoSpawnPos = false;

        protected int _chaseCount = 0;
        public FSMController(OwnerType owner) : base(owner)
        {
        }

        #region AI
        protected abstract TargetType FindTarget();

        protected override void UpdateIdle()
        {
            _target = FindTarget();

            // 1. 주변에 타겟이 있으면 추적.
            if(_target != null)
            {
                _patrolDest = null;

                int dist = _target.GetDistance(Owner);
                int skillRange = Owner.SkillComp.GetNextUseSkillDistance(_target.ObjectId);

                // 1-2. 사거리 밖에 공격자가 있는 경우
                if (dist > skillRange)
                {
                    //달려들기
                    FindPathAndMove(_target.CellPos);
                    return;
                }
                else
                {
                    //사거리에 있으면 바로 공격.
                    SetState(EObjectState.Skill);
                    return;
                }
            }

            // 2. 타겟이 없으면, 주기적으로 주변 정찰.
            int randValue = _rand.Next(0, 4); // TODO : 데이터로 빼기
            if (randValue == 0 && _patrolCellDist > 0)
            {
                _patrolDest = GetPatrolPos();
                if (_patrolDest.HasValue)
                {
                    FindPathAndMove(_patrolDest.Value);
                    return;
                }
            }
        }

        protected override void UpdateMoving()
        {
            if (Owner.IsStunned)
            {
                SetState(EObjectState.Idle);
                return;
            }

            // 1. 스폰장소로 되돌아가야 하는지 확인
            if (ManageReturnToSpawn())
                return;

            // 2. 타겟 추적
            if (ProcessTargetChase())
                return;

            // 3. 정찰
            if (_patrolDest.HasValue)
            {
                FindPathAndMove(_patrolDest.Value);
                return;
            }

            // 4. 타겟도 없고 정찰도 아니면 이동 상태 종료.
            SetState(EObjectState.Idle);
            return;
        }

        bool FindPathAndMove(Vector2Int destPos)
        {
            // 1. 길찾기.
            List<Vector2Int> path = Owner.Room?.Map.FindPath(Owner, Owner.CellPos, destPos);

            // 2. 길을 못찾았거나 너무 멀면 포기.
            if (path == null || path.Count < 2)
            {
                _patrolDest = null;
                _target = null;
                SetState(EObjectState.Idle);
                return false;
            }

            //쫒는중이라면 상대가 너무 멀면 포기
            if (_target != null && path.Count > _chaseCellDist)
            {
                GiveUpChase();
            }

            // 3. 이동 실행.
            Owner.Room.Map.ApplyMove(Owner, path[1]);
            SetState(EObjectState.Move); // UpdateTick 설정을 위해 ApplyMove -> Move.
            Owner.BroadcastMove();

            return true;
        }

        protected override void UpdateSkill()
        {
            if (Owner.IsStunned)
            {
				SetState(EObjectState.Idle);
				return;
            }

            // 1. 유효한 타겟인지.
            if(_target.IsValid() == false)
            {
                _target = null;
                SetState(EObjectState.Move);
                return;
            }

            // 2. 거리가 충분히 가까운지.
            int dist = Owner.GetDistance(_target);
            Skill skill = Owner.SkillComp.GetNextUseSkill(_target.ObjectId);
            if (skill != null)
            {
                // 3. 스킬 사용.
                Owner.Room?.UseSkill(Owner, skill.TemplateId, _target.ObjectId);
            }
            else
            {
                // 타겟은 날리지 말고 그냥 따라가게 만든다
                SetState(EObjectState.Move);
                return;
            }
        }

        protected override void UpdateDead()
        {
        }

        #endregion

        #region UpdateMoving
        private bool ManageReturnToSpawn()
        {
            if (isGotoSpawnPos)
            {
                if (Owner.GetDistance(GetSpawnPos()) < 3)
                {
                    isGotoSpawnPos = false;
                    _chaseCount = 0;
                    SetState(EObjectState.Idle);
                }
                else
                {
                    FindPathAndMove(GetSpawnPos());
                }
                return true;
            }
            return false;
        }

        private bool ProcessTargetChase()
        {
            if (_target != null)
            {
                _patrolDest = null;

                int dist = Owner.GetDistance(_target);

                // 2-1. 따라갈 수 있는 거리인지 확인 + 너무 오랫동안 추적했으면 포기
                if (dist == 0 || dist > _chaseCellDist || _chaseCount > _chaseCellDist)
                {
                    GiveUpChase();
                    return true;
                }

                // 2-2. 사거리에 있으면 스킬로 넘어감.
                int skillRange = Owner.SkillComp.GetNextUseSkillDistance(_target.ObjectId);
                if (dist <= skillRange)
                {
                    SetState(EObjectState.Skill);
                }
                else
                {
                    // 2-3. 사거리에 없으면 이동.
                    if (FindPathAndMove(_target.CellPos))
                        _chaseCount++;
                }
                return true;
            }
            return false;
        }
        #endregion

        public override void OnDamaged(BaseObject attacker, float damage)
        {
            if (Owner.State == EObjectState.Dead)
                return;
            //이미 타겟이 있고 전투중이면 return
            if (_target != null && Owner.State == EObjectState.Skill)
                return;

            attacker = attacker.GetOwner();

            base.OnDamaged(attacker, damage);

            if(Owner.State == EObjectState.Idle)
            {
                if (attacker is TargetType)
                    _attacker = (TargetType)attacker;

                SetState(EObjectState.Move);
            }
            else if(Owner.State == EObjectState.Move)
            {
                if(attacker is TargetType)
                    _attacker = (TargetType)attacker;
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

        protected abstract Vector2Int GetSpawnPos();

        protected abstract int GetSpawnRange();

        protected bool IsOutOfSpawnRange(Vector2Int? pos = null)
        {
            Vector2Int position = pos ?? Owner.CellPos;
            return Utils.GetDistance(position, GetSpawnPos()) > GetSpawnRange();
        }

        private void GiveUpChase()
        {
            _target = null;
            _attacker = null;
            isGotoSpawnPos = true;
        }
    }
}
