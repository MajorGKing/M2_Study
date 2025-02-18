using UnityEngine;

public class CameraController : MonoBehaviour
{
    public BaseObject Target { get; set; }

	private void Start()
	{
		Camera.main.orthographicSize = 12;
		Camera.main.cullingMask = ~(1 << LayerMask.NameToLayer("Minimap"));
	}

    private void LateUpdate()
    {
        if (Target == null)
            return;

        transform.position = new Vector3(Target.transform.position.x, Target.transform.position.y, -10f);
    }
}
