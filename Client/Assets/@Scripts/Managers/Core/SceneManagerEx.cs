using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }
    public EScene NextSceneType;

    public void LoadScene(EScene type, Transform parents = null)
    {
        NextSceneType = type;
        Managers.Clear();
        SceneManager.LoadScene(GetSceneName(EScene.LoadingScene));
    }

    public string GetSceneName(EScene type)
    {
        string name = System.Enum.GetName(typeof(EScene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
