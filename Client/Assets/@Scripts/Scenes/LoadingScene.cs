using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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
        operation.allowSceneActivation = false; // ���� �ڵ� ��ȯ false

        while (!operation.isDone)
        {
            // ���� ���¸� ������Ʈ�մϴ�.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log(progress);
            // progressBar.value = progress;
            // progressText.text = progress * 100f + "%";

            // �ε��� �Ϸ�Ǹ� ���� ��ȯ�մϴ�.
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