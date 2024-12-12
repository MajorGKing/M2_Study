#if UNITY_EDITOR
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

public class CaptureScreenShot : MonoBehaviour
{
    private RecorderController _recorderController;

    private void OnEnable()
    {
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        _recorderController = new RecorderController(controllerSettings);

        var mediaOutputFolder = Path.Combine(Application.dataPath, "../..", "ScreenShots");

        var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        // imageRecorder.name = DateTime.Now.ToString("yy-MM-dd HH:mm:ss tt");
        imageRecorder.name = "M1_Screenshot";
        imageRecorder.Enabled = true;
        imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        imageRecorder.CaptureAlpha = false;

        imageRecorder.OutputFile = Path.Combine(mediaOutputFolder, "image_") + DefaultWildcard.Take;

        imageRecorder.imageInputSettings = new GameViewInputSettings
        {
            OutputWidth = 2960,
            OutputHeight = 1440,
        };
        
        controllerSettings.AddRecorderSettings(imageRecorder);
        controllerSettings.SetRecordModeToSingleFrame(0);
    }

    private void OnGUI()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _recorderController.PrepareRecording();
            _recorderController.StartRecording();
        }
    }
}

#endif
