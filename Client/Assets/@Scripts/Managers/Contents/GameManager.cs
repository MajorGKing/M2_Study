using System;
using Google.Protobuf.Protocol;
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
}