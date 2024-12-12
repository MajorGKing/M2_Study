using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class ObjectManager
{
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

    public void Clear()
    {

    }

    public GameObject SpawnGameObject(Vector3 position, string prefabName)
    {
        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        go.transform.position = position;
        
        return go;
    }

    public void DespawnGameObject<T>(T obj) where T : BaseController
    {

    }


}