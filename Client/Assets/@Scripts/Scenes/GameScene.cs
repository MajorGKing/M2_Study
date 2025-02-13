using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class GameScene : BaseScene
{
    private UI_GameScene _uiGameScene;
    private UI_Joystick _uiJoystick;
    private UI_LoadingPopup _loadingPopup;
    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        gameObject.AddComponent<CaptureScreenShot>();
#endif

        Debug.Log("@>> GameScene Init()");
        SceneType = EScene.GameScene;
        Managers.UI.CacheAllPopups();

        _loadingPopup = Managers.UI.ShowPopupUI<UI_LoadingPopup>();
        //Managers.Map.LoadMap("MMO_edu_map");

        C_EnterGame enterGame = new C_EnterGame();
        enterGame.HeroIndex = Managers.Game.SelectedHeroIndex;
        Managers.Network.Send(enterGame);

        //var loadingPopup = Managers.UI.ShowPopupUI<UI_LoadingPopup>();
        //Managers.UI.ShowSceneUI<UI_Joystick>();

        //loadingPopup.ClosePopupUI();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void HandleEnterGame(S_EnterGame packet)
    {
        #region Scene UI

        if (_uiGameScene == null)
        {
            _uiJoystick = Managers.UI.ShowSceneUI<UI_Joystick>();
            _uiGameScene = Managers.UI.ShowSceneUI<UI_GameScene>();
            _uiGameScene.GetComponent<Canvas>().sortingOrder = 1;
            Managers.UI.SceneUI = _uiGameScene;
            _uiGameScene.SetInfo();
            _loadingPopup.ClosePopupUI();
        }
        #endregion

    }

    public override void Clear()
    {
    }

    void OnApplicationQuit()
    {
        Managers.Network.GameServer.Disconnect();
    }
}