using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class DynamicHandler : MonoBehaviour
{
    #region includedll
    //get Device idx
    [DllImport("Multi-TeeTester-API")]
    private static extern uint GetDevicesNum();
    //Open Device
    [DllImport("Multi-TeeTester-API")]
    private static extern bool Open(uint deDeviceIndex);

    //Close Device 
    [DllImport("Multi-TeeTester-API")]
    private static extern void Close();
    //set sensivity
    [DllImport("Multi-TeeTester-API")]
    private static extern bool SetPowA(byte bPowA);

    [DllImport("Multi-TeeTester-API")]
    private static extern int CollectFrame(byte[] array);
    [DllImport("OpenCVDLL")]
    private static extern void interpolationTexture(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, double widthScale, double heightScale, int flag);
    [DllImport("OpenCVDLL")]
    private static extern void interpolationTextureGaussianBlur(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, double widthScale, double heightScale, int flag);
    [DllImport("OpenCVDLL")]
    private static extern void getCannyEdge(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, int scale);
    [DllImport("OpenCVDLL")]
    private static extern void addImage(ref Color32[] rawImage, ref Color32[] rawImage2, ref Color32[] returnImage, int width, int height, int scale);
    [DllImport("OpenCVDLL")]
    private static extern void addText(ref Color32[] rawImage, int width, int height, int scale, int leftCOPX, int leftCOPY, int rightCOPX, int rightCOPY, int leftPeakForceX, int leftPeakForceY, int rightPeakForceX, int rightPeakForceY, int COGX, int COGY);
    [DllImport("OpenCVDLL")]
    private static extern void addDynamicText(ref Color32[] rawImage, int width, int height, int scale, int COPX, int COPY, int PeakForceX, int PeakForceY, int[] COPXHistory, int[] COPYHistory, int[] PeakForceXHistory, int[] PeakForceYHistory, int frameStart, int frameEnd);
    #endregion

    #region public variables
    public Button staticButton;
    public Button showResultButton;
    public Button showStaticButton;
    public Button startButton;
    public Button mainButton;

    public Image dynamicHalfLine;

    public Image imageTarget;
    public Image imageDetected;
    public Image imageInterpolation;
    public Image imageCanny;
    public Image imageResult;


    public Text waitingText;
    public Text waitingText2;
    public Text countdownText;
    public Text countdownText2;
    public Text endText;

    public Text currentDate;
    public Text currentMeasureTime;
    public Text currentName;

    public Text currentDate2;
    public Text currentMeasureTime2;
    public Text currentName2;
    public Text patientStatusText;

    public GameObject programLogo;
    public VideoPlayer video;
    public RawImage screen;

    public VideoPlayer video2;
    public RawImage screen2;

    public GameObject display2Explain;
    public GameObject loadingShape;
    public GameObject checkShape;
    public Button closeButton;
    public int WIDTH = 52;
    public int HEIGHT = 44;
    public int SCALE = 10;
    public byte[] tmpImageBuffer;
    public bool isValueExist = false;

    public bool leftFlag = false;
    public bool rightFlag = false;

    public int COPX;
    public int COPY;

    public int PeakForceX;
    public int PeakForceY;

    public float currentTime = 0f;
    public float secondTestTime = 0f;
    public float startMeasureTime = 0f;
    public enum MODE {WAITING_START, WAITING_LEFT, START_LEFT, WAITING_RIGHT, START_RIGHT, END};
    public static int STATUS;
    #endregion

    #region static variables
    //frame Count
    public static int leftIdx = 0;
    public static int rightIdx = 0;

    public static int leftMaxPixelIdx = 0;
    public static int leftMaxPixelValue = 9999;
    public static int rightMaxPixelIdx = 0;
    public static int rightMaxPixelValue = 9999;

    public static int[] leftImageDetectedSum;
    public static int[] rightImageDetectedSum;

    public static int[] COPXHistory;
    public static int[] COPYHistory;

    public static int[] PeakForceXHistory;
    public static int[] PeakForceYHistory;

    public static int[] topYHistory;
    public static int[] bottomYHistory;

    public static int DynamicframeCount = 0;

    public static byte[,] DynamicFrameRecordArray;
    public bool isTimeover = false;
    #endregion

    private void Awake()
    {
        startButton.onClick.AddListener(startMeasuring);
        showStaticButton.onClick.AddListener(showStatic);
        //mainButton.onClick.AddListener(showStatic);
        closeButton.onClick.AddListener(closeDevice);
        if (screen != null & video != null)
        {
            StartCoroutine(prepareVideo());
        }
        if (video != null && video.isPrepared)
        {
            video.Play();
        }
        GameObject.Find("currentName").GetComponent<Text>().text = StaticVaribleHandler.currentPatient;
        GameObject.Find("currentDate").GetComponent<Text>().text = DateTime.Now.ToString("yyyy-MM-dd");
        GameObject.Find("currentName2").GetComponent<Text>().text = StaticVaribleHandler.currentPatient;
        GameObject.Find("currentDate2").GetComponent<Text>().text = DateTime.Now.ToString("yyyy-MM-dd");
    }

    protected IEnumerator prepareVideo()
    {
        video.Prepare();
        while (!video.isPrepared)
        {
            yield return new WaitForSeconds(0.5f);
        }
        screen.texture = video.texture;
    }

    protected IEnumerator prepareVideo2()
    {
        video2.Prepare(); 

        while (!video2.isPrepared)
        {
            yield return new WaitForSeconds(0.5f);
        }
        screen2.texture = video2.texture;

        video2.Play();
    }

    void startMeasuring()
    {
        startMeasureTime = Time.time;
        StartCoroutine("startFrame");
    }
    void closeDevice()
    {
        Application.Quit();
    }
    void showStatic()
    {
        SceneManager.LoadScene("StaticScene");
    }
    IEnumerator startFrame()
    {
        programLogo.SetActive(false);
        patientStatusText.gameObject.SetActive(false);
        display2Explain.gameObject.SetActive(true);
        StartCoroutine(prepareVideo2());
        waitingText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false); 
        dynamicHalfLine.gameObject.SetActive(false);
        yield return new WaitForSeconds(5);
        waitingText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        countdownText2.gameObject.SetActive(true);

        display2Explain.gameObject.SetActive(false);


        uint ConnectIdx = 0;
        ConnectIdx = GetDevicesNum();
        Open(ConnectIdx);


        SetPowA(40);    //set sensivity
        dynamicHalfLine.gameObject.SetActive(true);

        float currentTime = 0f;
        while (true)
        {
            if (currentTime >= 5f)
            {
                isTimeover = true;
                countdownText.gameObject.SetActive(false);
                countdownText2.gameObject.SetActive(false);

                STATUS = (int)MODE.WAITING_LEFT;
                loadingShape.gameObject.SetActive(true);
                waitingText2.gameObject.SetActive(true);

                yield break;
            }
            else
            {
                currentTime += Time.deltaTime;
                float t = 6 - currentTime;
                if (t > 1f)
                {
                    countdownText.text = (6 - currentTime).ToString("N1");
                    countdownText2.text = (6 - currentTime).ToString("N1");
                }
            }
            yield return null;
        }
        startButton.gameObject.SetActive(false);   
    }

    void resetFrame()
    {
        DynamicframeCount = 0;
        DynamicFrameRecordArray.Initialize();
        STATUS = (int)MODE.WAITING_START;
        leftImageDetectedSum = Enumerable.Repeat(0, leftImageDetectedSum.Length).ToArray();
        rightImageDetectedSum = Enumerable.Repeat(0, rightImageDetectedSum.Length).ToArray();
        waitingText.gameObject.SetActive(true);
        Close();

        startFrame();
    }
    // Start is called before the first frame update
    void Start()
    {
        leftIdx = 0;
        rightIdx = 0;
        leftMaxPixelIdx = 0;
        leftMaxPixelValue = 9999;
        rightMaxPixelIdx = 0;
        DynamicframeCount = 0;
        isTimeover = false;
        rightMaxPixelValue = 9999;
        StaticVaribleHandler.isEndDynamic = false;
        //init dll
        COPXHistory = new int[100];
        COPYHistory = new int[100];
        PeakForceXHistory = new int[100];
        PeakForceYHistory = new int[100];
        topYHistory = new int[100];
        bottomYHistory = new int[100];
        leftImageDetectedSum = new int[StaticHandler.HEIGHT * StaticHandler.WIDTH];
        leftImageDetectedSum = Enumerable.Repeat(0, leftImageDetectedSum.Length).ToArray();
        rightImageDetectedSum = new int[StaticHandler.HEIGHT * StaticHandler.WIDTH];
        rightImageDetectedSum = Enumerable.Repeat(0, rightImageDetectedSum.Length).ToArray();

        STATUS = (int)MODE.WAITING_START;
        DynamicFrameRecordArray = new byte[100, 2288];
        SetPowA(40);    //set sensivity

        //Display.displays[0].Activate(1920, 1080, 60);
        //Display.displays[1].Activate(1920, 1080, 60);

        Close();
    }

    // Update is called once per frame
    void Update()
    {
        if (loadingShape.activeInHierarchy) //Loading shape
        {
            float z = 0;
            z += Time.time * 400f;
            var rot = loadingShape.transform.rotation;
            rot.eulerAngles = new Vector3(0, 0, z);
            loadingShape.transform.rotation = rot;
        }

        byte[] rawDataBuffer = new byte[WIDTH * HEIGHT];
        SetPowA(40);    //set sensivity
        CollectFrame(rawDataBuffer);    //SAVE BUFFER
        
        tmpImageBuffer = new byte[WIDTH * HEIGHT];
        var leftAvg = new Tuple<double, double, double, double>(0, 0, 0, 0);
        var rightAvg = new Tuple<double, double, double, double>(0, 0, 0, 0);

        currentTime += Time.deltaTime;

        //mapping
        for (int i = 0; i < rawDataBuffer.Length; i++)
        {
            int arraywidth = i / HEIGHT;
            int arrayheight = i % HEIGHT;

            int widthIndex = StaticVaribleHandler.widthMappingArray[arraywidth];
            int heightIndex = StaticVaribleHandler.heightMappingArray[arrayheight];

            tmpImageBuffer[(heightIndex * WIDTH) + widthIndex] = rawDataBuffer[i];
        }

        isValueExist = false;
        int pixelCount = 0;
        //check noise
        for (int i = 0; i < tmpImageBuffer.Length; i++)
        {
            if (tmpImageBuffer[i] != 0)
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

        //is sensor active
        if (isTimeover && isValueExist && pixelCount > 30)
        {
            if(STATUS == (int)MODE.WAITING_LEFT)
            {
                STATUS = (int)MODE.START_LEFT;
            }
            else if(STATUS == (int)MODE.WAITING_RIGHT && currentTime > secondTestTime + 2f)
            {
                STATUS = (int)MODE.START_RIGHT;
            }
            //initalize
            for (int i = 0; i < interpolationImage.Length; i++)
            {
                interpolationImage[i].r = 0;
                interpolationImage[i].g = 0;
                interpolationImage[i].b = 0;

                cannyImage[i].r = 0;
                cannyImage[i].g = 0;
                cannyImage[i].b = 0;
            }
            //initalize
            for (int i = 0; i < targetImage.Length; i++)
            {
                detectedImage[i].r = 255;
                detectedImage[i].g = 255;
                detectedImage[i].b = 255;

                targetImage[i].r = 255;
                targetImage[i].g = 255;
                targetImage[i].b = 255;
            }

            //get Frame Data
            int frame_temp = 0;
            for (int i = 0; i < targetImage.Length; i++)
            {
                
                if(tmpImageBuffer[i] < 30)
                {
                    tmpImageBuffer[i] = 0;
                }
                targetImage[i].r = (byte)(255 - tmpImageBuffer[i]);
                targetImage[i].g = (byte)(255 - tmpImageBuffer[i]);
                targetImage[i].b = (byte)(255 - tmpImageBuffer[i]);

                if (targetImage[i].r != 255 || targetImage[i].g != 255 || targetImage[i].b != 255)
                {
                    detectedImage[i].r = 0;
                    detectedImage[i].g = 0;
                    detectedImage[i].b = 0;

                    if (STATUS == (int)MODE.START_LEFT)
                    {
                        leftImageDetectedSum[i] += tmpImageBuffer[i];
                    }
                    else if (STATUS == (int)MODE.START_RIGHT)
                    {
                        rightImageDetectedSum[i] += tmpImageBuffer[i];

                    }
                }
                else
                {
                    frame_temp++;
                }
            }
            //remove noise
            for (int i = 0; i<targetImage.Length;i++)
            {
                /*
                int noiseCount = 0;
                int[] pixelDirction = new int[8];
                pixelDirction[0] = (i - StaticHandler.WIDTH - 1) > 0 ? (i - StaticHandler.WIDTH - 1) : 0;
                pixelDirction[1] = (i - StaticHandler.WIDTH + 1) > 0 ? (i - StaticHandler.WIDTH + 1) : 0;
                pixelDirction[2] = (i + StaticHandler.WIDTH - 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH - 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;
                pixelDirction[3] = (i + StaticHandler.WIDTH + 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH + 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;
                pixelDirction[4] = (i - 1) > 0 ? (i - 1) : 0;
                pixelDirction[5] = (i + 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;
                pixelDirction[6] = (i - StaticHandler.WIDTH) > 0 ? (i - StaticHandler.WIDTH) : 0;
                pixelDirction[7] = (i + StaticHandler.WIDTH) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;

                for(int j = 0; j < 8; j++)
                {
                    if(pixelDirction[j] == 255)
                    {
                        noiseCount++;
                    }
                }

                if(noiseCount >= 6)
                {
                    detectedImage[i].r = 255;
                    detectedImage[i].g = 255;
                    detectedImage[i].b = 255;

                    targetImage[i].r = 255;
                    targetImage[i].g = 255;
                    targetImage[i].b = 255;
                }
                
                
                int topleft = (i - StaticHandler.WIDTH - 1) > 0 ? (i - StaticHandler.WIDTH - 1) : 0;
                int topright = (i - StaticHandler.WIDTH + 1) > 0 ? (i - StaticHandler.WIDTH + 1) : 0;
                int bottomleft = (i + StaticHandler.WIDTH - 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH - 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT-1;
                int bottomright = (i + StaticHandler.WIDTH + 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH + 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT-1;

                int left = (i - 1) > 0 ? (i - 1) : 0;
                int right = (i + 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT-1;
                int top = (i - StaticHandler.WIDTH) > 0 ? (i - StaticHandler.WIDTH) : 0;
                int bottom = (i + StaticHandler.WIDTH) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH) : StaticHandler.WIDTH * StaticHandler.HEIGHT-1;

                if(targetImage[topleft].r == 255 && targetImage[topright].r == 255 && targetImage[bottomleft].r == 255 && targetImage[bottomright].r == 255 &&
                    targetImage[left].r == 255 && targetImage[right].r == 255 && targetImage[top].r == 255 && targetImage[bottom].r == 255)
                {
                    detectedImage[i].r = 255;
                    detectedImage[i].g = 255;
                    detectedImage[i].b = 255;

                    targetImage[i].r = 255;
                    targetImage[i].g = 255;
                    targetImage[i].b = 255;
                }
                */
            }

            if (STATUS == (int)MODE.START_LEFT)
            {
                if (leftMaxPixelValue > frame_temp)
                {
                    leftMaxPixelValue = frame_temp;
                    leftMaxPixelIdx = DynamicframeCount;
                }
            }
            else if (STATUS == (int)MODE.START_RIGHT)
            {
                if (rightMaxPixelValue > frame_temp)
                {
                    rightMaxPixelValue = frame_temp;
                    rightMaxPixelIdx = DynamicframeCount;
                }
            }

            if (DynamicframeCount < 1000)
            {
                for (int i = 0; i < WIDTH * HEIGHT; i++)
                {
                    DynamicFrameRecordArray[DynamicframeCount, i] = targetImage[i].r;
                }
            }

            imageDetected.sprite.texture.SetPixels32(detectedImage);
            imageDetected.sprite.texture.Apply();

            imageTarget.sprite.texture.SetPixels32(targetImage);
            imageTarget.sprite.texture.Apply();

            for (int i = 0; i < detectedImage.Length; i++)
            {
                if (detectedImage[i].r == 255 && detectedImage[i].g == 255 && detectedImage[i].b == 255)
                {
                    //default pixel 
                }
                //Only 1 foot detected.
                else
                {
                    getDynamicPoint(i);
                    break;
                }
            }

            interpolationTextureGaussianBlur(ref targetImage, ref interpolationImage, WIDTH, HEIGHT, SCALE, SCALE, 1);
            imageInterpolation.sprite.texture.SetPixels32(interpolationImage);
            imageInterpolation.sprite.texture.Apply();
            makeWeightImage();
            SetPowA(40);    //set sensivity

        }
        else//Sensor Not detected
        {
            if(STATUS == (int)MODE.START_LEFT)
            {
                STATUS = (int)MODE.WAITING_RIGHT;
                leftIdx = DynamicframeCount;
                secondTestTime = currentTime;
            }
            else if(STATUS == (int)MODE.START_RIGHT)
            {
                STATUS = (int)MODE.END;
                waitingText.text = "측정이 완료되었습니다!";
                waitingText.gameObject.SetActive(true);
                dynamicHalfLine.gameObject.SetActive(false);
                rightIdx = DynamicframeCount;
                loadingShape.gameObject.SetActive(false);
                waitingText2.gameObject.SetActive(false);
                endText.gameObject.SetActive(true);
                checkShape.gameObject.SetActive(true);

                //showResultScene();
                Close();
            }
            //Set default pixel
            for (int i = 0; i < interpolationImage.Length; i++)
            {
                interpolationImage[i].r = 34;
                interpolationImage[i].g = 46;
                interpolationImage[i].b = 56;

                cannyImage[i].r = 0;
                cannyImage[i].g = 0;
                cannyImage[i].b = 0;
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
            imageInterpolation.sprite.texture.SetPixels32(interpolationImage);
            imageInterpolation.sprite.texture.Apply();

            imageCanny.sprite.texture.SetPixels32(cannyImage);
            imageCanny.sprite.texture.Apply();

            imageTarget.sprite.texture.SetPixels32(targetImage);
            imageTarget.sprite.texture.Apply();

            imageDetected.sprite.texture.SetPixels32(detectedImage);
            imageDetected.sprite.texture.Apply();

            interpolationTextureGaussianBlur(ref targetImage, ref interpolationImage, WIDTH, HEIGHT, SCALE, SCALE, 1);
            addImage(ref interpolationImage, ref cannyImage, ref resultImage, WIDTH, HEIGHT, SCALE);
            for(int i = 0; i < resultImage.Length; i++)
            {
                if(resultImage[i].r == 0 && resultImage[i].g == 0 && resultImage[i].b == 128)
                {
                    resultImage[i].r = 34;
                    resultImage[i].g = 46;
                    resultImage[i].b = 56;
                }
            }
            imageResult.sprite.texture.SetPixels32(resultImage);
            imageResult.sprite.texture.Apply();
            //DynamicframeCount = 0;
        }
    }

    void getDynamicPoint(int START_IDX)
    {
        var originalImage = imageTarget.sprite.texture.GetPixels32();
        var detectedImage = imageDetected.sprite.texture.GetPixels32();
        var cannyImage = imageCanny.sprite.texture.GetPixels32();

        Queue<int> bfsQueue = new Queue<int>();
        Queue<int> visitQueue = new Queue<int>();
        Queue<int> centerWidthQueue = new Queue<int>();
        Queue<int> centerHeightQueue = new Queue<int>();

        int height = WIDTH * SCALE;
        var maxHeight = new Tuple<int, int, int, int>(0, 0, 0, 0);

        visitQueue = StaticVaribleHandler.SearchPixel(START_IDX, originalImage);
        maxHeight = StaticVaribleHandler.getLongestPixel(visitQueue, originalImage);    //최고길이, 최고길이 세로idx, 가로idx 시작, 가로idx끝 
        
        topYHistory[DynamicframeCount] = maxHeight.Item3;
        bottomYHistory[DynamicframeCount] = maxHeight.Item4;

        int copX = 0;
        int copY = 0;
        int peakForceIdx = 0;
        int peakForceVal = 255;
        int avg = 0;

        foreach (int idx in visitQueue)
        {
            avg += originalImage[idx].r;

            //GET MAX VALUE
            if (peakForceVal > originalImage[idx].r)
            {
                peakForceIdx = idx;
                peakForceVal = originalImage[idx].r;
            }
            copX += (idx % WIDTH);
            copY += (idx / WIDTH);
        }

        //COP
        copX /= visitQueue.Count();
        copY /= visitQueue.Count();

        COPX = copX * SCALE;
        COPY = copY * SCALE;

        COPXHistory[DynamicframeCount] = COPX;
        COPYHistory[DynamicframeCount] = COPY;

        PeakForceX = peakForceIdx % WIDTH * SCALE;
        PeakForceY = peakForceIdx / WIDTH * SCALE;
        PeakForceXHistory[DynamicframeCount] = PeakForceX;
        PeakForceYHistory[DynamicframeCount] = PeakForceY;

        imageDetected.sprite.texture.SetPixels32(detectedImage);
        imageDetected.sprite.texture.Apply();
        getCannyEdge(ref detectedImage, ref cannyImage, WIDTH, HEIGHT, SCALE);
        imageCanny.sprite.texture.SetPixels32(cannyImage);
        imageCanny.sprite.texture.Apply();
    }

    void makeWeightImage()
    {
        var resultImage = imageResult.sprite.texture.GetPixels32();
        var interpolationImage = imageInterpolation.sprite.texture.GetPixels32();
        var cannyImage = imageCanny.sprite.texture.GetPixels32();

        addImage(ref interpolationImage, ref cannyImage, ref resultImage, WIDTH, HEIGHT, SCALE);

        for (int i = 0; i < resultImage.Length; i++)
        {
            if (resultImage[i].r == 128 && resultImage[i].g == 128 && resultImage[i].b == 128)
            {
                resultImage[i].r = 255;
                resultImage[i].g = 255;
                resultImage[i].b = 255;
            }
        }
        //특징점 페인팅 opencv으로 대체

        var resultImage2 = imageResult.sprite.texture.GetPixels();
        for (int i = 0; i < resultImage.Length; i++)
        {
            resultImage2[i].r = resultImage[i].r / 255f;
            resultImage2[i].g = resultImage[i].g / 255f;
            resultImage2[i].b = resultImage[i].b / 255f;

            if (resultImage[i].r == 0 && resultImage[i].g == 0 && resultImage[i].b > 110)
            {

                resultImage[i].r = 34;
                resultImage[i].g = 46;
                resultImage[i].b = 56;
                
                resultImage2[i].r = 34 / 255f;
                resultImage2[i].g = 46 / 255f;
                resultImage2[i].b = 56 / 255f;
                resultImage2[i].a = 1f;
            }
        }
        if (STATUS == (int)MODE.START_LEFT)
        {
            addDynamicText(ref resultImage, WIDTH, HEIGHT, SCALE, COPX, COPY, PeakForceX, PeakForceY, COPXHistory, COPYHistory, PeakForceXHistory, PeakForceYHistory, 1, DynamicframeCount);
        }
        else if (STATUS == (int)MODE.START_RIGHT)
        {
            addDynamicText(ref resultImage, WIDTH, HEIGHT, SCALE, COPX, COPY, PeakForceX, PeakForceY, COPXHistory, COPYHistory, PeakForceXHistory, PeakForceYHistory, leftIdx + 1, DynamicframeCount);
        }
        imageResult.sprite.texture.SetPixels32(resultImage);
        imageResult.sprite.texture.Apply();
        DynamicframeCount++;
    }
}
