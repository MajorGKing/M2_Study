using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
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
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(GetSceneName(EScene.LoadingScene), LoadSceneMode.Single, true);
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
