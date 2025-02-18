using System;
using UnityEngine.EventSystems;
using UnityEngine;
using Google.Protobuf.Protocol;
using Spine.Unity;


public class UI_CreateCharacterPopup : UI_Popup
{
    enum GameObjects
    {
        SkeletonObject
    }

    enum Toggles
    {
        MaleToggle,
        FemaleToggle,

        WarriorToggle,
        RangerToggle,
        WizardToggle,
        RogueToggle,
    }

    enum Buttons
    {
        CreateCharacterButton,
        CloseButton,
    }

    enum Texts
    {
        NicknameText,
        CreateLabelText,
    }

    Action OnHeroChanged;
    SkeletonGraphic _skeletonGraphic;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
		BindToggles(typeof(Toggles));
        BindButtons(typeof(Buttons));		
		BindTexts(typeof(Texts));

        GetToggle((int)Toggles.MaleToggle).gameObject.BindEvent(OnClickHeroGenderToggleButton);
        GetToggle((int)Toggles.FemaleToggle).gameObject.BindEvent(OnClickHeroGenderToggleButton);

        GetToggle((int)Toggles.WarriorToggle).gameObject.BindEvent(OnClickHeroClassToggleButton);
        GetToggle((int)Toggles.RangerToggle).gameObject.BindEvent(OnClickHeroClassToggleButton);
        GetToggle((int)Toggles.WizardToggle).gameObject.BindEvent(OnClickHeroClassToggleButton);
        GetToggle((int)Toggles.RogueToggle).gameObject.BindEvent(OnClickHeroClassToggleButton);

        GetButton((int)Buttons.CreateCharacterButton).gameObject.BindEvent(OnClickCreateCharacterButton);
        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);

        _skeletonGraphic = GetObject((int)GameObjects.SkeletonObject).GetComponent<SkeletonGraphic>();
    }

    public void SetInfo(Action onHeroChanged)
    {
        OnHeroChanged = onHeroChanged;

        // �ʱ� ����
        GetToggle((int)Toggles.MaleToggle).isOn = true;
        GetToggle((int)Toggles.WarriorToggle).isOn = true;
        GetText((int)Texts.NicknameText).text = "";
        Refresh();
    }

    private void Refresh()
    {
	    string gender = "f";
	    if (GetGender() == EHeroGender.Male)
		    gender = "m";

	    string className = GetClassType().ToString().ToLower();
	    string name = $"illust_{gender}_{className}_SkeletonData";
	    _skeletonGraphic.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(name);
	    _skeletonGraphic.Initialize(true);
	    _skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
	    _skeletonGraphic.startingLoop = true;
    }

    void OnClickHeroGenderToggleButton(PointerEventData evt)
    {
        Debug.Log("OnClickHeroGenderToggleButton");
        Refresh();
    }

    void OnClickHeroClassToggleButton(PointerEventData evt)
    {
		Debug.Log("OnClickHeroClassToggleButton");
		Refresh();
    }

    void OnClickCreateCharacterButton(PointerEventData evt)
    {
        Debug.Log("OnClickCreateCharacterButton");

        // 1) ������ C_CreateHeroReq ��Ŷ ����
        // 2) �������� DB�� �̸� üũ �� ����
        // 3) �������� S_CreateHeroRes ��Ŷ ���
        C_CreateHeroReq reqPacket = new C_CreateHeroReq();

        reqPacket.ClassType = GetClassType();
        reqPacket.Gender = GetGender();
        reqPacket.Name = GetName();

        Managers.Network.Send(reqPacket);
    }
    void OnClickCloseButton(PointerEventData evt)
    {
        OnHeroChanged?.Invoke();
        ClosePopupUI();
    }

    EHeroClass GetClassType()
    {
        if (GetToggle((int)Toggles.WarriorToggle).isOn)
            return EHeroClass.Knight;

        if (GetToggle((int)Toggles.RangerToggle).isOn)
            return EHeroClass.Archer;

		if (GetToggle((int)Toggles.WizardToggle).isOn)
			return EHeroClass.Wizard;

        if (GetToggle((int)Toggles.RogueToggle).isOn)
            return EHeroClass.Rogue;

        return EHeroClass.None;
    }

    EHeroGender GetGender()
    {
        if (GetToggle((int)Toggles.MaleToggle).isOn)
            return EHeroGender.Male;

        return EHeroGender.Female;
    }

    string GetName()
    {
        return GetText((int)Texts.NicknameText).text;
    }

    public void OnCreateHeroResHandler(S_CreateHeroRes resPacket)
    {
        if(resPacket.Result == ECreateHeroResult.Success)
        {
            OnHeroChanged?.Invoke();
            ClosePopupUI();
        }
    }
}
