using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

[Serializable]
public class sendFlaskData
{
    public byte[] byteData;
}

public class StaticCSVHandler : MonoBehaviour
{
    [DllImport("OpenCVDLL")]
    private static extern void interpolationTexture(ref Color32[] rawImage, ref Color32[] interpolationImage, int width, int height, double widthScale, double heightScale, int flag);

    public Text currentFrameText;
    public Image frameImage;
    public Image pixelImage;
    public Image staticResultImage;
    public Image staticResultInterpolationImage;

    public int WIDTH = 52;
    public int HEIGHT = 44;
    public int SCALE = 10;
    public int frame_idx = 0;
    public bool isStart = true;

    public List<int> leftToeList = new List<int>();
    public List<int> rightToeList = new List<int>();
    public List<int> tmpToeList = new List<int>();
    public List<int> toeList = new List<int>();

    public static List<float> leftArchIndexList = new List<float>();
    public static List<float> rightArchIndexList = new List<float>();

    public RawImage imageObj;

    public ShowResultHandler dynamicResult;
    public Text statusText;

    // Start is called before the first frame update
    void Start()
    {
        if (StaticVaribleHandler.isEndStatic)
        {
            StartCoroutine(DataPost("http://127.0.0.1:5000/getframedata", frame_idx));
        }
    }

