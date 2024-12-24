using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_Joystick : UI_Scene
{
    enum GameObjects
    {
        JoystickBG,
        JoystickCursor,
        AttackButton,
        PickupButton,
        AutoButton
    }

    private GameObject _background;
    private GameObject _cursor;
    private float _radius;
    private Vector2 _touchPos;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));

        _background = GetObject((int)GameObjects.JoystickBG);
        _cursor = GetObject((int)GameObjects.JoystickCursor);
        _radius = _background.GetComponent<RectTransform>().sizeDelta.y / 5;

        gameObject.BindEvent(OnPointerDown, type: ETouchEvent.PointerDown);
        gameObject.BindEvent(OnPointerUp, type: ETouchEvent.PointerUp);
        gameObject.BindEvent(OnDrag, type: ETouchEvent.Drag);

        GetObject((int)GameObjects.AttackButton).BindEvent(OnAttackClick);
        GetObject((int)GameObjects.PickupButton).BindEvent(OnPickupClick);
        GetObject((int)GameObjects.AutoButton).BindEvent(OnAutoClick);

        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    #region Event
    private void OnAttackClick(PointerEventData evt)
    {
        Managers.Game.JoystickState = EJoystickState.Attack;
    }

    private void OnAutoClick(PointerEventData evt)
    {
        Managers.Game.JoystickState = EJoystickState.Auto;
    }

    private void OnPickupClick(PointerEventData evt)
    {
        Managers.Game.JoystickState = EJoystickState.Pickup;
    }

    private void OnPointerDown(PointerEventData evt)
    {
        _touchPos = Input.mousePosition;
        Managers.Game.JoystickState = EJoystickState.PointerDown;
    }

    private void OnPointerUp(PointerEventData evt)
    {
        _cursor.transform.localPosition = Vector3.zero;
        Managers.Game.JoystickState = EJoystickState.PointerUp;
    }

    private void OnDrag(PointerEventData eventData)
    {
        Vector2 touchDir = (eventData.position - _touchPos);

        float moveDist = Mathf.Min(touchDir.magnitude, _radius);
        Vector2 moveDir = touchDir.normalized;
        Vector2 newPosition = _touchPos + moveDir * moveDist;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(newPosition);
        _cursor.transform.position = worldPos;

        Managers.Game.MoveDir = DetermineMoveDirection(moveDir);
        Managers.Game.JoystickState = EJoystickState.Drag;
    }
    #endregion

    private EMoveDir DetermineMoveDirection(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return EMoveDir.None;

        float angle = Mathf.Atan2(-dir.x, +dir.y) * 180 / Mathf.PI;

        if (angle is > 15f and <= 75f)
            return EMoveDir.UpLeft;
        else if (angle is > 75f and <= 105f)
            return EMoveDir.Left;
        else if (angle is > 105f and <= 160f)
            return EMoveDir.DownLeft;
        else if (angle is > 160f or <= -160f)
            return EMoveDir.Down;
        else if (angle is < -15f and >= -75f)
            return EMoveDir.UpRight;
        else if (angle is < -75f and >= -105f)
            return EMoveDir.Right;
        else if (angle is < -105f and >= -160f)
            return EMoveDir.DownRight;
        else
            return EMoveDir.Up;
    }
}
