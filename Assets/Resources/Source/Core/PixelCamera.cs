using UnityEngine;

public class PixelCamera : MonoBehaviour
{
    Camera blackBarCamera, currentCamera;

    void Start()
    {
        blackBarCamera = new GameObject("BlackBar Camera").AddComponent<Camera>();
        blackBarCamera.transform.SetParent(GetComponent<Camera>().transform);
        blackBarCamera.transform.position = Vector3.zero;
        blackBarCamera.clearFlags = CameraClearFlags.SolidColor;
        blackBarCamera.backgroundColor = new Color32(15, 16, 18, 0);
        blackBarCamera.depth = GetComponent<Camera>().depth - 1;
        currentCamera = GetComponent<Camera>();
        Execute();
    }

    void Update() => Execute();

    void Execute()
    {
        float targetratio = 16.0f / 9.0f;
        float windowratio = UnityEngine.Screen.width / (float)UnityEngine.Screen.height;
        float scaleheight = windowratio / targetratio;
        Rect rect = currentCamera.rect;
        if (scaleheight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;
        }
        else
        {
            float scalewidth = 1.0f / scaleheight;
            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;
        }
        currentCamera.rect = rect;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Core.renderWidth <= 0) return;
        RenderTexture buffer = RenderTexture.GetTemporary(Core.renderWidth, Core.renderHeight, -1);
        buffer.filterMode = Settings.settings.pixelPerfectVision ? FilterMode.Point : FilterMode.Bilinear;
        source.filterMode = Settings.settings.pixelPerfectVision ? FilterMode.Point : FilterMode.Bilinear;
        Graphics.Blit(source, buffer);
        Graphics.Blit(buffer, destination);
        RenderTexture.ReleaseTemporary(buffer);
    }
}