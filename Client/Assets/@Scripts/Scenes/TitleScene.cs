using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;

public class TitleScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        SceneType = Define.EScene.TitleScene;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
        GraphicsSettings.transparencySortAxis = new Vector3(0.0f, 1.0f, 0.0f);
        Application.runInBackground = true;

        // 예외적으로 직접 등록한다 (UI_TitleScene은 애셋 로딩도 담당하기 때문)
        Managers.UI.SceneUI = GameObject.FindAnyObjectByType<UI_TitleScene>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Clear()
    {

    }
}
