using System;
using Google.Protobuf.Protocol;
using Scripts.Data.SO;
using UnityEngine;
using static Define;

public class GameManager
{
    public int SelectedHeroIndex { get; set; }

    private EJoystickState _joystickState;
    public EJoystickState JoystickState
    {
        get => _joystickState;
        set
        {
            _joystickState = value;
            OnJoystickChanged?.Invoke(_joystickState, MoveDir);
        }
    }

    private EMoveDir _moveDir;
    public EMoveDir MoveDir
    {
        get => _moveDir;
        set
        {
            _moveDir = value;
        }
    }

    public event Action<EJoystickState, EMoveDir> OnJoystickChanged;
    
    public void StartDialogue(int DialogueId, Action OnEndDialogue, string iconName)
    {
        if (Managers.Data.DialogueDict.TryGetValue(DialogueId, out var data) == false)
        {
            Debug.LogError($"Dialogue {DialogueId} not found");
            return;
        }
        
        StartDialogue(data, OnEndDialogue, iconName);
    }

    public void StartDialogue(DialogueData dialogueData, Action OnEndDialogue, string iconName)
    {
        Debug.Log($"Starting Dialogue {dialogueData.TemplateId}");
        Managers.Object.MyHero.DialogueHandler.StartDialogue(dialogueData, OnEndDialogue);
        UI_DialoguePopup ui = Managers.UI.ShowPopupUI<UI_DialoguePopup>();
        ui.SetInfo(iconName);
    }
}