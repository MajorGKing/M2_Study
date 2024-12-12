using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    public Define.EScene SceneType { get; protected set; } = Define.EScene.Unknown;

    protected virtual void Awake()
	{
		Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
		if (obj == null)
			Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
	}

	protected virtual void Start() { }
	protected virtual void Update() { }

	public abstract void Clear();
}
