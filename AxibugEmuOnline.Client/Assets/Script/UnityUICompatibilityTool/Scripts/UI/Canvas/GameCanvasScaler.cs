//Made by Olsc

using UnityEngine.UI;

public class GameCanvasScaler : CanvasScaler
{
    protected override void Start()
    {
        uiScaleMode = ScaleMode.ScaleWithScreenSize;
        //referenceResolution = new UnityEngine.Vector2(2340, 1080);
    }
}