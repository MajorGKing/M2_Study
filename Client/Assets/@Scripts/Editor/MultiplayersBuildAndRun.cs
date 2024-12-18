using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class MultiplayersBuildAndRun
{
    #region Run
    [MenuItem("Tools/Run Multiplayer (No Build)/1 Players")]
    static void PerformRun1()
    {
        RunExe(1);
    }

    [MenuItem("Tools/Run Multiplayer (No Build)/2 Players")]
    static void PerformRun2()
    {
        RunExe(2);
    }

    [MenuItem("Tools/Run Multiplayer (No Build)/3 Players")]
    static void PerformRun3()
    {
        RunExe(3);
    }

    [MenuItem("Tools/Run Multiplayer (No Build)/4 Players")]
    static void PerformRun4()
    {
        RunExe(4);
    }

    #endregion

    #region Build & Run

    [MenuItem("Tools/Run Multiplayer (With Build)/2 Players")]
    static void PerformWin64Build2()
    {
        PerformWin64Build(2);
    }

    [MenuItem("Tools/Run Multiplayer (With Build)/3 Players")]
    static void PerformWin64Build3()
    {
        PerformWin64Build(3);
    }

    [MenuItem("Tools/Run Multiplayer (With Build)/4 Players")]
    static void PerformWin64Build4()
    {
        PerformWin64Build(4);
    }

    #endregion

    static void PerformWin64Build(int playerCount)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);

        // 해상도 설정
        PlayerSettings.defaultScreenWidth = 400;
        PlayerSettings.defaultScreenHeight = 400;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

        for (int i = 1; i <= playerCount; i++)
        {
            BuildPipeline.BuildPlayer(GetScenePaths(),
                "Builds/Win64/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe",
                BuildTarget.StandaloneWindows64, BuildOptions.AutoRunPlayer);
        }
    }

    static void RunExe(int playerCount)
    {
        List<Process> _processes = new List<Process>();
        for (int i = 1; i <= playerCount; i++)
        {
            string exePath = "Builds/Win64/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe";
            _processes.Add(RunProcess(exePath));
        }
    }

    static Process RunProcess(string exePath)
    {
        Process process = new Process();
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = ""; // 필요한 경우 명령줄 인수 추가
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.EnableRaisingEvents = true;

        process.Exited += (sender, e) =>
        {
            Process proc = (Process)sender;
            // 프로세스가 종료된 후 추가 작업 수행 (선택 사항)
            // string output = proc.StandardOutput.ReadToEnd();
            // string error = proc.StandardError.ReadToEnd();
        };

        process.Start();

        return process;
    }

    static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];

        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        return scenes;
    }
}
