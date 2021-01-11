using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotHandler : MonoBehaviour
{
    private static ScreenshotHandler instance;

    private Camera cam;
    private bool takeScreenshotNextFrame;
    private string screenshotName;
    private const string screenshotFolder = "/../Images/";

    void Awake()
    {
        instance = this;
        cam = gameObject.GetComponent<Camera>();
    }

    private void OnPostRender()
    {
        if (takeScreenshotNextFrame)
        {
            takeScreenshotNextFrame = false;
            RenderTexture renderTexture = cam.targetTexture;

            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            byte[] byteArray = renderResult.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + screenshotFolder + screenshotName + ".png", byteArray);

            RenderTexture.ReleaseTemporary(renderTexture);
            cam.targetTexture = null;
        }
    }

    private void SetTextureAndName(int width, int height, string name)
    {
        cam.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        screenshotName = name;
        takeScreenshotNextFrame = true;
    }

    public static void TakeScreenshot(int width, int height, string name)
    {
        instance.SetTextureAndName(width, height, name);
    }

}
