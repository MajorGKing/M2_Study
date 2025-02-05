using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
    public bool TestMode = true;

    public Define.EScene SceneType { get; protected set; } = Define.EScene.Unknown;

    protected virtual void Awake()
	{
		#if UNITY_EDITOR
        TestMode = true;
		#else
		TestMode = false;
		#endif
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
		if (obj == null)
			Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
	}

	protected virtual void Start() { }
	protected virtual void Update() { }

	public abstract void Clear();
}
