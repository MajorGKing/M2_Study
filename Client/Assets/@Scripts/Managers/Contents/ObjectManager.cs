using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

public class ObjectManager
{
    public MyHero MyHero { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
    Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
    Dictionary<int, Hero> _heroes = new Dictionary<int, Hero>();
    HashSet<GameObject> _gameObjects = new HashSet<GameObject>(); // 서버랑 상관없는 오브젝트들

    #region Roots
    public Transform GetRootTransform(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root == null)
            root = new GameObject { name = name };

        return root.transform;
    }

    public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
    public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
    public Transform ProjectileRoot { get { return GetRootTransform("@Projectiles"); } }
    public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
    public Transform EffectRoot { get { return GetRootTransform("@Effects"); } }
    public Transform NpcRoot { get { return GetRootTransform("@Npc"); } }
    public Transform ItemHolderRoot { get { return GetRootTransform("@ItemHolders"); } }
    #endregion

    public ObjectManager()
    {
    }

    #region Spawn
    public MyHero Spawn(MyHeroInfo myHeroInfo)
    {
        HeroInfo info = myHeroInfo.HeroInfo;
        if (info == null || info.CreatureInfo == null || info.CreatureInfo.ObjectInfo == null)
            return null;
        ObjectInfo objectInfo = info.CreatureInfo.ObjectInfo;
        if (MyHero != null && MyHero.ObjectId == objectInfo.ObjectId)
            return null;
        if (_objects.ContainsKey(objectInfo.ObjectId))
            return null;
        EGameObjectType objectType = Utils.GetObjectTypeFromId(objectInfo.ObjectId);
        if (objectType != EGameObjectType.Hero)
            return null;
        
        int templateId = Utils.GetTemplateIdFromId(objectInfo.ObjectId);
        GameObject go = Managers.Resource.Instantiate($"HeroPrefab_{info.ClassType}_{info.Gender}"); // TEMP	
        //go.name = info.Name;
        go.name = "MyHero";
        go.transform.parent = HeroRoot;
        _objects.Add(objectInfo.ObjectId, go);

        MyHero = Utils.GetOrAddComponent<MyHero>(go);
        MyHero.SetInfo(templateId);
        MyHero.MyHeroInfo = myHeroInfo;
        MyHero.ObjectId = objectInfo.ObjectId;
        MyHero.PosInfo = objectInfo.PosInfo;
        MyHero.SyncWorldPosWithCellPos();

        Managers.Skill.Init(templateId);

        return MyHero;
    }

    public Hero Spawn(HeroInfo info)
    {
        if (info == null || info.CreatureInfo == null || info.CreatureInfo.ObjectInfo == null)
            return null;
        ObjectInfo objectInfo = info.CreatureInfo.ObjectInfo;
        if (MyHero.ObjectId == objectInfo.ObjectId)
            return null;
        if (_objects.ContainsKey(objectInfo.ObjectId))
            return null;
        EGameObjectType objectType = Utils.GetObjectTypeFromId(objectInfo.ObjectId);
        if (objectType != EGameObjectType.Hero)
            return null;

        int templateId = Utils.GetTemplateIdFromId(objectInfo.ObjectId);
        GameObject go = Managers.Resource.Instantiate($"HeroPrefab_{info.ClassType}_{info.Gender}"); // TEMP
        go.name = $"Hero_{objectInfo.ObjectId}";
        go.transform.parent = HeroRoot;
        _objects.Add(objectInfo.ObjectId, go);

        Hero hero = Utils.GetOrAddComponent<Hero>(go);
        hero.InitHero(info);
        hero.SetInfo(templateId);
        _heroes.Add(objectInfo.ObjectId, hero);
        return hero;
    }

