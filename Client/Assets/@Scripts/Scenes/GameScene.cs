using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Managers.UI.CacheAllPopups();

        #region Scene UI

        var loadingPopup = Managers.UI.ShowPopupUI<UI_LoadingPopup>();
        Managers.UI.ShowSceneUI<UI_Joystick>();

        // UI_GameScene sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
        // sceneUI.GetComponent<Canvas>().sortingOrder = 1;
        // Managers.UI.SceneUI = sceneUI;
        // sceneUI.SetInfo();

        #endregion

        Managers.Map.LoadMap("MMO_edu_map");

        C_EnterGame enterGame = new C_EnterGame();
        enterGame.HeroIndex = Managers.Game.SelectedHeroIndex;
        Managers.Network.Send(enterGame);

        loadingPopup.ClosePopupUI();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Clear()
    {
    }

    void OnApplicationQuit()
    {
        Managers.Network.GameServer.Disconnect();
    }
}