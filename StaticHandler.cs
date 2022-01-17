using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class StaticHandler : MonoBehaviour
{
    #region includedll


    //set sensivity
    [DllImport("Multi-TeeTester-API")]
    private static extern bool Open(byte dwDeviceIndex);

    //set sensivity
    [DllImport("Multi-TeeTester-API")]
    private static extern byte GetDevicesNum();
    //set sensivity
    [DllImport("Multi-TeeTester-API")]
    private static extern bool SetPowA(byte bPowA);

    [DllImport("Multi-TeeTester-API")]
    private static extern int CollectFrame(byte[] array);

    [DllImport("OpenCVDLL")]
    private static extern void interpolationTexture(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, double widthScale, double heightScale, int flag);

    [DllImport("OpenCVDLL")]
    private static extern void interpolationTextureGaussianBlur(ref Color32[] rawImage, ref Color32[] interpolationImage, ref Color32[] resultGrayScaleImage, int width, int height, double widthScale, double heightScale, int flag);
    [DllImport("OpenCVDLL")]
    private static extern void getCannyEdge(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, int scale);

    [DllImport("OpenCVDLL")]
    private static extern void addImage(ref Color32[] rawImage, ref Color32[] rawImage2, ref Color32[] returnImage, int width, int height, int scale);

    [DllImport("OpenCVDLL")]
    private static extern void addText(ref Color32[] rawImage, int width, int height, int scale, int leftCOPX, int leftCOPY, int rightCOPX, int rightCOPY, int leftPeakForceX, int leftPeakForceY, int rightPeakForceX, int rightPeakForceY, int COGX, int COGY, int noWeightCOGX, int noWeightCOGY);

    //Close Device 
    [DllImport("Multi-TeeTester-API")]
    private static extern void Close();

    #endregion

    #region public variables
    public static bool isStart = false;
    public static bool isWaiting = false;
    public float startTime;

    public Image imageTarget;
    public Image imageDetected;
    public Image imageInterpolation;
    public Image imageCanny;
    public Image imageResult;
    public Image imageResultGrayScale;
    public Texture2D Image;

    public static int WIDTH = 52;
    public static int HEIGHT = 44;
    public static int SCALE = 10;

    public Queue<int> leftPixelQueue;
    public Queue<int> rightPixelQueue;

    public int leftCOPX;
    public int leftCOPY;
    public int rightCOPX;
    public int rightCOPY;

    public int leftPeakForceX;
    public int leftPeakForceY;

    public int rightPeakForceX;
    public int rightPeakForceY;
    public int COGX;
    public int COGY;

    public int cogavg;
    public int cogpointX;
    public int cogpointY;
    public int noWeightleftcopX;
    public int noWeightleftcopY;
    public int noWeightrightcopX;
    public int noWeightrightcopY;


    public static int StaticframeCount = 0;
    public static int[,] StaticFrameRecordArray;
    public static int[] StaticFrameDetectedSum;

    public static int[] leftCOPXHistory;
    public static int[] leftCOPYHistory;
    public static int[] rightCOPXHistory;
    public static int[] rightCOPYHistory;
    public static int[] leftPeakForceXHistory;
    public static int[] leftPeakForceYHistory;
    public static int[] rightPeakForceXHistory;
    public static int[] rightPeakForceYHistory;
    public static int[] COGXHistory;
    public static int[] COGYHistory;

    public static int[] leftAreaCountHistory;
    public static int[] rightAreaCountHistory;
    public static float[] leftArchIndexHistory;
    public static float[] rightArchIndexHistory;
    public static float[] LeftPercentHistory;
    public static float[] RightPercentHistory;
    public static float[] TopPercentHistory;
    public static float[] BottomPercentHistory;
    public static int[] leftFootLengthHistory;
    public static int[] rightFootLengthHistory;

    public byte[] tmpImageBuffer;
    public bool isValueExist = false;

    public Text timeText;
    public Text currentName;
    public Text currentMeasureTime;
    public Text currentDate;

    public Slider currentBar;

    public Text leftPercentText;
    public Text rightPercentText;
    public Text topPercentText;
    public Text bottomPercentText;

    public GameObject leftPeak;
    public GameObject rightPeak;
    public GameObject topPeak;
    public GameObject bottomPeak;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        //Display.displays[0].Activate(1920, 1080, 60);
        //Display.displays[1].Activate(1920, 1080, 60);

        //getComponents

        leftPeak = GameObject.Find("leftPeak");
        rightPeak = GameObject.Find("rightPeak");
        topPeak = GameObject.Find("topPeak");
        bottomPeak = GameObject.Find("bottomPeak");
        timeText.text = "0 sec";

        leftPeak.SetActive(false);
        rightPeak.SetActive(false);
        topPeak.SetActive(false);
        bottomPeak.SetActive(false);

        currentBar.value = 0f;
        leftPixelQueue = new Queue<int>();
        rightPixelQueue = new Queue<int>();

        StaticFrameRecordArray = new int[StaticVaribleHandler.MaximumStaticFrameCount, WIDTH * HEIGHT];
        StaticFrameDetectedSum = new int[WIDTH * HEIGHT];
        leftCOPXHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        leftCOPYHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        rightCOPXHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        rightCOPYHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        leftPeakForceXHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        leftPeakForceYHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        rightPeakForceXHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        rightPeakForceYHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        COGXHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        COGYHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        leftAreaCountHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        rightAreaCountHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        leftFootLengthHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];
        rightFootLengthHistory = new int[StaticVaribleHandler.MaximumStaticFrameCount];

        leftArchIndexHistory = new float[StaticVaribleHandler.MaximumStaticFrameCount];
        rightArchIndexHistory = new float[StaticVaribleHandler.MaximumStaticFrameCount];
        LeftPercentHistory = new float[StaticVaribleHandler.MaximumStaticFrameCount];
        RightPercentHistory = new float[StaticVaribleHandler.MaximumStaticFrameCount];
        TopPercentHistory = new float[StaticVaribleHandler.MaximumStaticFrameCount];
        BottomPercentHistory = new float[StaticVaribleHandler.MaximumStaticFrameCount];

        StaticframeCount = 0;
        StaticFrameRecordArray.Initialize();
    }

    void Update()
    {
        if (StaticframeCount == 0)
        {
            startTime = Time.time;
        }
        byte[] rawDataBuffer = new byte[WIDTH * HEIGHT];
        SetPowA(80);
        CollectFrame(rawDataBuffer);    //SAVE BUFFER

        var leftAvg = new Tuple<double, double, double,int>(0,0, 0, 0);
        var rightAvg = new Tuple<double, double, double,int>(0,0, 0, 0);

        tmpImageBuffer = new byte[WIDTH * HEIGHT];
        isValueExist = false;
        int pixelCount = 0;

        cogavg = 0;
        cogpointX = 0;
        cogpointY = 0;
        noWeightleftcopX = 0;
        noWeightleftcopY = 0;
        noWeightrightcopX = 0;
        noWeightrightcopY = 0;

        //mapping
        for (int i = 0; i < rawDataBuffer.Length; i++)
        {
            int arraywidth = i / HEIGHT;
            int arrayheight = i % HEIGHT;

            int widthIndex = StaticVaribleHandler.widthMappingArray[arraywidth];
            int heightIndex = StaticVaribleHandler.heightMappingArray[arrayheight];

            tmpImageBuffer[(heightIndex * WIDTH) + widthIndex] = rawDataBuffer[i];

            if(tmpImageBuffer[(heightIndex * WIDTH) + widthIndex] != 0)
            {
                isValueExist = true;
                pixelCount++;
            }
        }

        var targetImage = imageTarget.sprite.texture.GetPixels32();
        var detectedImage = imageDetected.sprite.texture.GetPixels32();
        var interpolationImage = imageInterpolation.sprite.texture.GetPixels32();
        var cannyImage = imageCanny.sprite.texture.GetPixels32();
        var resultImage = imageResult.sprite.texture.GetPixels32();
        var resultGrayScaleImage = imageResultGrayScale.sprite.texture.GetPixels32();

        //about noise pixel so, over 30pixels
        if (isValueExist && StaticframeCount < StaticVaribleHandler.MaximumStaticFrameCount && pixelCount >= 30)
        {
            //initalize
            for (int i = 0; i < interpolationImage.Length; i++)
            {
                interpolationImage[i].r = 0;
                interpolationImage[i].g = 0;
                interpolationImage[i].b = 0;

                cannyImage[i].r = 0;
                cannyImage[i].g = 0;
                cannyImage[i].b = 0;

                resultGrayScaleImage[i].r = 255;
                resultGrayScaleImage[i].g = 255;
                resultGrayScaleImage[i].b = 255;
            }
            imageResultGrayScale.sprite.texture.SetPixels32(resultGrayScaleImage);
            imageResultGrayScale.sprite.texture.Apply();
            for (int i = 0; i < targetImage.Length; i++)
            {
                detectedImage[i].r = 255;
                detectedImage[i].g = 255;
                detectedImage[i].b = 255;

                targetImage[i].r = 255;
                targetImage[i].g = 255;
                targetImage[i].b = 255;

                int tmp = (255 - tmpImageBuffer[i]);
                targetImage[i].r = (byte)tmp;
                targetImage[i].g = (byte)tmp;
                targetImage[i].b = (byte)tmp;

                detectedImage[i].r = targetImage[i].r;
                detectedImage[i].g = targetImage[i].g;
                detectedImage[i].b = targetImage[i].b;

                if(isStart)
                {
                    StaticFrameDetectedSum[i] += tmpImageBuffer[i];
                }
            }
            //remove noise
            for (int i = 0; i < targetImage.Length; i++)
            {
                int topleft = (i - StaticHandler.WIDTH - 1) > 0 ? (i - StaticHandler.WIDTH - 1) : 0;
                int topright = (i - StaticHandler.WIDTH + 1) > 0 ? (i - StaticHandler.WIDTH + 1) : 0;
                int bottomleft = (i + StaticHandler.WIDTH - 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH - 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;
                int bottomright = (i + StaticHandler.WIDTH + 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH + 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;

                int left = (i - 1) > 0 ? (i - 1) : 0;
                int right = (i + 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;
                int top = (i - StaticHandler.WIDTH) > 0 ? (i - StaticHandler.WIDTH) : 0;
                int bottom = (i + StaticHandler.WIDTH) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;

                if (targetImage[topleft].r == 255 && targetImage[topright].r == 255 && targetImage[bottomleft].r == 255 && targetImage[bottomright].r == 255 &&
                    targetImage[left].r == 255 && targetImage[right].r == 255 && targetImage[top].r == 255 && targetImage[bottom].r == 255)
                {
                    detectedImage[i].r = 255;
                    detectedImage[i].g = 255;
                    detectedImage[i].b = 255;

                    targetImage[i].r = 255;
                    targetImage[i].g = 255;
                    targetImage[i].b = 255;
                }
            }

            imageDetected.sprite.texture.SetPixels32(detectedImage);
            imageDetected.sprite.texture.Apply();

            imageTarget.sprite.texture.SetPixels32(targetImage);
            imageTarget.sprite.texture.Apply();

            bool left_flag = true;
            bool right_flag = true;

            for (int i = 0; i < detectedImage.Length; i++)
            {
                if (detectedImage[i].r == 255 && detectedImage[i].g == 255 && detectedImage[i].b == 255)
                {
                    //default pixel 
                }
                else if (i % WIDTH < WIDTH / 2 && left_flag)
                {
                    //LEFT FOOT
                    left_flag = false;
                    leftAvg = getFootData("LEFT", i);   //left avg, left_top avg, left_bottom avg, foot length

                }
                else if (i % WIDTH > WIDTH / 2 && right_flag)
                {
                    //RIGHT FOOT
                    right_flag = false;
                    rightAvg = getFootData("RIGHT", i);

                }
                if (!left_flag && !right_flag)
                {
                    //DETECTED ALL FOOT
                    double leftpercent = Math.Round(leftAvg.Item1 / (leftAvg.Item1 + rightAvg.Item1), 3) * 100;

                    double rightpercent = Math.Round(rightAvg.Item1 / (leftAvg.Item1 + rightAvg.Item1), 3) * 100;

                    double temp1 = Math.Round((leftAvg.Item2 / (leftAvg.Item2 + leftAvg.Item3 + rightAvg.Item2 + rightAvg.Item3)), 3) * 100;
                    double temp2 = Math.Round((leftAvg.Item3 / (leftAvg.Item2 + leftAvg.Item3 + rightAvg.Item2 + rightAvg.Item3)), 3) * 100;
                    double temp3 = Math.Round((rightAvg.Item2 / (leftAvg.Item2 + leftAvg.Item3 + rightAvg.Item2 + rightAvg.Item3)), 3) * 100;
                    double temp4 = Math.Round((rightAvg.Item3 / (leftAvg.Item2 + leftAvg.Item3 + rightAvg.Item2 + rightAvg.Item3)), 3) * 100;

                    string result1 = leftpercent + "%";
                    string result2 = rightpercent + "%";
                    string result3 = Convert.ToString(temp1 + temp3) + "%";
                    string result4 = Convert.ToString(temp2 + temp4) + "%";

                    if(isStart)
                    {
                        leftPercentText.text = result1;
                        rightPercentText.text = result2;
                        topPercentText.text = result3;
                        bottomPercentText.text = result4;

                        LeftPercentHistory[StaticframeCount] = (float)Math.Round(Convert.ToSingle(leftpercent),2);
                        RightPercentHistory[StaticframeCount] = (float)Math.Round(Convert.ToSingle(rightpercent),2);
                        BottomPercentHistory[StaticframeCount] = (float)Math.Round(Convert.ToSingle(temp1 + temp3),2);
                        TopPercentHistory[StaticframeCount] = (float)Math.Round(Convert.ToSingle(temp2 + temp4),2);

                        leftFootLengthHistory[StaticframeCount] = leftAvg.Item4;
                        rightFootLengthHistory[StaticframeCount] = rightAvg.Item4;

                        if (leftpercent > rightpercent)
                        {
                            leftPeak.SetActive(true);
                            rightPeak.SetActive(false);
                        }
                        else if (leftpercent < rightpercent)
                        {
                            leftPeak.SetActive(false);
                            rightPeak.SetActive(true);
                        }
                        else
                        {
                            leftPeak.SetActive(false);
                            rightPeak.SetActive(false);
                        }

                        if (temp1 + temp3 > temp2 + temp4)
                        {
                            topPeak.SetActive(true);
                            bottomPeak.SetActive(false);
                        }
                        else if (temp1 + temp3 < temp2 + temp4)
                        {
                            topPeak.SetActive(false);
                            bottomPeak.SetActive(true);
                        }
                        else
                        {
                            topPeak.SetActive(false);
                            bottomPeak.SetActive(false);
                        }
                    }
                    break;
                }
            }
            //interpolationTextureGaussianBlur(ref targetImage, ref resultGrayScaleImage, WIDTH, HEIGHT, SCALE, SCALE, 0);
            interpolationTextureGaussianBlur(ref targetImage, ref interpolationImage, ref resultGrayScaleImage, WIDTH, HEIGHT, SCALE, SCALE, 1);

            imageResultGrayScale.sprite.texture.SetPixels32(resultGrayScaleImage);
            imageResultGrayScale.sprite.texture.Apply();

            if (StaticframeCount < StaticVaribleHandler.MaximumStaticFrameCount)
            {
                for (int i = 0; i < WIDTH * HEIGHT; i++)
                {
                    StaticFrameRecordArray[StaticframeCount, i] = targetImage[i].r;
                }
            }

            imageInterpolation.sprite.texture.SetPixels32(interpolationImage);
            imageInterpolation.sprite.texture.Apply();

            makeWeightImage();
        }
        else
        {
            //Set default pixel
            for (int i = 0; i < interpolationImage.Length; i++)
            {
                interpolationImage[i].r = 128;
                interpolationImage[i].g = 128;
                interpolationImage[i].b = 128;

                cannyImage[i].r = 0;
                cannyImage[i].g = 0;
                cannyImage[i].b = 0;

                resultGrayScaleImage[i].r = 255;
                resultGrayScaleImage[i].g = 255;
                resultGrayScaleImage[i].b = 255;

            }
            for (int i = 0; i < targetImage.Length; i++)
            {
                detectedImage[i].r = 255;
                detectedImage[i].g = 255;
                detectedImage[i].b = 255;

                targetImage[i].r = 255;
                targetImage[i].g = 255;
                targetImage[i].b = 255;
            }

            imageResultGrayScale.sprite.texture.SetPixels32(resultGrayScaleImage);
            imageResultGrayScale.sprite.texture.Apply();

            imageInterpolation.sprite.texture.SetPixels32(interpolationImage);
            imageInterpolation.sprite.texture.Apply();

            imageCanny.sprite.texture.SetPixels32(cannyImage);
            imageCanny.sprite.texture.Apply();

            imageTarget.sprite.texture.SetPixels32(targetImage);
            imageTarget.sprite.texture.Apply();

            imageDetected.sprite.texture.SetPixels32(detectedImage);
            imageDetected.sprite.texture.Apply();

            interpolationTexture(ref targetImage, ref interpolationImage, WIDTH, HEIGHT, SCALE, SCALE, 1);

            var resultImage2 = imageResult.sprite.texture.GetPixels();
            for (int i = 0; i < resultImage.Length; i++)
            {
                resultImage[i].r = 34;
                resultImage[i].g = 46;
                resultImage[i].b = 56;
                resultImage[i].a = 255;

                resultImage2[i].r = (float)34 / 255;
                resultImage2[i].g = (float)46 / 255;
                resultImage2[i].b = (float)56 / 255;
                resultImage2[i].a = (float)255 / 255;
            }   
            imageResult.sprite.texture.SetPixels(resultImage2);
            imageResult.sprite.texture.Apply();
        }
    }

    Tuple<double, double, double, int> getFootData(string FOOT_TYPE, int START_IDX)
    {
        var originalImage = imageTarget.sprite.texture.GetPixels32();
        var detectedImage = imageDetected.sprite.texture.GetPixels32();
        var cannyImage = imageCanny.sprite.texture.GetPixels32();

        Queue<int> bfsQueue = new Queue<int>();
        Queue<int> visitQueue = new Queue<int>();
        Queue<int> centerWidthQueue = new Queue<int>();
        Queue<int> centerHeightQueue = new Queue<int>();

        int height = WIDTH * SCALE;
        double avg = 0;
        double copavg = 0;

        double topAvg = 0;
        int topCount = 0;
        double bottomAvg = 0;
        int bottomCount = 0;
        var maxHeight = new Tuple<int, int, int, int>(0, 0, 0, 0);

        bfsQueue.Enqueue(START_IDX);


        visitQueue = StaticVaribleHandler.SearchPixel(START_IDX, detectedImage);
        maxHeight = StaticVaribleHandler.getLongestPixel(visitQueue, detectedImage);    //최고길이, 최고길이 세로idx, 가로idx 시작, 가로idx끝 

        //maxHeight = getTopBottomCenterPoint(visitQueue);    //최고길이, 최고길이 세로idx, 가로idx 시작, 가로idx끝 

        int leftcopX = 0;
        int leftcopY = 0;
        int rightcopX = 0;
        int rightcopY = 0;


        int footLength = maxHeight.Item1;
        int footTop = maxHeight.Item3;
        int footBottom = maxHeight.Item4;
        //("FOOT_TYPE : " + FOOT_TYPE + " TOP : " + footTop + " BOTTOM : " + footBottom);
        int minIdx = 9999;
        int maxIdx = -1;

        int leftCount = 1;
        int rightCount = 1;
        foreach (int idx in visitQueue)
        {
            avg += (255- originalImage[idx].r);

            if (idx < minIdx)
            {
                minIdx = idx;
            }
            else if(idx > maxIdx)
            {
                maxIdx = idx;
            }

            if (FOOT_TYPE == "LEFT")
            {
                leftcopX = leftcopX + (idx % WIDTH) * (255-originalImage[idx].r); //좌표 * 값으로 가중치
                leftcopY = leftcopY + (idx / WIDTH) * (255-originalImage[idx].r);
                cogpointX = cogpointX + (idx % WIDTH) * (255 - originalImage[idx].r); 
                cogpointY = cogpointY + (idx / WIDTH) * (255 - originalImage[idx].r);

                noWeightleftcopX = noWeightleftcopX + (idx % WIDTH * SCALE);
                noWeightleftcopY = noWeightleftcopY + (idx / WIDTH * SCALE);
                leftCount++;
            }
            else if (FOOT_TYPE == "RIGHT")
            {
                rightcopX = rightcopX + (idx % WIDTH) * (255 - originalImage[idx].r);
                rightcopY = rightcopY + (idx / WIDTH) * (255 - originalImage[idx].r);
                cogpointX = cogpointX + (idx % WIDTH) * (255 - originalImage[idx].r);
                cogpointY = cogpointY + (idx / WIDTH) * (255 - originalImage[idx].r);

                noWeightrightcopX = noWeightrightcopX + (idx % WIDTH * SCALE);
                noWeightrightcopY = noWeightrightcopY + (idx / WIDTH * SCALE);
                rightCount++;
            }

            if (FOOT_TYPE == "LEFT")
            {
                detectedImage[idx].r = 0;
                detectedImage[idx].g = 0;
                detectedImage[idx].b = 0;
            }
            else
            {
                detectedImage[idx].r = 128;
                detectedImage[idx].g = 128;
                detectedImage[idx].b = 128;
            }

            detectedImage[idx].g = 0;
            detectedImage[idx].b = 0;


            //GET TOP BOTTOM
            if (idx / WIDTH <= (maxHeight.Item3 + maxHeight.Item4) / 2)
            {
                topAvg += (255-originalImage[idx].r);
                topCount++;
                detectedImage[idx].r = 0;
                
                
                if(FOOT_TYPE == "LEFT")
                {
                    detectedImage[idx].r = 128;
                }
                
            }
            else
            {
                bottomAvg += (255-originalImage[idx].r);
                bottomCount++;
                
                detectedImage[idx].r = 255;

                if(FOOT_TYPE == "LEFT")
                {
                    detectedImage[idx].r = 64;
                }
                
            }
            copavg += (255-originalImage[idx].r);
            cogavg += (255-originalImage[idx].r);
        }
        minIdx /= WIDTH;
        maxIdx /= WIDTH;
        footLength = maxIdx - minIdx;

        //중심점 좌표 시작

        //get input count
        if (FOOT_TYPE == "LEFT")
        {
            leftCOPX = (int)(leftcopX / copavg) * SCALE;
            leftCOPY = (int)(leftcopY / copavg) * SCALE;

            noWeightleftcopX /= leftCount;
            noWeightleftcopY /= leftCount;

            leftCOPXHistory[StaticframeCount] = leftCOPX;
            leftCOPYHistory[StaticframeCount] = leftCOPY;

            leftAreaCountHistory[StaticframeCount] = visitQueue.Count();
        }
        else if(FOOT_TYPE == "RIGHT")
        {
            rightCOPX = (int)(rightcopX / copavg) * SCALE;
            rightCOPY = (int)(rightcopY / copavg) * SCALE;

            noWeightrightcopX /= rightCount;
            noWeightrightcopY /= rightCount;

            rightCOPXHistory[StaticframeCount] = rightCOPX;
            rightCOPYHistory[StaticframeCount] = rightCOPY;

            rightAreaCountHistory[StaticframeCount] = visitQueue.Count();
        }
        imageDetected.sprite.texture.SetPixels32(detectedImage);
        imageDetected.sprite.texture.Apply();

        visitQueue.Clear();

        return new Tuple<double, double, double, int>(avg, topAvg, bottomAvg, footLength);
    }

    void makeWeightImage()
    {
        var resultImage = imageResult.sprite.texture.GetPixels32();
        var interpolationImage = imageInterpolation.sprite.texture.GetPixels32();
        var cannyImage = imageCanny.sprite.texture.GetPixels32();
        
        COGX = (int)(cogpointX / cogavg) * SCALE;
        COGY = (int)(cogpointY / cogavg) * SCALE;
        int noWeightCOGX;
        int noWeightCOGY;

        noWeightCOGX = (noWeightleftcopX + noWeightrightcopX) / 2;
        noWeightCOGY = (noWeightleftcopY + noWeightrightcopY) / 2;

        COGXHistory[StaticframeCount] = COGX;
        COGYHistory[StaticframeCount] = COGY;

        var resultImage2 = imageResult.sprite.texture.GetPixels();
        var imageGrayScale = imageResultGrayScale.sprite.texture.GetPixels32();

        //filtering 이슈 때문에 visualition하면서 peakforce의 좌표를 구한다.
        int leftPeakForceVal = 999;
        int leftPeakForceIdxMax = 0;
        int leftPeakForceIdxMin = 0;
        int rightPeakForceVal = 999;
        int rightPeakForceIdxMax = 0;
        int rightPeakForceIdxMin = 0;

        for (int i = 0; i < resultImage.Length; i++)
        {
            resultImage[i].r = interpolationImage[i].r;
            resultImage[i].g = interpolationImage[i].g;
            resultImage[i].b = interpolationImage[i].b;

            resultImage2[i].r = resultImage[i].r / 255f;
            resultImage2[i].g = resultImage[i].g / 255f;
            resultImage2[i].b = resultImage[i].b / 255f;
            resultImage2[i].a = 1f;

            //PeakForce좌표 관련해서 같은 색상의 픽셀의 최소,최대값을 구해 평균내는식으로 변경
            int value = imageGrayScale[i].r;
            if (value < leftPeakForceVal && i % (WIDTH * SCALE) < (WIDTH * SCALE / 2))
            {
                leftPeakForceVal = value;
                leftPeakForceIdxMin = i;
                leftPeakForceIdxMax = i;
            }
            else if (value == leftPeakForceVal && i % (WIDTH * SCALE) < (WIDTH * SCALE / 2)){
                leftPeakForceIdxMax = i;
            }
            else if (value < rightPeakForceVal && i % (WIDTH * SCALE) > (WIDTH * SCALE / 2))
            {
                rightPeakForceVal = value;
                rightPeakForceIdxMin = i;
                rightPeakForceIdxMax = i;
            }
            else if (value == rightPeakForceVal && i % (WIDTH * SCALE) > (WIDTH * SCALE / 2))
            {
                rightPeakForceIdxMax = i;
            }
            resultImage[i].a = 255;
            if (resultImage[i].r == 0 && resultImage[i].g == 0 && resultImage[i].b == 128)
            {
                //blank
                resultImage[i].r = 34;
                resultImage[i].g = 46;
                resultImage[i].b = 56;

                resultImage2[i].r = 34 / 255f;
                resultImage2[i].g = 46 / 255f;
                resultImage2[i].b = 56 / 255f;
            }
            else if(!isStart)
            {
                //show grayscale
                float tmpr = resultImage2[i].r;
                float tmpb = resultImage2[i].b;
                float tmpg = resultImage2[i].g;
                float r = 0.299f;
                float g = 0.587f;
                float b = 0.114f;
                float color = (r * tmpr + g * tmpg + b * tmpb);
                resultImage2[i].r = color;
                resultImage2[i].g = color;
                resultImage2[i].b = color;

                if (!isStart && isWaiting)
                {
                    resultImage2[i].a = 0.7f;
                }
                else
                {
                    resultImage2[i].a = 0.5f;
                }
            }
        }

        //특징점 페인팅 opencv으로 대체
        if (isStart)
        {
            //PeakForce좌표 관련해서 같은 색상의 픽셀의 최소,최대값을 구해 평균내는식으로 변경
            int leftPeakForceMaxX = leftPeakForceIdxMax % (WIDTH * SCALE);
            int leatPeakForceMaxY = leftPeakForceIdxMax / (WIDTH * SCALE);
            int leftPeakForceMinX = leftPeakForceIdxMin % (WIDTH * SCALE);
            int leftPeakForceMinY = leftPeakForceIdxMin / (WIDTH * SCALE);

            int rightPeakForceMaxX = rightPeakForceIdxMax % (WIDTH * SCALE);
            int rightPeakForceMaxY = rightPeakForceIdxMax / (WIDTH * SCALE);
            int rightPeakForceMinX = rightPeakForceIdxMin % (WIDTH * SCALE);
            int rightPeakForceMinY = rightPeakForceIdxMin / (WIDTH * SCALE);

            leftPeakForceX = (leftPeakForceMaxX + leftPeakForceMinX) / 2;
            leftPeakForceY = (leatPeakForceMaxY + leftPeakForceMinY) / 2;
            rightPeakForceX = (rightPeakForceMaxX + rightPeakForceMinX) / 2;
            rightPeakForceY = (rightPeakForceMaxY + rightPeakForceMinY) / 2;

            leftPeakForceXHistory[StaticframeCount] = leftPeakForceX;
            leftPeakForceYHistory[StaticframeCount] = leftPeakForceY;
            rightPeakForceXHistory[StaticframeCount] = rightPeakForceX;
            rightPeakForceYHistory[StaticframeCount] = rightPeakForceY;

            addText(ref resultImage, WIDTH, HEIGHT, SCALE, leftCOPX, leftCOPY, rightCOPX, rightCOPY, leftPeakForceX, leftPeakForceY, rightPeakForceX, rightPeakForceY, COGX, COGY, noWeightCOGX, noWeightCOGY);
            StaticframeCount++;
            currentBar.value = (float)StaticframeCount / StaticVaribleHandler.MaximumStaticFrameCount;
            timeText.text = Convert.ToInt32(Time.time - startTime) + " sec".ToString();

            imageResult.sprite.texture.SetPixels32(resultImage);
            imageResult.sprite.texture.Apply();
        }
        else
        {
            imageResult.sprite.texture.SetPixels(resultImage2);
            imageResult.sprite.texture.Apply();
        }
    }
}

