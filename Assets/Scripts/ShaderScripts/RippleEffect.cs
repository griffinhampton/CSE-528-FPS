using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RippleEffect : MonoBehaviour
{
    public int TextureSize = 512;
    public RenderTexture ObjectsRT;
    private RenderTexture CurrRT, PrevRT, TempRT;
    public Shader RippleShader, AddShader;
    private Material RippleMat, AddMat;
    // Start is called before the first frame update
    void Start()
    {
        //Creating render textures and materials
        CurrRT = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.RFloat);
        PrevRT = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.RFloat);
        TempRT = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.RFloat);
        RippleMat = new Material(RippleShader);
        AddMat = new Material(AddShader);

        // FIX: Clear the textures to remove VRAM garbage (static)
        Graphics.Blit(Texture2D.blackTexture, CurrRT);
        Graphics.Blit(Texture2D.blackTexture, PrevRT);
        Graphics.Blit(Texture2D.blackTexture, TempRT);

        // Change the texture in the material of this object
        GetComponent<Renderer>().material.SetTexture("_RippleTex", CurrRT);
    
        StartCoroutine(ripples());
    }

    // Update is called once per frame
    IEnumerator ripples()
    {
        // FIX: Use a loop instead of recursively calling StartCoroutine
        while (true)
        {
            // Copy the result of blending the render textures to TempRT.
            AddMat.SetTexture("_ObjectsRT", ObjectsRT);
            AddMat.SetTexture("_CurrentRT", CurrRT);
            Graphics.Blit(null, TempRT, AddMat);

            RenderTexture rt0 = TempRT;
            TempRT = CurrRT;
            CurrRT = rt0;

            // Calculate the ripple animation using ripple shader.
            RippleMat.SetTexture("_PrevRT", PrevRT);
            RippleMat.SetTexture("_CurrentRT", CurrRT);
            Graphics.Blit(null, TempRT, RippleMat);
            Graphics.Blit(TempRT, PrevRT);

            // Swap PrevRT and CurrentRT to calculate the result for the next frame.
            RenderTexture rt = PrevRT;
            PrevRT = CurrRT;
            CurrRT = rt;

            // Wait for one frame and then execute the loop again.
            yield return null;
        }
    }
}
