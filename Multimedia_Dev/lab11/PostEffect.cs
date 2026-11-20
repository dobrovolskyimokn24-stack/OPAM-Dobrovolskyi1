using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostEffect : MonoBehaviour
{
    public Material material;
    public bool twoPass = false;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material == null)
        {
            Graphics.Blit(src, dst);
            return;
        }

        if (!twoPass)
        {
            Graphics.Blit(src, dst, material);
            return;
        }

        RenderTexture tmp = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
        Graphics.Blit(src, tmp, material, 0);
        Graphics.Blit(tmp, dst, material, 1);
        RenderTexture.ReleaseTemporary(tmp);
    }
}