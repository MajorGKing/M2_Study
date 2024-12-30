using GameServer;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class BaseAIController<OwnerType> where OwnerType : BaseObject
    {
        public OwnerType Owner { get; protected set; }
        public int UpdateTick { get; set; } = 100;

        public BaseAIController(OwnerType owner)
        {
            Owner = owner;
        }

        protected IJob _job;
        public virtual void Update()
        {
            switch (Owner.State)
            {
                case EObjectState.Idle:
                    UpdateIdle();
                    break;
                case EObjectState.Move:
                    UpdateMoving();
                    break;
                case EObjectState.Skill:
                    UpdateSkill();
                    break;
                case EObjectState.Dead:
                    UpdateDead();
                    break;
            }

            if (Owner.Room != null)
                _job = Owner.Room.PushAfter(UpdateTick, Update);
        }

        public virtual void SetState(EObjectState State)
        {
            if (Owner.State == State)
                return;

            Owner.State = State;
        }

        protected virtual void UpdateIdle()
        {
        }

        protected virtual void UpdateMoving()
        {
        }

        protected virtual void UpdateSkill()
        {
        }

        protected virtual void UpdateDead()
        {
        }

        public virtual void OnDamaged(BaseObject attacker, float damage)
        {
        }

        public virtual void OnDead(BaseObject attacker)
        {

        }
    }
}
