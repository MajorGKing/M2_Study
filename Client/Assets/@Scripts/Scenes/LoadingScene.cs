using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class LoadingScene : BaseScene
{
    private UI_LoadingPopup _ui;
    public EScene _nextSceneType;


    protected override void Awake()
    {
        base.Awake();
        SceneType = EScene.LoadingScene;
        _ui = Managers.UI.ShowPopupUI<UI_LoadingPopup>();
    }

    protected override void Start()
    {
        base.Start();

        _nextSceneType = Managers.Scene.NextSceneType;
        StartCoroutine(LoadNextScene());
    }

    public override void Clear()
    {
    }

    IEnumerator LoadNextScene()
    {
        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(Managers.Scene.GetSceneName(_nextSceneType));
        operation.allowSceneActivation = false; // 씬의 자동 전환 false

        while (!operation.isDone)
        {
            // 진행 상태를 업데이트합니다.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log(progress);
            // progressBar.value = progress;
            // progressText.text = progress * 100f + "%";

            // 로딩이 완료되면 씬을 전환합니다.
            if (operation.progress >= 0.9f)
            {
                // progressBar.value = 1f;
                // progressText.text = "100%";
                operation.allowSceneActivation = true;
                Managers.Clear();
            }

            yield return null;
        }
    }
}