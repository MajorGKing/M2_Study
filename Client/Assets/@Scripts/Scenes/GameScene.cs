using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class GameScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        gameObject.AddComponent<CaptureScreenShot>();
#endif

        Debug.Log("@>> GameScene Init()");
        SceneType = EScene.GameScene;

    }
    
    public override void Clear()
    {
    }
}