    public Monster Spawn(CreatureInfo info)
    {
        if(info == null || info.ObjectInfo == null)
            return null;
        ObjectInfo objectInfo = info.ObjectInfo;
        if(_objects.ContainsKey(objectInfo.ObjectId))
            return null;
        EGameObjectType objectType = Utils.GetObjectTypeFromId(objectInfo.ObjectId);
        if(objectType != EGameObjectType.Monster)
            return null;
        int templateId = Utils.GetTemplateIdFromId(objectInfo.ObjectId);
        if(Managers.Data.MonsterDict.TryGetValue(templateId, out MonsterData monsterData) == false)
            return null;

        Debug.Log($"objectInfo.ObjectId : {objectInfo.ObjectId}");

        GameObject go = Managers.Resource.Instantiate(monsterData.PrefabName); // TEMP
        go.name = $"Monster_{objectInfo.ObjectId}";
        go.transform.parent = MonsterRoot;
        _objects.Add(objectInfo.ObjectId, go);
        Monster monster = Utils.GetOrAddComponent<Monster>(go);
        monster.SetInfo(templateId);
        monster.InitMonster(info);
        _monsters.Add(objectInfo.ObjectId, monster);

        monster.SyncWorldPosWithCellPos();
        return monster;
    }

    public Projectile Spawn(ProjectileInfo info)
    {
        if (info == null || info.ObjectInfo == null)
            return null;

        ObjectInfo objectInfo = info.ObjectInfo;
        if (_objects.ContainsKey(info.ObjectInfo.ObjectId))
            return null;

        //EGameObjectType objectType = Utils.GetObjectTypeFromId(objectInfo.ObjectId);
        int templateId = Utils.GetTemplateIdFromId(objectInfo.ObjectId);
        if (Managers.Data.ProjectileDict.TryGetValue(templateId, out ProjectileData projectileData) == false)
            return null;

        // 첫번재 화살
        GameObject go = Managers.Resource.Instantiate(projectileData.PrefabName, pooling: true);
        Projectile proj = Utils.GetOrAddComponent<Projectile>(go);
        _objects.Add(objectInfo.ObjectId, go);
        proj.SetInfo(info, info.TargetId);

        //발사체 갯수가 여러개일때 나머지 화살 spawn
        if (projectileData.Count > 1)
            Managers.Instance.StartCoroutine(GenerateProjectile(projectileData, info, info.TargetId));

        //proj.ObjectId = objectInfo.ObjectId;
        //proj.PosInfo = objectInfo.PosInfo;
        

        return proj;
    }

    public void Spawn(ObjectInfo info)
    {
        if (_objects.ContainsKey(info.ObjectId))
            return;

        EGameObjectType objectType = Utils.GetObjectTypeFromId(info.ObjectId);
        int templateId = Utils.GetTemplateIdFromId(info.ObjectId);

        switch (objectType)
        {
            case EGameObjectType.Npc:
                SpawnNpc(templateId, info);
                break;
        }
    }

    private void SpawnNpc(int templateId, ObjectInfo info)
    {
        if (Managers.Data.NpcDict.TryGetValue(templateId, out NpcData npcData) == false)
            return;

        GameObject go = Managers.Resource.Instantiate(npcData.PrefabName);
        _objects.Add(info.ObjectId, go);

        Npc npc = Utils.GetOrAddComponent<Npc>(go);
        npc.SetInfo(info);
    }

    public GameObject Spawn(string name, Transform parents = null, bool isPooling = false)
    {
        GameObject go = Managers.Resource.Instantiate(name, parents, isPooling);
        _gameObjects.Add(go);
        return go;
    }

    public void DespawnGameObject(GameObject obj)
    {
        Managers.Resource.Destroy(obj);
        _gameObjects.Remove(obj);
    }

    public ParticleController SpawnParticle(string name, bool lookLeft = false, Transform parent = null, bool isCenterPos = false)
    {
        if (parent == null)
            return null;

        GameObject go = Managers.Resource.Instantiate(name, pooling: true);
        go.transform.parent = parent;

        if (isCenterPos)
        {
            BaseObject baseObject = parent.GetComponent<BaseObject>();
            go.transform.position = baseObject.CenterPos;
        }
        else
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
        }

        if (lookLeft)
            go.transform.Rotate(0, 180, 0);

        ParticleController pc = go.GetOrAddComponent<ParticleController>();

