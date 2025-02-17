using GameServer;
using GameServer.Game;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Game
{
    public abstract class FSMController<OwnerType, TargetType> : BaseAIController<OwnerType> where OwnerType : Creature where TargetType : BaseObject
    {
        protected int _searchCellDist;
        protected int _chaseCellDist;
        protected int _patrolCellDist;
        protected int _spawnRange;
		protected TargetType _target { get; set; }

        protected Vector2Int? _patrolDest;
        protected Random _rand = new Random();

        protected bool _returnToSpawnPos = false;
        protected int _chaseCount = 0;
        public FSMController(OwnerType owner) : base(owner)
        {
        }

        #region AI
        protected abstract TargetType FindTarget();

        protected override void UpdateIdle()
        {
            // 0. 타겟 찾기.
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

            // 0. 타겟 찾기.
            _target = FindTarget();

			// 1. 타겟 추적
			if (ProcessTargetChase())
				return;

            // 2. 스폰장소로 되돌아가야 하는지 확인
            if (ProcessReturnToSpawnPos())
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

            // 3. 쫒는중인데 상대가 너무 멀면 포기
            if (_target != null && path.Count > _chaseCellDist)
            {
                GiveUpChaseTarget();
                return false;
            }

            // 4. 이동 실행.
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
            if (skill == null)
            {
				// 타겟은 날리지 말고 그냥 따라가게 만든다
				SetState(EObjectState.Move);
				return;
			}
                
            // 3. 스킬 사용.                
            Owner.Room?.UseSkill(Owner, skill.TemplateId, _target.ObjectId);
        }

        protected override void UpdateDead()
        {
        }

        #endregion

        #region UpdateMoving
        private bool ProcessReturnToSpawnPos()
        {
            if (_returnToSpawnPos == false)
                return false;

            if (Owner.GetDistance(GetSpawnPos()) < 3)
            {
                _returnToSpawnPos = false;
                _chaseCount = 0;
                SetState(EObjectState.Idle);
            }
            else
            {
                FindPathAndMove(GetSpawnPos());
            }

            return true;
        }

        private bool ProcessTargetChase()
        {
            if (_target == null)
                return false;
            
            _patrolDest = null;

            int dist = Owner.GetDistance(_target);

            // 1. 따라갈 수 있는 거리인지 확인 + 너무 오랫동안 추적했으면 포기
            if (dist == 0 || dist > _chaseCellDist || _chaseCount > _chaseCellDist * 3)
            {
                GiveUpChaseTarget();
                return true;
            }
           
            int skillRange = Owner.SkillComp.GetNextUseSkillDistance(_target.ObjectId);
			// 2. 사거리에 없으면 이동.
			if (skillRange < dist)
            {
				if (FindPathAndMove(_target.CellPos))
					_chaseCount++;

                return true;
			}

			// 3. 사거리에 있으면 스킬로 넘어감.
            _chaseCount = 0;
            SetState(EObjectState.Skill);

            return true;
        }
        #endregion

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

        private void GiveUpChaseTarget()
        {
            _chaseCount = 0;
            _target = null;
            _returnToSpawnPos = true;
        }
    }
}
