using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ShowResultHandler : MonoBehaviour
{
    [DllImport("OpenCVDLL")]
    private static extern void interpolationTexture(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, double widthScale, double heightScale, int flag);
    [DllImport("OpenCVDLL")]
    private static extern void interpolationResult(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, int scale);
    [DllImport("OpenCVDLL")]
    private static extern void addPointLine(ref Color32[] rawImage, int width, int height, int scale, int FrameStart, int FrameEnd, int FrameMid, int FrameBot);
    [DllImport("OpenCVDLL")]
    private static extern void addResultLine(ref Color32[] rawImage, int width, int height, int scale, int[] COPXHistory, int[] COPYHistory, int[] PeakForceXHistory, int[] PeakForceYHistory, int frameStart, int frameEnd);
    [DllImport("OpenCVDLL")]
    private static extern void addDynamicResultLine(ref Color32[] rawImage, int width, int height, int[] copXHistory, int[] copYHistory, int startIdx, int endIdx);


    // Start is called before the first frame update

    public Image tmp;
    public Image tmp2;
    public Image[] clone;

    public FrameImageObject prefab;
    public FrameImageObject[] frameClones;

    public Image leftResultImage;
    public Image rightResultImage;
    public Image leftCropImage;
    public Image rightCropImage;

    public int leftStartY = 999;
    public int leftEndY = -1;
    public int rightStartY = 999;
    public int rightEndY = -1;
    public int leftFrameSection1;
    public int leftFrameSection2;
    public int rightFrameSection1;
    public int rightFrameSection2;

    public Color[] wedgeColors;
    public Image wedgePrefab;
    public static float[] leftValueArray;
    public static float[] rightValueArray;

    void goBackScene()
    {
        SceneManager.LoadScene("DynamicScene");
    }
    void Start()
    {
        startLoop();
    }

    public void startLoop()
    {

    }
    void Update()
    {
        if (DynamicHandler.STATUS == (int)DynamicHandler.MODE.END && !StaticVaribleHandler.isEndDynamic)
        {
            getResultImage();

            Vector3 ScrollContentVector;
            ScrollContentVector = GameObject.Find("LeftScrollContent").transform.position;
            GameObject.Find("LeftScrollContent").transform.position = ScrollContentVector;
            GameObject.Find("RightScrollContent").transform.position = ScrollContentVector;

            frameClones = new FrameImageObject[100];

            for (int i = 0; i < DynamicHandler.leftIdx; i++)
            {
                frameClones[i] = Instantiate(prefab, GameObject.Find("LeftScrollContent").transform);
                frameClones[i].name = "Frame_" + i;
                frameClones[i].transform.localPosition = new Vector3(0, 0, 0);
                frameClones[i].transform.localScale = new Vector3(-1, 0, 0);

                var cloneImage = new Color32[StaticHandler.WIDTH * StaticHandler.HEIGHT];
                var cloneInterpolation = new Color32[StaticHandler.WIDTH * StaticHandler.SCALE * StaticHandler.HEIGHT * StaticHandler.SCALE];
                for (int j = 0; j < cloneImage.Length; j++)
                {
                    cloneImage[j] = new Color32
                    {
                        r = DynamicHandler.DynamicFrameRecordArray[i, j],
                        g = DynamicHandler.DynamicFrameRecordArray[i, j],
                        b = DynamicHandler.DynamicFrameRecordArray[i, j],
                        a = 255
                    };
                }
                frameClones[i].SetImage(cloneImage, cloneInterpolation, StaticHandler.WIDTH, StaticHandler.HEIGHT, 1, 0, i);
            }

            for (int i = DynamicHandler.leftIdx; i < DynamicHandler.rightIdx; i++)
            {
                frameClones[i] = Instantiate(prefab, GameObject.Find("RightScrollContent").transform);
                frameClones[i].name = "Frame_" + i;
                frameClones[i].transform.localScale = new Vector3(-1, 0, 0);

                var cloneImage = new Color32[StaticHandler.WIDTH * StaticHandler.HEIGHT];
                var cloneInterpolation = new Color32[StaticHandler.WIDTH * StaticHandler.SCALE * StaticHandler.HEIGHT * StaticHandler.SCALE];
                for (int j = 0; j < cloneImage.Length; j++)
                {
                    cloneImage[j] = new Color32
                    {
                        r = DynamicHandler.DynamicFrameRecordArray[i, j],
                        g = DynamicHandler.DynamicFrameRecordArray[i, j],
                        b = DynamicHandler.DynamicFrameRecordArray[i, j],
                        a = 255
                    };
                }
                frameClones[i].SetImage(cloneImage, cloneInterpolation, StaticHandler.WIDTH, StaticHandler.HEIGHT, 1, DynamicHandler.leftIdx, i);

            }
            getFrameArch(leftFrameSection1, leftFrameSection2, rightFrameSection1, rightFrameSection2);
            StaticVaribleHandler.isEndDynamic = true;
        }
    }

    void getResultImage()
    {
        var leftResult = leftResultImage.sprite.texture.GetPixels32();
        var rightResult = rightResultImage.sprite.texture.GetPixels32();
        var leftCrop = leftCropImage.sprite.texture.GetPixels32();
        var rightCrop = rightCropImage.sprite.texture.GetPixels32();
        var tmpResult = tmp.sprite.texture.GetPixels32();
        var tmpResult2 = tmp2.sprite.texture.GetPixels32();

        //left
        for (int i = 0; i < tmpResult.Length; i++)
        {
            DynamicHandler.leftImageDetectedSum[i] = (DynamicHandler.leftImageDetectedSum[i] / DynamicHandler.leftIdx);
            if (DynamicHandler.leftImageDetectedSum[i] >= 255)
            {
                DynamicHandler.leftImageDetectedSum[i] = 255;
            }
            if (DynamicHandler.leftImageDetectedSum[i] <= 5)
            {
                DynamicHandler.leftImageDetectedSum[i] = 0;
            }
            tmpResult[i] = new Color32
            {
                r = (byte)(DynamicHandler.leftImageDetectedSum[i] > 0 ? 0 : 255),
                g = (byte)(DynamicHandler.leftImageDetectedSum[i] > 0 ? 0 : 255),
                b = (byte)(DynamicHandler.leftImageDetectedSum[i] > 0 ? 0 : 255)
            };

            if (DynamicHandler.leftImageDetectedSum[i] > 0)
            {
                int tmp = i % StaticHandler.WIDTH;

                if (leftStartY > tmp)
                {
                    leftStartY = tmp;
                }
                if (leftStartY != 999 && leftEndY < tmp)
                {
                    leftEndY = tmp;
                }
            }
        }
        leftFrameSection1 = (leftEndY - leftStartY) / 3 + leftStartY;
        leftFrameSection2 = (leftEndY - leftStartY) / 3 * 2 + leftStartY;


        Debug.Log("leftstartY :" + leftStartY + " leftEndY : " + leftEndY + " leftFrameSction1 : " + leftFrameSection1 + " leftFrameSection2 : " + leftFrameSection2);
        tmp.sprite.texture.SetPixels32(tmpResult);
        tmp.sprite.texture.Apply();
        interpolationResult(ref tmpResult, ref leftResult, StaticHandler.WIDTH, StaticHandler.HEIGHT, StaticHandler.SCALE);
        tmp.sprite.texture.SetPixels32(tmpResult);
        tmp.sprite.texture.Apply();
        leftResultImage.sprite.texture.SetPixels32(leftResult);
        leftResultImage.sprite.texture.Apply();
        addResultLine(ref leftResult, StaticHandler.WIDTH, StaticHandler.HEIGHT, StaticHandler.SCALE, DynamicHandler.COPXHistory, DynamicHandler.COPYHistory, DynamicHandler.PeakForceXHistory, DynamicHandler.PeakForceYHistory, 0, DynamicHandler.leftIdx);

        leftResultImage.sprite.texture.SetPixels32(leftResult);
        leftResultImage.sprite.texture.Apply();

        //addPointLine(ref leftResult, StaticHandler.WIDTH, StaticHandler.HEIGHT, StaticHandler.SCALE, leftStartY, leftEndY, leftFrameSection1, leftFrameSection2);

        int minWidth = 999;
        int maxWidth = 0;
        int minHeight = 999;
        int maxHeight = 0;
        int widthidx;
        int heightidx;
        for (int i = 0; i < leftResult.Length; i++)
        {
            if (leftResult[i].r == 0 && leftResult[i].b > 0)
            {
                leftResult[i].r = 255;
                leftResult[i].g = 255;
                leftResult[i].b = 255;
            }
            else
            {
                leftResult[i].r = 0;
                leftResult[i].g = 0;
                leftResult[i].b = 0;

                widthidx = i / (StaticHandler.WIDTH * StaticHandler.SCALE);
                heightidx = i % (StaticHandler.WIDTH * StaticHandler.SCALE);

                if (widthidx < minWidth)
                {
                    minWidth = widthidx;
                }
                else if (widthidx > maxWidth)
                {
                    maxWidth = widthidx;
                }

                if (heightidx < minHeight)
                {
                    minHeight = heightidx;
                }
                else if (heightidx > maxHeight)
                {
                    maxHeight = heightidx;
                }

            }

        }
        int widthavg;
        int widthmin;
        int widthmax;
        int heightavg;
        int heightmin;
        int heightmax;

        widthavg = (maxWidth + minWidth) / 2;
        heightavg = (maxHeight + minHeight) / 2;

        if (widthavg - 130 < 0)
        {
            widthmin = 0;
            widthmax = 260;
        }
        else if (widthavg + 130 > 440)
        {
            widthmax = 440;
            widthmin = 180;
        }
        else
        {
            widthmin = widthavg - 130;
            widthmax = widthavg + 130;
        }

        if (heightavg - 220 < 0)
        {
            heightmin = 0;
            heightmax = 440;
        }
        else if (heightavg + 220 > 520)
        {
            heightmax = 520;
            heightmin = 80;
        }
        else
        {
            heightmin = heightavg - 220;
            heightmax = heightavg + 220;
        }
        /*
        int widthavg = (maxWidth + minWidth) / 2;
        int widthmin = widthavg - 130 > 0 ? widthavg - 130 : 0;
        int widthmax = widthmin > 0 ? widthavg + 130 : 260;
        int heightavg = (maxHeight + minHeight) / 2;
        int heightmin = heightavg - 220 > 0 ? heightavg - 220 : 0;
        int heightmax = heightmin > 0 ? heightavg + 220 : 440;
        */
        Debug.Log("#1 width avg : " + widthavg + " height avg : " + heightavg);


        //crop point
        for (int i = 0; i < DynamicHandler.leftIdx; i++)
        {
            DynamicHandler.PeakForceXHistory[i] -= heightmin;
            DynamicHandler.PeakForceYHistory[i] -= widthmin;
            DynamicHandler.COPXHistory[i] -= heightmin;
            DynamicHandler.COPYHistory[i] -= widthmin;
        }
        //crop image
        int cropIdx = 0;
        for (int i = 0; i < leftResult.Length; i++)
        {
            if (i % 520 >= heightmin && i % 520 < heightmax && i / 520 >= widthmin && i / 520 < widthmax)
            {
                leftCrop[cropIdx].r = leftResult[i].r;
                leftCrop[cropIdx].g = leftResult[i].g;
                leftCrop[cropIdx].b = leftResult[i].b;
                cropIdx++;
            }
        }
        leftResultImage.sprite.texture.SetPixels32(leftResult);
        leftResultImage.sprite.texture.Apply();

        //interpolationTexture(ref leftResult, ref leftCrop, StaticHandler.WIDTH * StaticHandler.SCALE, StaticHandler.HEIGHT * StaticHandler.SCALE, 0.84615, 0.5909, 0);

        //addDynamicResultLine(ref leftCrop, 440, 260, DynamicHandler.COPXHistory, DynamicHandler.COPYHistory, 0, DynamicHandler.leftIdx);
        leftCropImage.sprite.texture.SetPixels32(leftCrop);
        leftCropImage.sprite.texture.Apply();

        Debug.Log("crop length : " + leftCrop.Length + " cropidx : " + cropIdx);

        //Right
        int total_right = DynamicHandler.rightIdx - DynamicHandler.leftIdx;

        for (int i = 0; i < tmpResult2.Length; i++)
        {
            //DynamicHandler.rightImageDetectedSum[i] = (DynamicHandler.rightImageDetectedSum[i] / total_right);
            DynamicHandler.rightImageDetectedSum[i] = (DynamicHandler.rightImageDetectedSum[i] / DynamicHandler.rightIdx);
            if (DynamicHandler.rightImageDetectedSum[i] >= 255)
            {
                DynamicHandler.rightImageDetectedSum[i] = 255;
            }
            if (DynamicHandler.rightImageDetectedSum[i] <= 5)
            {
                DynamicHandler.rightImageDetectedSum[i] = 0;
            }
            tmpResult2[i] = new Color32
            {
                r = (byte)(DynamicHandler.rightImageDetectedSum[i] > 0 ? 0 : 255),
                g = (byte)(DynamicHandler.rightImageDetectedSum[i] > 0 ? 0 : 255),
                b = (byte)(DynamicHandler.rightImageDetectedSum[i] > 0 ? 0 : 255)
            };
            if (DynamicHandler.rightImageDetectedSum[i] > 0)
            {
                int tmp = i % StaticHandler.WIDTH;

                if (rightStartY > tmp)
                {
                    rightStartY = tmp;
                }
                if (rightStartY != 999 && rightEndY < tmp)
                {
                    rightEndY = tmp;
                }
            }
        }
        rightFrameSection1 = (rightEndY - rightStartY) / 3 + rightStartY;
        rightFrameSection2 = (rightEndY - rightStartY) / 3 * 2 + rightStartY;
        Debug.Log("rightStartY :" + rightStartY + " rightEndY : " + rightEndY + " rightFrameSection1 : " + rightFrameSection1 + " rightFrameSection2 : " + rightFrameSection2);

        tmp2.sprite.texture.SetPixels32(tmpResult2);
        tmp2.sprite.texture.Apply();
        interpolationResult(ref tmpResult2, ref rightResult, StaticHandler.WIDTH, StaticHandler.HEIGHT, StaticHandler.SCALE);

        tmp2.sprite.texture.SetPixels32(tmpResult2);
        tmp2.sprite.texture.Apply();
        rightResultImage.sprite.texture.SetPixels32(rightResult);
        rightResultImage.sprite.texture.Apply();
        addResultLine(ref rightResult, StaticHandler.WIDTH, StaticHandler.HEIGHT, StaticHandler.SCALE, DynamicHandler.COPXHistory, DynamicHandler.COPYHistory, DynamicHandler.PeakForceXHistory, DynamicHandler.PeakForceYHistory, DynamicHandler.leftIdx, DynamicHandler.rightIdx - 1);
        rightResultImage.sprite.texture.SetPixels32(rightResult);
        rightResultImage.sprite.texture.Apply();

        minWidth = 999;
        maxWidth = 0;
        minHeight = 999;
        maxHeight = 0;
        for (int i = 0; i < rightResult.Length; i++)
        {
            if (rightResult[i].r == 0 && rightResult[i].b > 0)
            {
                rightResult[i].r = 255;
                rightResult[i].g = 255;
                rightResult[i].b = 255;
            }
            else
            {
                rightResult[i].r = 0;
                rightResult[i].g = 0;
                rightResult[i].b = 0;

                widthidx = i / (StaticHandler.WIDTH * StaticHandler.SCALE);
                heightidx = i % (StaticHandler.WIDTH * StaticHandler.SCALE);

                if (widthidx < minWidth)
                {
                    minWidth = widthidx;
                }
                else if (widthidx > maxWidth)
                {
                    maxWidth = widthidx;
                }

                if (heightidx < minHeight)
                {
                    minHeight = heightidx;
                }
                else if (heightidx > maxHeight)
                {
                    maxHeight = heightidx;
                }
            }
        }

        widthavg = (maxWidth + minWidth) / 2;
        heightavg = (maxHeight + minHeight) / 2;

        if (widthavg - 130 < 0)
        {
            widthmin = 0;
            widthmax = 260;
        }
        else if (widthavg + 130 > 440)
        {
            widthmax = 440;
            widthmin = 180;
        }
        else
        {
            widthmin = widthavg - 130;
            widthmax = widthavg + 130;
        }

        if (heightavg - 220 < 0)
        {
            heightmin = 0;
            heightmax = 440;
        }
        else if (heightavg + 220 > 520)
        {
            heightmax = 520;
            heightmin = 80;
        }
        else
        {
            heightmin = heightavg - 220;
            heightmax = heightavg + 220;
        }
        /*
        widthmin = widthavg - 130 > 0 ? widthavg - 130 : 0;
        widthmax = widthmin > 0 ? widthavg + 130 : 260;
        
        heightavg = (maxHeight + minHeight) / 2;
        heightmin = heightavg - 220 > 0 ? heightavg - 220 : 0;
        heightmax = heightmin > 0 ? heightavg + 220 : 440;
        */

        Debug.Log("#2 width avg : " + widthavg + " height avg : " + heightavg);

        rightResultImage.sprite.texture.SetPixels32(rightResult);
        rightResultImage.sprite.texture.Apply();
        //crop point
        for (int i = total_right; i < DynamicHandler.rightIdx; i++)
        {
            DynamicHandler.PeakForceXHistory[i] -= heightmin;
            DynamicHandler.PeakForceYHistory[i] -= widthmin;
            DynamicHandler.COPXHistory[i] -= heightmin;
            DynamicHandler.COPYHistory[i] -= widthmin;
        }

        //crop Image
        cropIdx = 0;
        for (int i = 0; i < rightResult.Length; i++)
        {
            if (i % 520 >= heightmin && i % 520 < heightmax && i / 520 >= widthmin && i / 520 < widthmax)
            {
                rightCrop[cropIdx].r = rightResult[i].r;
                rightCrop[cropIdx].g = rightResult[i].g;
                rightCrop[cropIdx].b = rightResult[i].b;

                cropIdx++;
            }
        }
        Debug.Log("crop length : " + rightCrop.Length + " cropidx : " + cropIdx);

        // addDynamicResultLine(ref rightCrop, 440, 260, DynamicHandler.COPXHistory, DynamicHandler.COPYHistory, total_right, DynamicHandler.rightIdx);
        rightCropImage.sprite.texture.SetPixels32(rightCrop);
        rightCropImage.sprite.texture.Apply();

        rightResultImage.sprite.texture.SetPixels32(rightResult);
        rightResultImage.sprite.texture.Apply();
    }
    void getFrameArch(int leftSection1, int leftSection2, int rightSection1, int rightSection2)
    {
        int leftStartIdx;
        int leftEndIdx;
        int rightStartIdx;
        int rightEndIdx;

        double leftFrameContact = 0;
        double leftFrameMidStance = 0;
        double leftFramePropulsive = 0;

        int lastestShape = 0;
        int lastestStartIdx = 0;
        int lastestEndIdx = 0;

        for (int i = 0; i < DynamicHandler.leftIdx; i++)
        {
            leftStartIdx = 999;
            leftEndIdx = -1;
            for (int j = 0; j < StaticHandler.WIDTH * StaticHandler.HEIGHT; j++)
            {
                if (DynamicHandler.DynamicFrameRecordArray[i, j] != 255)
                {
                    int tmp = DynamicHandler.DynamicFrameRecordArray[i, j];
                    int tmp2 = j % StaticHandler.WIDTH;

                    if (leftStartIdx == 999)
                    {
                        leftStartIdx = tmp2;
                        leftEndIdx = tmp2;
                    }
                    if (leftStartIdx > tmp2)
                    {
                        leftStartIdx = tmp2;
                    }
                    else if (leftEndIdx < tmp2)
                    {
                        leftEndIdx = tmp2;
                    }
                }
            }

            if (leftStartY > leftStartIdx)
            {
                leftStartIdx = leftStartY;
            }
            if (leftEndY < leftEndIdx)
            {
                leftEndIdx = leftEndY;
            }

            Debug.Log("Frame" + i + " startidx = " + leftStartIdx + " endidx = " + leftEndIdx);
            //leftStartY : µÞ²ÞÄ¡ leftEndIdx : Á¾Àû°ñµÎ

            if (leftStartIdx <= leftSection1 && leftEndIdx <= leftSection2 || i == 0)
            {
                Debug.Log("Frame" + i + " is contact");
                leftFrameContact++;
                lastestEndIdx = leftEndIdx;
                lastestShape = 0;
            }
            else if (leftStartIdx <= leftSection1 && leftEndIdx >= leftSection2 && lastestShape != 2 && i < DynamicHandler.rightIdx - 1)
            {
                Debug.Log("Frame" + i + " is mid statce");
                if (lastestShape == 0)
                {
                    leftFrameContact--; //delete lastest frame

                    float firstindex = Math.Abs(leftSection2 - lastestEndIdx);
                    float secondindex = Math.Abs(leftEndIdx - leftSection2);
                    Debug.Log("firstindex : " + firstindex + " secondindex : " + secondindex);
                    float firstweight = firstindex * 2 / (firstindex + secondindex);
                    float secondweight = secondindex * 2 / (firstindex + secondindex);
                    Debug.Log("SHAPE CHANGE! contact : " + firstweight + " mid : " + secondweight);

                    leftFrameContact += firstweight;
                    leftFrameMidStance += secondweight;
                }
                else
                {
                    leftFrameMidStance++;
                }
                lastestShape = 1;
                lastestStartIdx = leftStartIdx;
                lastestEndIdx = leftEndIdx;
            }
            else if (leftStartIdx >= leftSection1 && leftEndIdx >= leftSection2 || lastestShape == 2 || i == DynamicHandler.rightIdx - 1)
            {
                Debug.Log("Frame" + i + " is propulsive");

                if (lastestShape == 1)
                {
                    leftFrameMidStance--;   //delete lastest frame

                    float firstindex = Math.Abs(leftSection1 - lastestStartIdx);
                    float secondindex = Math.Abs(leftStartIdx - leftSection1);

                    Debug.Log("firstindex : " + firstindex + " secondindex : " + secondindex);
                    float firstweight = firstindex * 2 / (firstindex + secondindex);
                    float secondweight = secondindex * 2 / (firstindex + secondindex);


                    leftFrameMidStance += firstweight;
                    leftFramePropulsive += secondweight;

                    Debug.Log("SHAPE CHANGE! mid : " + firstweight + " prop : " + secondweight);
                }
                else
                {
                    leftFramePropulsive++;
                }
                lastestShape = 2;
                lastestEndIdx = leftEndIdx;
            }
            else
            {
                Debug.Log("Frame" + i + "???");
            }
        }
        float leftFrameContactPercent = Convert.ToSingle(Math.Round(leftFrameContact / (leftFrameContact + leftFrameMidStance + leftFramePropulsive), 4) * 100);
        float leftFrameMidStancePercent = Convert.ToSingle(Math.Round(leftFrameMidStance / (leftFrameContact + leftFrameMidStance + leftFramePropulsive), 4) * 100);
        float leftFramePropulsivePercent = Convert.ToSingle(Math.Round(leftFramePropulsive / (leftFrameContact + leftFrameMidStance + leftFramePropulsive), 4) * 100);
        Debug.Log("Contact : " + leftFrameContactPercent + "% MidStance : " + leftFrameMidStancePercent + "% Propulsive : " + leftFramePropulsivePercent + "%");

        leftValueArray = new float[3];
        leftValueArray[0] = leftFrameContactPercent;
        leftValueArray[1] = leftFrameMidStancePercent;
        leftValueArray[2] = leftFramePropulsivePercent;

        while (true)
        {
            Debug.Log("before right y : " + rightEndY + " section1 : " + rightSection1 + " section2 : " + rightSection2);
            int result = getRightShape(rightSection1, rightSection2);
            if(result != rightEndY)
            {
                rightEndY = result;
                rightFrameSection1 = (rightEndY - rightStartY) / 3 + rightStartY;
                rightFrameSection2 = (rightEndY - rightStartY) / 3 * 2 + rightStartY;
                Debug.Log("after right y : " + result + " section1 : " + rightSection1 + " section2 : " + rightSection2);
            }
            else
            {
                break;
            }
        }
    }

    int getRightShape(int rightSection1, int rightSection2)
    {
        int rightStartIdx;
        int rightEndIdx;

        double rightFrameContact = 0;
        double rightFrameMidStance = 0;
        double rightFramePropulsive = 0;

        int lastestShape = 0;
        int lastestStartIdx = 0;
        int lastestEndIdx = 0;

        int rightMax = -1;
        for (int i = DynamicHandler.leftIdx; i < DynamicHandler.rightIdx; i++)
        {
            rightStartIdx = 999;
            rightEndIdx = -1;

            for (int j = 0; j < StaticHandler.WIDTH * StaticHandler.HEIGHT; j++)
            {
                if (DynamicHandler.DynamicFrameRecordArray[i, j] != 255)
                {
                    int tmp = DynamicHandler.DynamicFrameRecordArray[i, j];
                    int tmp2 = j % StaticHandler.WIDTH;

                    if (rightStartIdx == 999)
                    {
                        rightStartIdx = tmp2;
                        rightEndIdx = tmp2;
                    }
                    if (rightStartIdx > tmp2)
                    {
                        rightStartIdx = tmp2;
                    }
                    else if (rightEndIdx < tmp2)
                    {
                        rightEndIdx = tmp2;
                    }
                }
            }

            rightStartIdx = 52 - rightStartIdx;
            rightEndIdx = 52 - rightEndIdx;
            int t = 0;
            t = rightEndIdx;
            rightEndIdx = rightStartIdx;
            rightStartIdx = t;

            if (rightStartY > rightStartIdx)
            {
                rightStartIdx = rightStartY;
            }
            if (rightEndY < rightEndIdx)
            {
                rightEndIdx = rightEndY;
            }

            if(rightMax < rightEndIdx)
            {
                rightMax = rightEndIdx;
            }

            Debug.Log("Frame" + i + " startidx = " + rightStartIdx + " endidx = " + rightEndIdx);

            if (rightStartIdx <= rightSection1 && rightEndIdx <= rightSection2 || i == DynamicHandler.leftIdx)
            {
                Debug.Log("Frame" + i + " is contact");
                rightFrameContact++;
                lastestEndIdx = rightEndIdx;
                lastestShape = 0;
            }
            else if (rightStartIdx <= rightSection1 && rightEndIdx >= rightSection2 && lastestShape != 2 && i < DynamicHandler.rightIdx - 1)
            {
                /*
                if(lastestEndIdx > rightSection1)
                {
                    lastestEndIdx = rightSection1;
                }
                */
                Debug.Log("Frame" + i + " is mid statce");
                if (lastestShape == 0)
                {
                    rightFrameContact--; //delete lastest frame

                    float firstindex = Math.Abs(rightSection2 - lastestEndIdx);
                    float secondindex = Math.Abs(rightEndIdx - rightSection2);
                    Debug.Log("firstindex : " + firstindex + " secondindex : " + secondindex);
                    float firstweight = firstindex * 2 / (firstindex + secondindex);
                    float secondweight = secondindex * 2 / (firstindex + secondindex);
                    Debug.Log("SHAPE CHANGE! contact : " + firstweight + " mid : " + secondweight);

                    rightFrameContact += firstweight;
                    rightFrameMidStance += secondweight;
                }
                else
                {
                    rightFrameMidStance++;
                }
                lastestShape = 1;
                lastestStartIdx = rightStartIdx;
                lastestEndIdx = rightEndIdx;

            }
            else if (rightStartIdx >= rightSection1 && rightEndIdx >= rightSection2 || lastestShape == 2 || i == DynamicHandler.rightIdx - 1)
            {
                Debug.Log("Frame" + i + " is propulsive");

                if (lastestShape == 1)
                {
                    rightFrameMidStance--;   //delete lastest frame

                    float firstindex = Math.Abs(rightSection1 - lastestStartIdx);
                    float secondindex = Math.Abs(rightStartIdx - rightSection1);

                    Debug.Log("firstindex : " + firstindex + " secondindex : " + secondindex);
                    float firstweight = firstindex * 2 / (firstindex + secondindex);
                    float secondweight = secondindex * 2 / (firstindex + secondindex);

                    rightFrameMidStance += firstweight;
                    rightFramePropulsive += secondweight;

                    Debug.Log("SHAPE CHANGE! mid : " + firstweight + " prop : " + secondweight);
                }
                else
                {
                    rightFramePropulsive++;
                }
                lastestShape = 2;
                lastestEndIdx = rightEndIdx;
            }
            else
            {
                Debug.Log("Frame" + i + "???");
            }
        }



        float rightFrameContactPercent = Convert.ToSingle(Math.Round(rightFrameContact / (rightFrameContact + rightFrameMidStance + rightFramePropulsive), 4) * 100);
        float rightFrameMidStancePercent = Convert.ToSingle(Math.Round(rightFrameMidStance / (rightFrameContact + rightFrameMidStance + rightFramePropulsive), 4) * 100);
        float rightFramePropulsivePercent = Convert.ToSingle(Math.Round(rightFramePropulsive / (rightFrameContact + rightFrameMidStance + rightFramePropulsive), 4) * 100);
        Debug.Log("Contact : " + rightFrameContactPercent + "% MidStance : " + rightFrameMidStancePercent + "% Propulsive : " + rightFramePropulsivePercent + "%");

        rightValueArray = new float[3];
        rightValueArray[0] = rightFrameContactPercent;
        rightValueArray[1] = rightFrameMidStancePercent;
        rightValueArray[2] = rightFramePropulsivePercent;

        return rightMax;
    }
}