        return pc;
    }

    public void Despawn(int objectId)
    {
        if (MyHero != null && MyHero.ObjectId == objectId)
            return;
        if (_objects.ContainsKey(objectId) == false)
            return;

        GameObject go = FindById(objectId);
        if (go == null)
            return;

        BaseObject bo = go.GetComponent<BaseObject>();
        if (bo != null)
        {

        }

        Managers.Map.RemoveObject(bo);
        _objects.Remove(objectId);
        _monsters.Remove(objectId);

        Managers.Resource.Destroy(go);
    }

    private IEnumerator GenerateProjectile(ProjectileData projectileData, ProjectileInfo projInfo, int targetId)
    {
        Projectile projectile = null;
        for (int i = 0; i < projectileData.Count - 1; i++)
        {
            yield return new WaitForSeconds(0.333f);
            GameObject go = Managers.Resource.Instantiate(projectileData.PrefabName, pooling: true);
            Projectile proj = Utils.GetOrAddComponent<Projectile>(go);
            proj.SetInfo(projInfo, targetId);

        }
    }
    #endregion

    #region Find

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public Creature FindCreatureById(int id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return null;

        return go.GetComponent<Creature>();
    }

    public GameObject FindCreature(Vector3Int cellPos)
    {
        foreach (GameObject obj in _objects.Values)
        {
            Creature creature = obj.GetComponent<Creature>();
            if (creature == null)
                continue;

            //if (creature.CellPos == cellPos)
            //	return obj;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public Monster FindClosestMonster()
    {
        List<Monster> monsters = _monsters.Values.ToList();

        int minDist = int.MaxValue;
        Monster closestMonster = null;
        foreach (Monster monster in monsters)
        {
            int dist = Managers.Object.MyHero.GetDistance(monster);
            if(dist < minDist)
            {
                minDist = dist;
                closestMonster = monster;
            }
        }

        return closestMonster;
    }

    public List<T> FindAllComponents<T>(Func<T, bool> condition) where T : UnityEngine.Component
    {
        List<T> ret = new List<T>();

        foreach (GameObject obj in _objects.Values)
        {
            T t = Utils.FindChild<T>(obj, recursive: true);
            if (t != null && condition.Invoke(t))
                ret.Add(t);
        }

        return ret;
    }

    public List<Hero> FindHeroes(Vector3Int pos, Func<Hero, bool> condition = null)
    {
        List<Hero> objs = new List<Hero>();

        foreach (Hero hero in _heroes.Values.ToList())
        {
            if (condition == null || condition.Invoke(hero) == false)
                continue;
            objs.Add(hero);
        }

        return objs;
    }

    public List<Monster> FindMonsters(Vector3Int pos, Func<Monster, bool> condition = null)
    {
        List<Monster> objs = new List<Monster>();

        foreach (Monster monster in _monsters.Values.ToList())
        {
            if (condition == null || condition.Invoke(monster) == false)
                continue;

            objs.Add(monster);
        }
        return objs;
    }

    public List<Creature> FindCreatures(Vector3Int pos, Func<Creature, bool> condition = null)
    {
        List<Creature> objs = new List<Creature>();
        objs.AddRange(FindHeroes(pos, condition));
        objs.AddRange(FindMonsters(pos, condition));
        return objs;
    }
    #endregion

    #region PacketHandler
    public void HandleLeaveGameHandler(S_LeaveGame packet)
    {
        if (packet.LeaveType == ELeaveType.Dead)
        {
            //TODO UI 클릭 후 Clear
            // Despawn(MyHero.ObjectId);
            Managers.Instance.StartCoroutine(ReserveClear(2.5f));
        }
        else
        {
            Clear();
        }
    }
    #endregion

    IEnumerator ReserveClear(float delay)
    {
        yield return new WaitForSeconds(delay);
        Clear();
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);

        _objects.Clear();
        _monsters.Clear();
        _heroes.Clear();
        _gameObjects.Clear();
        if (MyHero)
        {
            MyHero.ClearMyHero();
            MyHero = null;
        }
    }
}