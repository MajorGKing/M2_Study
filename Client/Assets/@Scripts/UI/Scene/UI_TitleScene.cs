using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Data;
using Data.SO;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class UI_TitleScene : UI_Scene
{
    private enum GameObjects
    {
        ServerArea,
        Background
    }

    private enum Texts
    {
        StartText,
        StatusText,
        SelectedServerNameText,
    }

    private enum ETitleSceneState
    {
        None,

        // �ּ� �ε�
        AssetLoading,
        AssetLoaded,

        // ���� ����
        LoginSuccess,
        LoginFail,

        // ���� ���� ����
        ConnectingToGameServer,
        ConnectedToGameServer,
        FailedToConnectToGameServer,
    }

    ETitleSceneState _state = ETitleSceneState.None;
    ETitleSceneState State
    {
        get { return _state; }
        set
        {
            Debug.Log($"TitleSceneState : {_state} -> {value}");

            _state = value;

            GetText(((int)Texts.StartText)).gameObject.SetActive(true);

            switch (value)
            {
                case ETitleSceneState.None:
                    break;
                case ETitleSceneState.AssetLoading:
                    GetText((int)Texts.StatusText).text = $"TODO �ε���";
                    GetText(((int)Texts.StartText)).gameObject.SetActive(false);
                    break;
                case ETitleSceneState.AssetLoaded:
                    GetText((int)Texts.StatusText).text = "TODO �α��� ����� �����ϼ���";
                    GetText(((int)Texts.StartText)).gameObject.SetActive(false);
                    break;
                case ETitleSceneState.LoginSuccess:
                    GetText((int)Texts.StatusText).text = "TODO �α��� ����! \n ������ ����ּ���";
                    GetText((int)Texts.StartText).text = "TODO ȭ���� ��ġ�ϼ���.";
                    break;
                case ETitleSceneState.LoginFail:
                    GetText((int)Texts.StatusText).text = "TODO �α��� ����";
                    break;
                case ETitleSceneState.ConnectingToGameServer:
                    GetText((int)Texts.StatusText).text = "TODO ���� ������";
                    break;
                case ETitleSceneState.ConnectedToGameServer:
                    GetText((int)Texts.StatusText).text = "TODO ���� ���� ����";
                    GetText((int)Texts.StartText).text = "TODO ȭ���� ��ġ�ϼ���.";
                    break;
                case ETitleSceneState.FailedToConnectToGameServer:
                    GetText((int)Texts.StatusText).text = "TODO ���� ���� ����";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    private int _serverIndex = 0; // temp

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        GetText(((int)Texts.StartText)).gameObject.BindEvent(OnClickNextButton);
        GetText(((int)Texts.StartText)).gameObject.SetActive(false);

        GetObject((int)GameObjects.ServerArea).BindEvent(OnClickChooseServerButton);
    }

    protected override void Start()
    {
        base.Start();

        // Load ����
        State = ETitleSceneState.AssetLoading;

        Managers.Resource.LoadAllAsync<Object>("Preload", (key, count, totalCount) =>
        {
            GetText((int)Texts.StatusText).text = $"TODO �ε��� : {key} {count}/{totalCount}";

            if (count == totalCount)
            {
                OnAssetLoaded();
            }
        });
    }

	private void RefreshUI()
	{
		Managers.Data.ConfigDict.TryGetValue(GameSettingEx.ServerIndex, out ConfigData configData);
		GetText((int)Texts.SelectedServerNameText).text = configData.ServerName;
	}

	private void OnAssetLoaded()
	{
		State = ETitleSceneState.AssetLoaded;
		Managers.Data.Init();

        // �ε� �Ϸ�Ǹ� �α���
        UI_LoginPopup popup = Managers.UI.ShowPopupUI<UI_LoginPopup>();
        popup.SetInfo(OnLoginSuccess);
    }

    private void OnLoginSuccess(bool isSuccess)
    {
        if (isSuccess)
            State = ETitleSceneState.LoginSuccess;
        else
            State = ETitleSceneState.LoginFail;

        RefreshUI();
    }

    private void ConnectToGameServer()
    {
        State = ETitleSceneState.ConnectingToGameServer;
        Managers.Data.ConfigDict.TryGetValue(GameSettingEx.ServerIndex, out ConfigData configData);
        //IPAddress ipAddr = IPAddress.Parse("43.203.244.11");
        //IPAddress ipAddr = IPAddress.Parse("172.30.1.51");
        //IPAddress ipAddr = IPAddress.Parse("127.0.0.1");

        // 1. ���� ������ ���
        //IPAddress ipAddr = IPAddress.Parse("3.37.34.101");

        // 2. DNS ���
        //IPAddress ipAddr = Dns.GetHostAddresses("m2.rookiss.io")[0];

        // 3. Dev
        IPAddress ipAddr = IPAddress.Parse(configData.ServerIp);

        IPEndPoint endPoint = new IPEndPoint(ipAddr, configData.ServerPort);
        Managers.Network.GameServer.Connect(endPoint, OnGameServerConnectionSuccess, OnGameServerConnectionFailed);
    }

    private void OnGameServerConnectionSuccess()
    {
        State = ETitleSceneState.ConnectedToGameServer;

        // TODO : ���� ������ ���� JWT ��ū�� Ȱ����, ���� ���� ���� ���.
        C_AuthReq authReqPacket = new C_AuthReq();
        authReqPacket.Jwt = ""; // TEMP
        Managers.Network.Send(authReqPacket);
    }

    private void OnGameServerConnectionFailed()
    {
        State = ETitleSceneState.FailedToConnectToGameServer;
    }

    private void OnClickNextButton(PointerEventData evt)
    {
        if (State == ETitleSceneState.ConnectingToGameServer
            || State == ETitleSceneState.ConnectedToGameServer
            || State == ETitleSceneState.LoginFail)
                return;


        //if (State == ETitleSceneState.LoginSuccess)
        {
            ConnectToGameServer();
        }
    }

    public void OnAuthResHandler(S_AuthRes resPacket)
    {
        if (State != ETitleSceneState.ConnectedToGameServer)
            return;

        if (resPacket.Success == false)
            return;

        // ���Ӽ����� ���� ��� ���ָ� ĳ���� ��� ��û.
        UI_SelectCharacterPopup popup = Managers.UI.ShowPopupUI<UI_SelectCharacterPopup>();
        popup.SendHeroListReqPacket();
    }

    public void OnHeroListResHandler(S_HeroListRes resPacket)
    {
        List<MyHeroInfo> heroes = resPacket.Heroes.ToList();

        UI_SelectCharacterPopup popup = Managers.UI.GetLastPopupUI<UI_SelectCharacterPopup>();
        if (popup == null)
            popup = Managers.UI.ShowPopupUI<UI_SelectCharacterPopup>();

        popup.SetInfo(heroes);
    }

    private void OnClickChooseServerButton(PointerEventData evt)
    {
        //if (State != ETitleSceneState.LoginSuccess)
        //    return;

        UI_SelectServerPopup serverPopup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
        serverPopup.SetInfo(() =>
        {
            RefreshUI();
        });
    }
}
