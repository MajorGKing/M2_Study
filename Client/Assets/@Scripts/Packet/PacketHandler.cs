using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Linq;
using static Define;

class PacketHandler
{
    ///////////////////////////////////// GameServer - Client /////////////////////////////////////
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_Connected");
    }

    public static void S_AuthResHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_AuthResHandler");

        UI_TitleScene sceneUI = Managers.UI.GetSceneUI<UI_TitleScene>();
        if (sceneUI == null)
            return;

        S_AuthRes resPacket = packet as S_AuthRes;
        sceneUI.OnAuthResHandler(resPacket);
    }

    public static void S_HeroListResHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_HeroListResHandler");

        UI_TitleScene sceneUI = Managers.UI.GetSceneUI<UI_TitleScene>();
        if(sceneUI == null)
            return;

        S_HeroListRes resPacket = packet as S_HeroListRes;
        sceneUI.OnHeroListResHandler(resPacket);
    }

    public static void S_CreateHeroResHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_CreateHeroResHandler");

        UI_CreateCharacterPopup popupUI = Managers.UI.GetLastPopupUI<UI_CreateCharacterPopup>();
        if (popupUI == null)
            return;

        S_CreateHeroRes resPacket = packet as S_CreateHeroRes;
        popupUI.OnCreateHeroResHandler(resPacket);
    }

    public static void S_DeleteHeroResHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_DeleteHeroResHandler");

        UI_SelectCharacterPopup popupUI = Managers.UI.GetLastPopupUI<UI_SelectCharacterPopup>();
        if (popupUI == null)
            return;

        S_DeleteHeroRes resPacket = packet as S_DeleteHeroRes;
        popupUI.OnDeleteHeroResHandler(resPacket);
    }

    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_EnterGameHandler");

        S_EnterGame enterGamePacket = packet as S_EnterGame;
        // Hero
        MyHero myHero = Managers.Object.Spawn(enterGamePacket.MyHeroInfo);
        myHero.ObjectState = EObjectState.Idle;
        myHero.TotalStat = enterGamePacket.MyHeroInfo.HeroInfo.CreatureInfo.TotalStatInfo;

        //Init
        //Managers.Inventory.Refresh(enterGamePacket.Items.ToList());
        Managers.Skill.HandleEnterGame(enterGamePacket);

        //Scene
        GameScene scene = Managers.Scene.CurrentScene as GameScene;
        scene.HandleEnterGame();

        //UI
        var sceneUI = Managers.UI.GetSceneUI<UI_GameScene>();
        //sceneUI.QuickSlot.SetInfo();
        //sceneUI.OnHpChanged();
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_LeaveGameHandler");

        Managers.Object.Clear();
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_DespawnHandler");

        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int ObjectId in despawnPacket.ObjectIds)
        {
            Managers.Object.Despawn(ObjectId);
        }
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_SpawnHandler");

        S_Spawn spawnPacket = packet as S_Spawn;

        foreach (HeroInfo obj in spawnPacket.Heroes)
        {
            Managers.Object.Spawn(obj);
        }

        foreach (CreatureInfo obj in spawnPacket.Creatures)
        {
            Managers.Object.Spawn(obj);
        }

        foreach(ProjectileInfo obj in spawnPacket.Projectiles)
        {
            Managers.Object.Spawn(obj);
        }

        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Spawn(obj);
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_MoveHandler");

        S_Move movePacket = packet as S_Move;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        BaseObject bo = go.GetComponent<BaseObject>();
        if (bo == null)
            return;

        Debug.Log($"{go.name} , CellPos : {movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}");
        bo.PosInfo = movePacket.PosInfo;
    }

    // TODO
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {

    }

    // TODO
    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_SkillHandler");

        S_Skill skillPacket = packet as S_Skill;

        GameObject go =Managers.Object.FindById(skillPacket.ObjectId);
        if (go == null)
            return;

        Creature cc = go.GetComponent<Creature>();
        if (cc != null) 
            cc.HandleSkillPacket(skillPacket);
            
    }

    // TODO
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        //Debug.Log("S_ChangeHpHandler");

        S_ChangeHp changePacket = packet as S_ChangeHp;

        GameObject go = Managers.Object.FindById(changePacket.ObjectId);
        if (go == null)
            return;

        Creature cc = go.GetComponent<Creature>();
        if (cc != null)
        {
            int damage = (int)changePacket.Damage;
            cc.Hp = changePacket.Hp;
            cc.Mp = changePacket.Mp;
            //Managers.Object.ShowDamageFont(cc.CenterPos, damage, cc.transform, changePacket.DamageType);
            cc.DamageFontController.AddDamageFont(damage, cc.transform, changePacket.DamageType);
        }

        var gameScene = Managers.UI.GetSceneUI<UI_GameScene>();
        gameScene.OnHpChanged();
    }

    // TODO
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_DieHandler");

        S_Die diePacket = packet as S_Die;

        GameObject go = Managers.Object.FindById(diePacket.ObjectId);
        if (go == null)
            return;

        Creature cc = go.GetComponent<Creature>();
        if (cc != null)
        {
            cc.Hp = 0;
            cc.OnDead();
        }
    }

    // TODO
    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        Debug.Log("S_ChangeStatHandler");

        S_ChangeStat pkt = (S_ChangeStat)packet;

        MyHero myHero = Managers.Object.MyHero;
        if (myHero == null)
            return;

        myHero.HandleChangeStat(pkt);
    }

    public static void S_ApplyEffectHandler(PacketSession session, IMessage packet)
    {
        S_ApplyEffect pkt = (S_ApplyEffect)packet;

        GameObject go = Managers.Object.FindById(pkt.ObjectId);
        if (go == null)
            return;

        Creature cc = go.GetComponent<Creature>();
        if (cc == null)
            return;

        cc.ApplyEffect(pkt);
    }

    public static void S_RemoveEffectHandler(PacketSession session, IMessage packet)
    {
        S_RemoveEffect pkt = (S_RemoveEffect)packet;

        GameObject go = Managers.Object.FindById(pkt.ObjectId);
        if (go == null)
            return;

        Creature cc = go.GetComponent<Creature>();
        if (cc == null)
            return;

        cc.RemoveEffect(pkt);
    }
}
