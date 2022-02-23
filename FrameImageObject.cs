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
    [DllImport("OpenCVDLL")]
    private static extern void makeColormap(ref Color32[] rawImage, int width, int height, int flag);   //only use this script



    public RawImage image;

    private Texture2D texture;
    private Texture2D textureInterpolation;
    private Texture2D textureCrop;

    public void SetImage(Color32[] imageData, Color32[] imageInterpolation, int imageWidth, int imageHeight, int type, int startidx, int endidx, int CropWidthMin, int CropWidthMax, int CropHeightMin, int CropHeightMax, int frameCount)
    {
        if (texture != null)
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

        for (int i = 0; i < imageInterpolation.Length; i++)
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
            textureInterpolation.SetPixels32(imageInterpolation);
            textureInterpolation.Apply();
        }
        else
        {
            //do nothing
        }
        textureCrop = new Texture2D(440, 260, TextureFormat.RGB24, false);
        Color32[] frameCropImage = new Color32[260 * 440];
        int cropIdx = 0;
        for (int k = 0; k < imageInterpolation.Length; k++)
        {
            if (k % 520 >= CropHeightMin && k % 520 < CropHeightMax && k / 520 >= CropWidthMin && k / 520 < CropWidthMax)
            {
                frameCropImage[cropIdx].r = imageInterpolation[k].r;
                frameCropImage[cropIdx].g = imageInterpolation[k].g;
                frameCropImage[cropIdx].b = imageInterpolation[k].b;
                cropIdx++;
            }
        }
        textureCrop.SetPixels32(frameCropImage);
        textureCrop.Apply();

        /*
        // make grayscale
        float r = 0.299f;
        float g = 0.587f;
        float b = 0.114f;
        for (int k = 0; k < frameCropImage.Length; k++)
        {
            float tmpr = frameCropImage[k].r;
            float tmpb = frameCropImage[k].b;
            float tmpg = frameCropImage[k].g;

            float color = (r * tmpr + g * tmpg + b * tmpb) * 255f;
            frameCropImage[k].r = (byte)color;
            frameCropImage[k].g = (byte)color;
            frameCropImage[k].b = (byte)color;
        }
        */
        makeColormap(ref frameCropImage, 260, 420, 0);
        textureCrop.SetPixels32(frameCropImage);
        textureCrop.Apply();

        int PeakForceVal = 999;
        int PeakForceIdxMax = 0;
        int PeakForceIdxMin = 0;

        for (int i = 0; i < frameCropImage.Length; i++)
        {
            int value = frameCropImage[i].r;
            if (value < PeakForceVal)
            {
                PeakForceVal = value;
                PeakForceIdxMin = i;
                PeakForceIdxMax = i;
            }
            else if (value == PeakForceVal)
            {
                PeakForceIdxMax = i;
            }
        }
        int PeakforceMaxX = PeakForceIdxMax % 260;
        int PeakforceMinX = PeakForceIdxMin % 260;

        int PeakforceMaxY = PeakForceIdxMax / 440;
        int PeakforceMinY = PeakForceIdxMin / 440;

        int PeakForceX = (PeakforceMaxX + PeakforceMinX) / 2;
        int PeakForceY = (PeakforceMaxY + PeakforceMinY) / 2;

        DynamicHandler.PeakForceXHistory[frameCount] = PeakForceX;
        DynamicHandler.PeakForceYHistory[frameCount] = PeakForceY;

        makeColormap(ref frameCropImage, 260, 420, 1);
        textureCrop.SetPixels32(frameCropImage);
        textureCrop.Apply();

        for (int i = 0; i < frameCropImage.Length; i++)
        {
            if(frameCropImage[i].r == 0 && frameCropImage[i].g == 0 && frameCropImage[i].b == 128)
            {
                frameCropImage[i].r = 255;
                frameCropImage[i].g = 255;
                frameCropImage[i].b = 255;
            }
        }
        textureCrop.SetPixels32(frameCropImage);
        textureCrop.Apply();

        image.texture = textureCrop;
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