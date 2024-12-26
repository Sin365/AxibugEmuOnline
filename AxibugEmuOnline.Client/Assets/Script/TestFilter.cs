#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TestFilter : MonoBehaviour
{
    public Material filterMat;
    public RawImage img;
    public Texture2D source;

    private RenderTexture rt;
    private RenderTexture rtWrap;

    private void Update()
    {
        if (rt == null)
        {
            rt = RenderTexture.GetTemporary(Screen.width, Screen.height);
            rtWrap = RenderTexture.GetTemporary(Screen.width, Screen.height);
        }
        else if (rt.width != Screen.width || rt.height != Screen.height)
        {
            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.ReleaseTemporary(rtWrap);

            rt = RenderTexture.GetTemporary(Screen.width, Screen.height);
            rtWrap = RenderTexture.GetTemporary(Screen.width, Screen.height);
        }

        filterMat.SetVector("_iResolution", new Vector2(rt.width, rt.height));
        filterMat.SetTexture("_Source", source);

        CommandBuffer cmd = new CommandBuffer();
        {
            cmd.Blit(source, rt);
            for (int i = 0; i < filterMat.shader.passCount; i++)
            {
                cmd.Blit(rt, rtWrap, filterMat, i);
                cmd.Blit(rtWrap, rt);
            }
        }
        Graphics.ExecuteCommandBuffer(cmd);
        //cmd.Release();

        img.texture = rt;
    }
}
#endif
