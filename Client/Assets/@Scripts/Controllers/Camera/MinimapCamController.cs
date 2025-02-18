using UnityEngine;

namespace Scripts.Contents.Map
{
    public class MinimapCamController : MonoBehaviour
    {
        MyHero _myHero;
        Camera _cam;

        public void SetInfo(MyHero hero)
        {
            _myHero = hero;
            transform.localPosition = Vector3.back;
            _cam = GetComponent<Camera>();
            _cam.orthographicSize = 30;
            _cam.cullingMask = 1 << LayerMask.NameToLayer("Minimap");
        }
    } 
}
