using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameManager
{
    public int SelectedHeroIndex { get; set; }

    public event Action<EJoystickState, EMoveDir> OnJoystickChanged;
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
            OnJoystickChanged?.Invoke(_joystickState, MoveDir);
        }
    }
}