using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class FrameImageObject : MonoBehaviour
{
    [DllImport("OpenCVDLL")]
    private static extern void interpolationTexture(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, int scale);
    [DllImport("OpenCVDLL")]
    private static extern void addResultLine(ref Color32[] rawImage, int width, int height, int scale, int[] COPXHistory, int[] COPYHistory, int[] PeakForceXHistory, int[] PeakForceYHistory, int frameStart, int frameEnd);
    [DllImport("OpenCVDLL")]
    private static extern void interpolationResult(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, int scale);
    [DllImport("OpenCVDLL")]
    private static extern void getColormap(ref Color32[] rawImage, int width, int height, int scale);   //only use this script



    public RawImage image;

    private Texture2D texture;
    private Texture2D textureInterpolation;

    public void SetImage(Color32[] imageData, Color32[] imageInterpolation, int imageWidth, int imageHeight, int type, int startidx , int endidx)
    {
        if(texture != null)
        {
            Destroy(texture);
            Destroy(textureInterpolation);
        }

        texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        texture.SetPixels32(imageData);
        texture.Apply();

        interpolationResult(ref imageData, ref imageInterpolation, imageWidth, imageHeight, StaticHandler.SCALE);
        textureInterpolation = new Texture2D(imageWidth * StaticHandler.SCALE, imageHeight * StaticHandler.SCALE, TextureFormat.RGB24, false);
        textureInterpolation.SetPixels32(imageInterpolation);
        textureInterpolation.Apply();

        getColormap(ref imageInterpolation, imageWidth, imageHeight, StaticHandler.SCALE);    //set colormap jet

        for(int i = 0; i< imageInterpolation.Length; i++)
        {
            if (imageInterpolation[i].r == 0 && imageInterpolation[i].g == 0 && imageInterpolation[i].b > 110)
            {
                imageInterpolation[i].r = 255;
                imageInterpolation[i].g = 255;
                imageInterpolation[i].b = 255;
            }
        }
        textureInterpolation.SetPixels32(imageInterpolation);
        textureInterpolation.Apply();

        if (type == 1)   //dynamic 
        {
            //set interpolation
            //addResultLine(ref imageInterpolation, StaticHandler.WIDTH, StaticHandler.HEIGHT, StaticHandler.SCALE, DynamicHandler.COPXHistory, DynamicHandler.COPYHistory, DynamicHandler.PeakForceXHistory, DynamicHandler.PeakForceYHistory, startidx, endidx);
            textureInterpolation.SetPixels32(imageInterpolation);
            textureInterpolation.Apply();
        }
        else
        {
            //do nothing
        }   
        image.texture = textureInterpolation;
    }

    private void OnDestroy()
    {
        Destroy(texture);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
