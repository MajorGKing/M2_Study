﻿using GameServer;
using GameServer.Game;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public class Projectile : BaseObject
    {
        public Creature Owner { get; set; }
        public Data.SkillData SkillData { get; private set; }
        public ProjectileData ProjectileData { get { return SkillData.Projectile; } }
        public float Speed { get { return ProjectileData.ProjSpeed; } }

        private Creature _target { get; set; }
        public ProjectileInfo ProjectileInfo { get; private set; } = new ProjectileInfo();

        public Projectile()
        {
            ObjectType = EGameObjectType.Projectile;
            ProjectileInfo.ObjectInfo = ObjectInfo;
        }

        public void Init(Data.SkillData skillData, Creature target)
        {
            SkillData = skillData;
            _target = target;

            if (target != null)
                ProjectileInfo.TargetId = target.ObjectId;
        }

        // 굳이 투사체 연산을 하지 않고, 바로 피격 예약을 걸어준다.
        public override void Update()
        {
            ReserveHit();
        }

        public bool ReserveHit()
        {
            // 무슨 일이 일어나더라도, 소멸은 한다.
            GameRoom room = Room;
            room?.PushAfter(10 * 1000, room.LeaveGame, ObjectId, false);

            if (_target == null)
                return false;
            if (_target.Room != Room)
                return false;
            if (Room == null || Room.Map == null)
                return false;
            if (Speed == 0)
                return false;

            Vector2 worldPos = Room.Map.CellToWorld(CellPos);
            Vector2 targetWorldPos = Room.Map.CellToWorld(_target.CellPos);
            Vector2 dir = (targetWorldPos - worldPos);
            int tickAfter = (int)(dir.Length() * 1000 / Speed);

            Room.PushAfter(tickAfter, OnHitTarget);

            return true;
        }

        void OnHitTarget()
        {
            if (_target == null || _target.EffectComp == null)
                return;
            if (_target.Room != Room)
                return;
            if (_target.State == EObjectState.Dead)
                return;
            if (SkillData == null || SkillData.EffectData == null)
                return;
            List<Creature> targets = Skill.GatherSkillEffectTargets(Owner, SkillData, _target);
            if (targets.Count == 0)
                return;

            // 이펙트(효과 및 데미지) 적용
            foreach (Creature t in targets)
            {
                if(Owner.ObjectType == EGameObjectType.Hero)
                    Console.WriteLine($"{Owner} attack {t.ObjectId} by projectile {SkillData.EffectData.Name}!");
                t.EffectComp.ApplyEffect(SkillData.EffectData.TemplateId, Owner);
            }

            // 소멸.
            GameRoom room = Room;
            room?.Push(room.LeaveGame, ObjectId, false);
        }
    }
}
