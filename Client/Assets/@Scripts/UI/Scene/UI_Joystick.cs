using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_Joystick : UI_Scene
{
	enum GameObjects
	{
		JoystickBG,
		JoystickCursor,
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

		GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
		GetComponent<Canvas>().worldCamera = Camera.main;
	}

	#region Event
	public void OnPointerDown(PointerEventData evt)
	{
		_touchPos = Input.mousePosition;

		Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		// _background.transform.position = mouseWorldPos;
		// _cursor.transform.position = mouseWorldPos;

		//Managers.Game.JoystickState = ETouchEvent.PointerDown;
	}

	public void OnPointerUp(PointerEventData evt)
	{
		// _background.transform.position = _touchPos;
		_cursor.transform.localPosition = Vector3.zero;

		//Managers.Game.MoveDir = Vector2.zero;
		//Managers.Game.JoystickState = ETouchEvent.PointerUp;
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector2 touchDir = (eventData.position - _touchPos);

		float moveDist = Mathf.Min(touchDir.magnitude, _radius);
		Vector2 moveDir = touchDir.normalized;
		Vector2 newPosition = _touchPos + moveDir * moveDist;

		Vector2 worldPos = Camera.main.ScreenToWorldPoint(newPosition);
		_cursor.transform.position = worldPos;

		//Managers.Game.MoveDir = moveDir;
		//Managers.Game.JoystickState = ETouchEvent.Drag;
	}
	#endregion
}
