using System;
using UnityEngine.EventSystems;
using UnityEngine;
using Google.Protobuf.Protocol;


public class UI_CreateCharacterPopup : UI_Popup
{
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

    protected override void Awake()
    {
        base.Awake();

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
    }

    public void SetInfo(Action onHeroChanged)
    {
        OnHeroChanged = onHeroChanged;

        // 초기 상태
        GetToggle((int)Toggles.MaleToggle).isOn = true;
        GetToggle((int)Toggles.WarriorToggle).isOn = true;
        GetText((int)Texts.NicknameText).text = "";
    }

    void OnClickHeroGenderToggleButton(PointerEventData evt)
    {
        Debug.Log("OnClickHeroGenderToggleButton");
    }

    void OnClickHeroClassToggleButton(PointerEventData evt)
    {
        Debug.Log("OnClickHeroClassToggleButton");
    }

    void OnClickCreateCharacterButton(PointerEventData evt)
    {
        Debug.Log("OnClickCreateCharacterButton");

        // 1) 서버로 C_CreateHeroReq 패킷 전송
        // 2) 서버에서 DB로 이름 체크 후 생성
        // 3) 서버에서 S_CreateHeroRes 패킷 답신
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
            return EHeroClass.Warrior;

        if (GetToggle((int)Toggles.RangerToggle).isOn)
            return EHeroClass.Ranger;

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