    IEnumerator DataPost(string url, int frameIdx){

        //make load csv
        // C:/Users/admin/Desktop/flaskProject/data.csv
        // C:/Users/Carly/PycharmProjects/flaskProject/data.csv
        using (var writer = new CsvFileWriter("./flaskProject/data.csv"))
        {
            List<string> columns = new List<string>();
            columns.Clear();
            int count = 0;

            var imageFrame = frameImage.sprite.texture.GetPixels32();

            for(int i = 1; i <= StaticHandler.WIDTH * StaticHandler.HEIGHT; i++)
            {
                imageFrame[i - 1].g = 0;
                imageFrame[i - 1].b = 0;

                if (StaticHandler.StaticFrameRecordArray[frameIdx, i - 1] == 255)
                {
                    imageFrame[i - 1].r = 0;
                    StaticHandler.StaticFrameRecordArray[frameIdx, i - 1] = 255;
                }
                else if(StaticHandler.StaticFrameRecordArray[frameIdx, i - 1] > 245)
                {
                    imageFrame[i - 1].r = 255;
                    StaticHandler.StaticFrameRecordArray[frameIdx, i - 1] = 255;
                }
                else
                {
                    imageFrame[i - 1].r = 128;
                    //imageFrame[i - 1].r = (byte)StaticHandler.StaticFrameRecordArray[frameIdx, i - 1];
                }
                columns.Add(StaticHandler.StaticFrameRecordArray[frameIdx, i - 1].ToString());

                if (i % StaticHandler.WIDTH == 0)
                {
                    writer.WriteRow(columns);
                    columns.Clear();
                }
            }
            frameImage.sprite.texture.SetPixels32(imageFrame);
            frameImage.sprite.texture.Apply();
        }
        var staticImage = staticResultImage.sprite.texture.GetPixels32();
        var interpolationImage = staticResultInterpolationImage.sprite.texture.GetPixels32();
        for (int i = 0; i < staticImage.Length; i++)
        {
            int value = StaticHandler.StaticFrameDetectedSum[i] / StaticVaribleHandler.MaximumStaticFrameCount;
            //report static result images
            if(value <= 50) //filtering range
            {
                staticImage[i].r = 255;
                staticImage[i].g = 255;
                staticImage[i].b = 255;
            }
            else if(value >= 255)
            {
                staticImage[i].r = 0;
                staticImage[i].g = 0;
                staticImage[i].b = 0;
            }
            else
            {
                staticImage[i].r = ((byte)(255 - value));
                staticImage[i].g = ((byte)(255 - value));
                staticImage[i].b = ((byte)(255 - value));
            }
        }

        //remove noise
        for (int i = 0; i < staticImage.Length; i++)
        {
            int topleft = (i - StaticHandler.WIDTH - 1) > 0 ? (i - StaticHandler.WIDTH - 1) : 0;
            int topright = (i - StaticHandler.WIDTH + 1) > 0 ? (i - StaticHandler.WIDTH + 1) : 0;
            int bottomleft = (i + StaticHandler.WIDTH - 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH - 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;
            int bottomright = (i + StaticHandler.WIDTH + 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH + 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;

            int left = (i - 1) > 0 ? (i - 1) : 0;
            int right = (i + 1) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + 1) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;
            int top = (i - StaticHandler.WIDTH) > 0 ? (i - StaticHandler.WIDTH) : 0;
            int bottom = (i + StaticHandler.WIDTH) < StaticHandler.WIDTH * StaticHandler.HEIGHT ? (i + StaticHandler.WIDTH) : StaticHandler.WIDTH * StaticHandler.HEIGHT - 1;

            if (staticImage[topleft].r == 255 && staticImage[topright].r == 255 && staticImage[bottomleft].r == 255 && staticImage[bottomright].r == 255 &&
                staticImage[left].r == 255 && staticImage[right].r == 255 && staticImage[top].r == 255 && staticImage[bottom].r == 255)
            {
                staticImage[i].r = 255;
                staticImage[i].g = 255;
                staticImage[i].b = 255;

                staticImage[i].r = 255;
                staticImage[i].g = 255;
                staticImage[i].b = 255;
            }
        }

        staticResultImage.sprite.texture.SetPixels32(staticImage);
        staticResultImage.sprite.texture.Apply();

        interpolationTexture(ref staticImage, ref interpolationImage, StaticHandler.WIDTH, StaticHandler.HEIGHT, StaticHandler.SCALE, StaticHandler.SCALE, 1);
        for (int i = 0; i<interpolationImage.Length;i++)
        {
            if(interpolationImage[i].r == 0 && interpolationImage[i].g == 0 && interpolationImage[i].b == 128)
            {
                interpolationImage[i].r = 255;
                interpolationImage[i].g = 255;
                interpolationImage[i].b = 255;
            }
        }
        
        staticResultInterpolationImage.sprite.texture.SetPixels32(interpolationImage);
        staticResultInterpolationImage.sprite.texture.Apply();

        WWWForm form = new WWWForm();
        byte[] temp = new byte[2288];
        for (int i = 0; i < 2288; i++)
        {
            temp[i] = (byte)StaticHandler.StaticFrameRecordArray[frameIdx, i];
        }

        sendFlaskData data = new sendFlaskData();   //sendData;
        data.byteData = temp;
        string json = JsonUtility.ToJson(data);
 
        using (UnityWebRequest www = UnityWebRequest.Post(url, json))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if(www.isNetworkError){
                Debug.Log(www.error);
            }
            else{
                string output = www.downloadHandler.text;

                output = output.Replace('[' , ' ');
                output = output.Replace(']', ' ');

                string[] temps = output.Split(',');
                int[] nums = new int[2288];
                var originalImage = pixelImage.sprite.texture.GetPixels32();

                for(int i = 0; i<nums.Length; i++)
                {
                    nums[i] = int.Parse(temps[i]);
                    if (nums[i] == 128 && StaticHandler.StaticFrameRecordArray[frameIdx, i] != 255)
                    {
                        tmpToeList.Add(i);
                    }
                }
                //Compare AND
                for (int i = 0; i<nums.Length; i++)
                { 
                    nums[i] = int.Parse(temps[i]);

                    if(nums[i] == 128 && StaticHandler.StaticFrameRecordArray[frameIdx, i] != 255)
                    {
                        int toeCount = StaticVaribleHandler.DetectToe(originalImage, i, tmpToeList);
                        if(toeCount >= 1)
                        {
                            toeList.Add(i); //toe
                        }
                        else
                        {
                            //do noting toe noise
                        }
                    }
                    else if(nums[i] == 0 && StaticHandler.StaticFrameRecordArray[frameIdx, i] != 255)
                    {
                        //do nothing
                    }
                    else{
                        //blank
                        StaticHandler.StaticFrameRecordArray[frameIdx, i] = 255;
                    }
                }
                makeFrameImage(frame_idx);
            }
        }
    }
    void makeFrameImage(int start_idx)
    {
        var pixelResult = pixelImage.sprite.texture.GetPixels32();

        for (int i = 0; i < pixelResult.Length; i++)
        {
            pixelResult[i] = new Color32
            {
                r = (byte)(StaticHandler.StaticFrameRecordArray[start_idx, i]),
                g = (byte)(StaticHandler.StaticFrameRecordArray[start_idx, i]),
                b = (byte)(StaticHandler.StaticFrameRecordArray[start_idx, i])
            };
        }
        pixelImage.sprite.texture.SetPixels32(pixelResult);
        pixelImage.sprite.texture.Apply();

        bool left_flag = true;
        bool right_flag = true;

        for (int i = 0; i < pixelResult.Length; i++)
        {
            if (pixelResult[i].r == 255 && pixelResult[i].g == 255 && pixelResult[i].b == 255)
            {
                //default pixel
            }
            else if (i % WIDTH < WIDTH / 2 && left_flag)
            {
                //LEFT FOOT
                left_flag = false;
                getLeftRightCenterPoint("LEFT", i);   //left avg, left_top avg, left_bottom avg, arch index
            }

            else if (i % WIDTH > WIDTH / 2 && right_flag)
            {
                //RIGHT FOOT
                right_flag = false;
                getLeftRightCenterPoint("RIGHT", i);
            }
            if (!left_flag && !right_flag)
            {
                //DETECTED ALL FOOT
                leftToeList.Clear();
                rightToeList.Clear();
                toeList.Clear();
                break;
            }
        }
        frame_idx++;
        if(frame_idx != 100 && isStart)
        {
            StartCoroutine(DataPost("http://127.0.0.1:5000/getframedata", frame_idx));
        }
        else
        {
            isStart = false;
        }
    }
    void getLeftRightCenterPoint(string FOOT_TYPE, int START_IDX)
    {
        var originalImage = pixelImage.sprite.texture.GetPixels32();

        Queue<int> visitQueue = new Queue<int>();

        int height = WIDTH * SCALE;
        
        int archA = 0;
        int archB = 0;
        int archC = 0;
        double archIndex = 0;

        var maxHeight = new Tuple<int, int, int, int>(0, 0, 0, 0);

        visitQueue = StaticVaribleHandler.SearchPixel(START_IDX, originalImage);
        maxHeight = StaticVaribleHandler.getLongestPixel(visitQueue, originalImage);    //최고길이, 최고길이 세로idx, 가로idx 시작, 가로idx끝 

        int footTop = maxHeight.Item3;
        int footBottom = maxHeight.Item4;

        int leftMaxY = 0;
        int leftMaxIdx = 0;
        int rightMaxY = 0;
        int rightMaxIdx = 0;

        //and
        for(int i = 0; i<originalImage.Length; i++)
        {
            if (i - WIDTH - 1 < 0 && i + WIDTH + 1 > WIDTH * HEIGHT && originalImage[i].r == 255 && originalImage[i + 1].r != 255 && originalImage[i - 1].r != 255 
                && originalImage[i + WIDTH].r != 255 && originalImage[i - WIDTH].r != 255 && originalImage[i + WIDTH + 1].r != 255 && originalImage[i + WIDTH - 1].r != 255
                && originalImage[i - WIDTH + 1].r != 255 && originalImage[i - WIDTH - 1].r != 255)
            {
                int t = (originalImage[i + WIDTH].r + originalImage[i - WIDTH].r + originalImage[i + 1].r + originalImage[i - 1].r + originalImage[i + WIDTH + 1].r + originalImage[i + WIDTH - 1].r + originalImage[i - WIDTH + 1].r + originalImage[i - WIDTH - 1].r )/ 8;
                originalImage[i].r = (byte)t;
                originalImage[i].g = (byte)t;
                originalImage[i].b = (byte)t;

                visitQueue.Enqueue(i);
            }
        }

        foreach (int idx in visitQueue){
            if(FOOT_TYPE == "LEFT" && toeList.Contains(idx))
            {
                leftToeList.Add(idx);
                int leftY = idx / 52;
                if(leftMaxY < leftY)
                {
                    leftMaxY = leftY;
                }
                if(leftMaxIdx < idx)
                {
                    leftMaxIdx = idx;
                }
            }
            else if(FOOT_TYPE == "RIGHT" && toeList.Contains(idx))
            {
                rightToeList.Add(idx);
                int rightY = idx / 52;
                if(rightMaxY < rightY)
                {
                    rightMaxY = rightY;
                }
                if (rightMaxIdx < idx)
                {
                    rightMaxIdx = idx;
                }
            }
        }

        if(FOOT_TYPE == "LEFT" && leftMaxIdx != 0)
        {
            while (true)
            {
                leftMaxIdx += WIDTH;
                if (originalImage[leftMaxIdx].r == 255 && originalImage[leftMaxIdx-1].r == 255 && originalImage[leftMaxIdx+1].r == 255)  //has no pixel
                {
                    leftMaxY += 1;
                }
                else
                {
                    break;
                }
            }
        }
        else if(FOOT_TYPE == "RIGHT" && rightMaxIdx != 0)
        {
            while (true)
            {
                rightMaxIdx += WIDTH;
                if (originalImage[rightMaxIdx].r == 255 && originalImage[rightMaxIdx - 1].r == 255 && originalImage[rightMaxIdx + 1].r == 255)  //has no pixel
                {
                    rightMaxY += 1;
                }
                else
                {
                    break;
                }
            }
        }

        if (FOOT_TYPE == "LEFT")
        {
            if(leftToeList.Count == 0)
            {
                footTop = maxHeight.Item3;
            }
            else
            {
                footTop = leftMaxY + 1;
            }
        }
        else if(FOOT_TYPE == "RIGHT")
        {
            if (rightToeList.Count == 0)
            {
                footTop = maxHeight.Item3;
            }
            else
            {
                footTop = rightMaxY + 1;
            }
        }
        foreach (int idx in visitQueue)
        {
            //GET ARCH
            if(idx / WIDTH <= footTop)
            {
                //TOE
                originalImage[idx].r = 255;
            }
            else if (idx / WIDTH <= (footBottom - footTop) / 3 + footTop)
            {
                archA++;
                originalImage[idx].r = 0;
            }
            else if (idx / WIDTH <= (footBottom - footTop) / 3 * 2 + footTop)
            {
                archB++;
                originalImage[idx].r = 128;
            }
            else
            {
                archC++;
                originalImage[idx].r = 255;
            }
            originalImage[idx].g = 0;
            originalImage[idx].b = 0;
        }
        archIndex = Math.Round(archB / (double)(archA + archB + archC), 2);
        if(FOOT_TYPE == "LEFT")
        {
            leftArchIndexList.Add((float)archIndex);
        }
        else
        {
            rightArchIndexList.Add((float)archIndex);
        }
        pixelImage.sprite.texture.SetPixels32(originalImage);
        pixelImage.sprite.texture.Apply();

        visitQueue.Clear();
    }
}

