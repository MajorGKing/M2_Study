using UnityEngine;

public class UI_Popup : UI_Base
{
    [HideInInspector] public Canvas UICanvas { get; set; }

    protected override void Awake()
    {
        base.Awake();

        UICanvas = Managers.UI.SetCanvas(gameObject);
        //PopupOpenAnimation(gameObject);
    }
    
    public virtual void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI(this);
    }

}
