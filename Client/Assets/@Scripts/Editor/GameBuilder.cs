using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public class GameBuilder : MonoBehaviour
{
    [MenuItem("Build/Build Android")]
    public static void BuildAndroid()
    {
        // TODO 어드레서블 프로파일 변경
        // AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        // string profileID = settings.profileSettings.GetProfileId(Define.EBuildType.Remote.ToString());
        // settings.activeProfileId = profileID;        

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[]
            { "Assets/@Scenes/TitleScene.unity", "Assets/@Scenes/GameScene.unity" };

        string buildFolder = "./Builds";
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        string date = DateTime.Now.ToString("yyyyMMdd");
        buildPlayerOptions.locationPathName = $"{buildFolder}/{date}_M2.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        PlayerSettings.Android.keystorePass = "rookiss";
        PlayerSettings.Android.keyaliasName = "rookiss";
        PlayerSettings.Android.keyaliasPass = "rookiss";
        PlayerSettings.Android.renderOutsideSafeArea = false;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }


}