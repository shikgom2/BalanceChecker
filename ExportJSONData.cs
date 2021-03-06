using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.SceneManagement;

public class ExportJSONData : MonoBehaviour
{
    public bool exportDataFlag = false;
    public Image staticResultObj;
    public RawImage imageObj;

    [Serializable]

    public class returnObject
    {
        public staticObject stat;
        public dynamicObject[] dya;
        public dynamicRecultObject dyare;
    }
    public class staticObject
    {
        public double leftPercent;
        public double rightPercent;
        public double topPercent;
        public double bottomPercent;

        public double topleftPercent;
        public double toprightPercent;
        public double bottomleftPercent;
        public double bottomrightPercent;

        public double leftAreaCount;
        public double rightAreaCount;

        public int leftPeakforceX;
        public int leftPeakforceY;
        public int rightPeakforceX;
        public int rightPeakforceY;

        public double leftArchindex;
        public double rightArchindex;
        public double leftFootLength;
        public double rightFootLength;
        public int leftCOPX;
        public int leftCOPY;
        public int rightCOPX;
        public int rightCOPY;
        public int COGX;
        public int COGY;

        public string staticImage;
    }


    public class sendFlaskData
    {
        public byte[] byteData;
    }
    public class dynamicObject
    {
        public int frameNo;
        public string isLeftRight;
        public int COPX;
        public int COPY;
        public int peakForceX;
        public int peakForceY;
        public int frameValueSum;
        public string frameImage;
    }

    public class dynamicRecultObject
    {
        public double leftContactPercent;
        public double leftMidStancePercent;
        public double leftPropulsivePercent;

        public double rightContactPercent;
        public double rightMidStancePercent;
        public double rightPropulsivePercent;

        public string leftPeakForceXHistory;
        public string leftPeakForceYHistory;
        public string rightPeakForceXHistory;
        public string rightPeakForceYHistory;

        public string leftImage;
        public string rightImage;
    }

    public enum Rotate
    {
        LEFT,
        RIGHT
    }
    staticObject staticJsonData = new staticObject();
    dynamicRecultObject dynamicResultData = new dynamicRecultObject();
    dynamicObject dynamicJsonData = new dynamicObject();

    // Start is called before the first frame update
    void Start()
    {
        exportDataFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(StaticVaribleHandler.isEndStatic && StaticVaribleHandler.isEndDynamic && !exportDataFlag)
        {
            var dirPath = Application.dataPath + "/../ResultImage/";
            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            StaticCSVHandler.leftArchIndexList.Sort();
            StaticCSVHandler.rightArchIndexList.Sort();

            Array.Sort(StaticHandler.LeftPercentHistory);
            Array.Sort(StaticHandler.RightPercentHistory);
            Array.Sort(StaticHandler.TopPercentHistory);
            Array.Sort(StaticHandler.BottomPercentHistory);

            Array.Sort(StaticHandler.leftAreaCountHistory);
            Array.Sort(StaticHandler.rightAreaCountHistory);

            Array.Sort(StaticHandler.leftPeakForceXHistory);
            Array.Sort(StaticHandler.leftPeakForceYHistory);
            Array.Sort(StaticHandler.rightPeakForceXHistory);
            Array.Sort(StaticHandler.rightPeakForceYHistory);   

            Array.Sort(StaticHandler.leftFootLengthHistory);
            Array.Sort(StaticHandler.rightFootLengthHistory);

            float leftArchIndex = 0f;
            float rightArchIndex = 0f;
            float leftPercent = 0f;
            float rightPercent = 0f;
            float topPercent = 0f;
            float bottomPercent = 0f;

            double leftPixelCount = 0;
            double rightPixelCount = 0;
            double leftFootLength = 0;
            double rightFootLength = 0;


            float leftPeakForceX = 0f;
            float leftPeakForceY = 0f;
            float rightPeakForceX = 0f;
            float rightPeakForceY = 0f;
            float leftCOPX = 0f;
            float leftCOPY = 0f;
            float rightCOPX = 0f;
            float rightCOPY = 0f;
            float COGX = 0f;
            float COGY = 0f;
            int count = 0;
            //Median filtering

            for (int i = (int)(StaticHandler.StaticframeCount * 0.2); i <= (int)(StaticHandler.StaticframeCount * 0.8); i++)
            {
                leftArchIndex += StaticCSVHandler.leftArchIndexList[i];
                rightArchIndex += StaticCSVHandler.rightArchIndexList[i];
                leftPercent += StaticHandler.LeftPercentHistory[i];
                rightPercent += StaticHandler.RightPercentHistory[i];
                topPercent += StaticHandler.TopPercentHistory[i];
                bottomPercent += StaticHandler.BottomPercentHistory[i];
                leftPixelCount += StaticHandler.leftAreaCountHistory[i];
                rightPixelCount += StaticHandler.rightAreaCountHistory[i];

                leftFootLength += StaticHandler.leftFootLengthHistory[i];
                rightFootLength += StaticHandler.rightFootLengthHistory[i];

                leftPeakForceX += StaticHandler.leftPeakForceXHistory[i];
                leftPeakForceY += StaticHandler.leftPeakForceYHistory[i];
                rightPeakForceX += StaticHandler.rightPeakForceXHistory[i];
                rightPeakForceY += StaticHandler.rightPeakForceYHistory[i];
                leftCOPX += StaticHandler.leftCOPXHistory[i];
                leftCOPY += StaticHandler.leftCOPYHistory[i];
                rightCOPX += StaticHandler.rightCOPXHistory[i];
                rightCOPY += StaticHandler.rightCOPYHistory[i];
                COGX += StaticHandler.COGXHistory[i];
                COGY += StaticHandler.COGYHistory[i];
                count++;
            }

            leftArchIndex /= count;
            rightArchIndex /= count;
            leftPercent /= count;
            rightPercent /= count;
            topPercent /= count;
            bottomPercent /= count;
            leftPixelCount /= count;
            leftPixelCount *= 48.3;
            rightPixelCount /= count;
            rightPixelCount *= 48.3;

            leftFootLength /= count;
            leftFootLength *= 6.95;
            rightFootLength /= count;
            rightFootLength *= 6.95;

            leftPeakForceX /= count;
            leftPeakForceY /= count;
            rightPeakForceX /= count;
            rightPeakForceY /= count;
            leftCOPX /= count;
            leftCOPY /= count;
            rightCOPX /= count;
            rightCOPY /= count;
            COGX /= count;
            COGY /= count;

            leftPixelCount /= StaticVaribleHandler.MaximumStaticFrameCount;
            rightPixelCount /= StaticVaribleHandler.MaximumStaticFrameCount;

            Debug.Log("LEFT arch index : " + leftArchIndex + " RIGHT arch index : " + rightArchIndex);
            Debug.Log("LEFT Area : " + leftPixelCount + " RIGHT Area : " + rightPixelCount);
            Debug.Log("LEFT FOOT LENGTH : " + leftFootLength + " RIGHT FOOT LENGTH : " + rightFootLength);
            Debug.Log("LEFT PEAKFORCE : " + leftPeakForceX + "," + leftPeakForceY);
            Debug.Log("RIGHT PEAKFORCE : " + rightPeakForceX + "," + rightPeakForceY);
            Debug.Log("LEFT COP : " + leftCOPX + "," + leftCOPY);
            Debug.Log("RIGHT COP : " + rightCOPX + "," + rightCOPY);
            Debug.Log("COG : " + COGX + "," + COGY);

            WWWForm form = new WWWForm();

            staticJsonData.leftArchindex = Math.Round(leftArchIndex,2);
            staticJsonData.rightArchindex = Math.Round(rightArchIndex, 2);
            staticJsonData.leftAreaCount = Math.Round(leftPixelCount,2);
            staticJsonData.rightAreaCount = Math.Round(rightPixelCount,2);
            staticJsonData.leftPercent = Math.Round(leftPercent,2);
            staticJsonData.rightPercent = Math.Round(rightPercent,2);
            staticJsonData.topPercent = Math.Round(topPercent,2);
            staticJsonData.bottomPercent = Math.Round(bottomPercent,2);
            staticJsonData.leftFootLength = (int)leftFootLength;
            staticJsonData.rightFootLength = (int)rightFootLength;
            staticJsonData.leftPeakforceX = (int)leftPeakForceX;
            staticJsonData.rightPeakforceX = (int)rightPeakForceX;
            staticJsonData.leftPeakforceY = (int)leftPeakForceY;
            staticJsonData.rightPeakforceY = (int)rightPeakForceY;
            staticJsonData.leftCOPX = (int)leftCOPX;
            staticJsonData.leftCOPY = (int)leftCOPY;
            staticJsonData.rightCOPX = (int)rightCOPX;
            staticJsonData.rightCOPY = (int)rightCOPY;
            staticJsonData.COGX = (int)COGX;
            staticJsonData.COGY = (int)COGY;

            double topleft = leftPercent + topPercent;
            double topright = rightPercent + topPercent;
            double bottomleft = leftPercent + bottomPercent;
            double bottomright = rightPercent + bottomPercent;

            staticJsonData.topleftPercent = Math.Round((topleft / (topleft + topright + bottomleft + bottomright)), 3) * 100;
            staticJsonData.toprightPercent = Math.Round((topright / (topleft + topright + bottomleft + bottomright)), 3) * 100;
            staticJsonData.bottomleftPercent = Math.Round((bottomleft / (topleft + topright + bottomleft + bottomright)), 3) * 100;
            staticJsonData.bottomrightPercent = Math.Round((bottomright / (topleft + topright + bottomleft + bottomright)), 3) * 100;

            staticResultObj = GameObject.Find("staticResultInterpolationImage").GetComponent<Image>();
            Texture2D rawImageTexture = (Texture2D)staticResultObj.mainTexture;
            rawImageTexture = FlipTexture(rawImageTexture, true);
            byte[] imageData = rawImageTexture.EncodeToPNG();
            File.WriteAllBytes(dirPath + " staticResult" + ".png" , imageData);
            string png = Convert.ToBase64String(imageData);

            staticJsonData.staticImage = png;

            if(StaticVaribleHandler.currentKinectCode != "")
            {
                string resultJSon = "{";
                resultJSon += "\"Static\" : [";
                string staticJson = JsonUtility.ToJson(staticJsonData);
                resultJSon += staticJson;
                resultJSon += "],";

                string dynamicJson = "[";

                int leftCount = DynamicHandler.leftIdx > 10 ? 10 : DynamicHandler.leftIdx;

                string leftPeakForceXHistory = "";
                string leftPeakForceYHistory = "";
                string rightPeakForceXHistory = "";
                string rightPeakForceYHistory = "";

                for (int i = 0; i < leftCount; i++)
                {
                    dynamicJsonData.isLeftRight = "LEFT";
                    dynamicJsonData.frameNo = i;
                    
                    dynamicJsonData.COPX = DynamicHandler.COPXHistory[i];
                    dynamicJsonData.COPY = DynamicHandler.COPYHistory[i];
                    dynamicJsonData.peakForceX = DynamicHandler.PeakForceXHistory[i];
                    dynamicJsonData.peakForceY = DynamicHandler.PeakForceYHistory[i];

                    leftPeakForceXHistory += DynamicHandler.PeakForceXHistory[i].ToString();
                    leftPeakForceXHistory += ",";
                    leftPeakForceYHistory += DynamicHandler.PeakForceYHistory[i].ToString();
                    leftPeakForceYHistory += ",";


                    imageObj = GameObject.Find("Frame_" + i).GetComponent<RawImage>();
                    rawImageTexture = (Texture2D)imageObj.texture;
                    rawImageTexture = FlipTexture(rawImageTexture, true);
                    rawImageTexture = RotateTexture(rawImageTexture, Rotate.LEFT);
                    imageData = rawImageTexture.EncodeToPNG();
                   
                    File.WriteAllBytes(dirPath + "Dynamic_LEFT_" + i + ".png", imageData);

                    png = Convert.ToBase64String(imageData);
                    dynamicJsonData.frameImage = png;

                    byte[] tmpArray = new byte[52 * 44];
                    int sum = 0;
                    for (int k = 0; k < tmpArray.Length; k++)
                    {
                        tmpArray[k] = ((byte)(255 - DynamicHandler.DynamicFrameRecordArray[i, k]));
                        sum += tmpArray[k];
                    }
                    //sum = sum / (StaticHandler.WIDTH * StaticHandler.HEIGHT);
                    dynamicJsonData.frameValueSum = sum;

                    dynamicJson += JsonUtility.ToJson(dynamicJsonData);

                    if (i != DynamicHandler.rightIdx - 1)
                    {
                        dynamicJson += ",";
                    }
                }
                leftPeakForceXHistory = leftPeakForceXHistory.Substring(0, leftPeakForceXHistory.Length - 1);
                leftPeakForceYHistory = leftPeakForceYHistory.Substring(0, leftPeakForceYHistory.Length - 1);

                int rightStart = DynamicHandler.leftIdx;
                int rightEnd = DynamicHandler.rightIdx - DynamicHandler.leftIdx > 10 ? DynamicHandler.leftIdx + 10 : DynamicHandler.rightIdx;

                for (int i = rightStart; i < rightEnd; i++)
                {
                    dynamicJsonData.isLeftRight = "RIGHT";
                    dynamicJsonData.frameNo = i - DynamicHandler.leftIdx;
                    
                    dynamicJsonData.COPX = DynamicHandler.COPXHistory[i];
                    dynamicJsonData.COPY = DynamicHandler.COPYHistory[i];
                    dynamicJsonData.peakForceX = DynamicHandler.PeakForceXHistory[i];
                    dynamicJsonData.peakForceY = DynamicHandler.PeakForceYHistory[i];

                    rightPeakForceXHistory += DynamicHandler.PeakForceXHistory[i].ToString();
                    rightPeakForceXHistory += ",";
                    rightPeakForceYHistory += DynamicHandler.PeakForceYHistory[i].ToString();
                    rightPeakForceYHistory += ",";


                    imageObj = GameObject.Find("Frame_" + i).GetComponent<RawImage>();

                    rawImageTexture = (Texture2D)imageObj.texture;
                    rawImageTexture = RotateTexture(rawImageTexture, Rotate.LEFT);
                    rawImageTexture = FlipTexture(rawImageTexture, true);
                    imageData = rawImageTexture.EncodeToPNG();
                    File.WriteAllBytes(dirPath + "Dynamic_RIGHT_" + (i - DynamicHandler.leftIdx) + ".png", imageData);

                    png = Convert.ToBase64String(imageData);
                    dynamicJsonData.frameImage = png;

                    byte[] tmpArray = new byte[52 * 44];
                    int sum = 0;
                    for (int k = 0; k < tmpArray.Length; k++)
                    {
                        tmpArray[k] = ((byte)(255 - DynamicHandler.DynamicFrameRecordArray[i, k]));
                        sum += tmpArray[k];
                    }
                    //sum = sum / (StaticHandler.WIDTH * StaticHandler.HEIGHT);
                    dynamicJsonData.frameValueSum = sum;

                    dynamicJson += JsonUtility.ToJson(dynamicJsonData);

                    if (i != rightEnd - 1)
                    {
                        dynamicJson += ",";
                    }
                }
                rightPeakForceXHistory = rightPeakForceXHistory.Substring(0, rightPeakForceXHistory.Length - 1);
                rightPeakForceYHistory = rightPeakForceYHistory.Substring(0, rightPeakForceYHistory.Length - 1);

                dynamicJson += "]";

                resultJSon += "\"Dynamic\" : [";
                resultJSon += dynamicJson;
                resultJSon += "],";

                dynamicResultData.leftContactPercent = Math.Round(ShowResultHandler.leftValueArray[0], 2);
                dynamicResultData.leftMidStancePercent = Math.Round(ShowResultHandler.leftValueArray[1], 2);
                dynamicResultData.leftPropulsivePercent = Math.Round(ShowResultHandler.leftValueArray[2], 2);

                dynamicResultData.rightContactPercent = Math.Round(ShowResultHandler.rightValueArray[0], 2);
                dynamicResultData.rightMidStancePercent = Math.Round(ShowResultHandler.rightValueArray[1], 2);
                dynamicResultData.rightPropulsivePercent = Math.Round(ShowResultHandler.rightValueArray[2], 2);

                dynamicResultData.leftPeakForceXHistory = leftPeakForceXHistory;
                dynamicResultData.leftPeakForceYHistory = leftPeakForceXHistory;
                dynamicResultData.rightPeakForceXHistory = rightPeakForceXHistory;
                dynamicResultData.rightPeakForceYHistory = rightPeakForceYHistory;

                staticResultObj = GameObject.Find("leftCropImage").GetComponent<Image>();
                rawImageTexture = (Texture2D)staticResultObj.mainTexture;
                rawImageTexture = FlipTexture(rawImageTexture, true);
                rawImageTexture = RotateTexture(rawImageTexture, Rotate.LEFT);

                imageData = rawImageTexture.EncodeToPNG();
                File.WriteAllBytes(dirPath + "Dynamic_LEFT.png", imageData);

                png = Convert.ToBase64String(imageData);
                dynamicResultData.leftImage = png;


                staticResultObj = GameObject.Find("rightCropImage").GetComponent<Image>();
                rawImageTexture = (Texture2D)staticResultObj.mainTexture;
                rawImageTexture = RotateTexture(rawImageTexture, Rotate.LEFT);
                rawImageTexture = FlipTexture(rawImageTexture, true);

                imageData = rawImageTexture.EncodeToPNG();
                File.WriteAllBytes(dirPath + "Dynamic_RIGHT.png", imageData);

                png = Convert.ToBase64String(imageData);
                dynamicResultData.rightImage = png;

                string dynamicResultjson = JsonUtility.ToJson(dynamicResultData);

                resultJSon += "\"DynamicResult\" : [";
                resultJSon += dynamicResultjson;
                resultJSon += "]}";

                //"C:/AutoSet10/public_html/balanceCheckerImage/" + StaticVaribleHandler.currentKinectCode.Substring(0, 7) + "
                //";
                // C:/Users/admin/Desktop/flaskProject/data.csv                " + StaticVaribleHandler.currentKinectCode.Substring(0, 7) + ".json";

                string fullpth = dirPath + "result.json";
                if (!File.Exists(fullpth))
                {
                    var file = File.CreateText(fullpth);
                    file.Close();
                }

                StreamWriter sw = new StreamWriter(fullpth);
                sw.WriteLine(resultJSon);
                sw.Flush();

                string fullpth2 = dirPath + "result.csv";
                if (!File.Exists(fullpth2))
                {
                    var file = File.CreateText(fullpth2);
                    file.Close();
                }

                sw = new StreamWriter(fullpth2);
                
                sw.WriteLine("image_path,left,right,top,bottom,topleft,topright,bottomleft,bottomright,leftAreaCount,rightAreaCount,leftPeakForceX,leftPeakForceY,rightPeakForceX, rightPeakForceY, leftArchIndex,rightArchIndex,leftCOPX,leftCOPY,rightCOPX,rightCOPY,COGX,COGY,leftContact,leftMidStance,leftPropulsive,rightContact,rightMidStance,rightPropulsive");
                sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28}",
                    " ",
                    staticJsonData.leftPercent,
                    staticJsonData.rightPercent,
                    staticJsonData.topPercent,
                    staticJsonData.bottomPercent,
                    staticJsonData.topleftPercent,
                    staticJsonData.toprightPercent,
                    staticJsonData.bottomleftPercent,
                    staticJsonData.bottomrightPercent,
                    staticJsonData.leftAreaCount,
                    staticJsonData.rightAreaCount,
                    staticJsonData.leftPeakforceX,
                    staticJsonData.leftPeakforceY,
                    staticJsonData.rightPeakforceX,
                    staticJsonData.rightPeakforceY,
                    staticJsonData.leftArchindex,
                    staticJsonData.rightArchindex,
                    staticJsonData.leftCOPX,
                    staticJsonData.leftCOPY,
                    staticJsonData.rightCOPX,
                    staticJsonData.rightCOPY,
                    staticJsonData.COGX,
                    staticJsonData.COGY,
                    dynamicResultData.leftContactPercent,
                    dynamicResultData.leftMidStancePercent,
                    dynamicResultData.leftPropulsivePercent,
                    dynamicResultData.rightContactPercent,
                    dynamicResultData.rightMidStancePercent,
                    dynamicResultData.rightPropulsivePercent
                    );

                sw.Close();
            }
            exportDataFlag = true;
            StartCoroutine(sendData());
        }
    }

    public IEnumerator sendData()
    {
        WWWForm form = new WWWForm();

        form.AddField("checkdateid", GenerateUID());
        form.AddField("patientid", StaticVaribleHandler.currentPatientCode);
        form.AddField("kinectid", StaticVaribleHandler.currentKinectCode);
        form.AddField("jointdirection", 1600);
        form.AddField("leftPercent", Math.Round(staticJsonData.leftPercent, 2).ToString());
        form.AddField("rightPercent", Math.Round(staticJsonData.rightPercent, 2).ToString());
        form.AddField("topPercent", Math.Round(staticJsonData.topPercent, 2).ToString());
        form.AddField("bottomPercent", Math.Round(staticJsonData.bottomPercent, 2).ToString());
        form.AddField("leftAreaCount", staticJsonData.leftAreaCount.ToString());
        form.AddField("rightAreaCount", staticJsonData.rightAreaCount.ToString());
        form.AddField("leftArchIndex", Math.Round(staticJsonData.leftArchindex, 2).ToString());
        form.AddField("rightArchIndex", Math.Round(staticJsonData.rightArchindex, 2).ToString());
        form.AddField("leftFootLength", staticJsonData.leftFootLength.ToString());
        form.AddField("rightFootLength", staticJsonData.rightFootLength.ToString());
        form.AddField("leftCopX", staticJsonData.leftCOPX.ToString());
        form.AddField("leftCopY", staticJsonData.leftCOPY.ToString());
        form.AddField("rightCopX", staticJsonData.rightCOPX.ToString());
        form.AddField("rightCopY", staticJsonData.rightCOPY.ToString());
        form.AddField("leftPeakForceX", staticJsonData.leftPeakforceX.ToString());
        form.AddField("leftPeakForceY", staticJsonData.leftPeakforceY.ToString());
        form.AddField("rightPeakForceX", staticJsonData.rightPeakforceX.ToString());
        form.AddField("rightPeakForceY", staticJsonData.rightPeakforceY.ToString());
        form.AddField("COGX", staticJsonData.COGX.ToString());
        form.AddField("COGY", staticJsonData.COGY.ToString());

        string copXHistory = "";
        string copYHistory = "";
        string peakForceXHistory = "";
        string peakForceYHistory = "";
        string frameValueSum = "";
        for (int i = 0; i < DynamicHandler.rightIdx; i++)
        {
            copXHistory += DynamicHandler.COPXHistory[i];
            copXHistory += ",";

            copYHistory += DynamicHandler.COPYHistory[i];
            copYHistory += ",";

            peakForceXHistory += DynamicHandler.PeakForceXHistory[i];
            peakForceXHistory += ",";

            peakForceYHistory += DynamicHandler.PeakForceYHistory[i];
            peakForceYHistory += ",";


            byte[] tmpArray = new byte[52 * 44];
            int sum = 0;
            for (int k = 0; k < tmpArray.Length; k++)
            {
                tmpArray[k] = ((byte)(255 - DynamicHandler.DynamicFrameRecordArray[i, k]));
                sum += tmpArray[k];
            }
            frameValueSum += sum;
            frameValueSum += ",";
        }

        copXHistory = copXHistory.Substring(0, copXHistory.Length - 1);
        copYHistory = copYHistory.Substring(0, copYHistory.Length - 1);
        peakForceXHistory = peakForceXHistory.Substring(0, peakForceXHistory.Length - 1);
        peakForceYHistory = peakForceYHistory.Substring(0, peakForceYHistory.Length - 1);
        frameValueSum = frameValueSum.Substring(0, frameValueSum.Length - 1);

        form.AddField("frameNo", DynamicHandler.rightIdx);
        form.AddField("footDirection", DynamicHandler.leftIdx);
        form.AddField("copXHistory", copXHistory);
        form.AddField("copYHistory", copYHistory);
        form.AddField("peakForceXHistory", peakForceXHistory);
        form.AddField("peakForceYHistory", peakForceYHistory);
        form.AddField("frameValueSum", frameValueSum);

        form.AddField("leftContact", Math.Round(dynamicResultData.leftContactPercent,2).ToString());
        form.AddField("leftMidStance", Math.Round(dynamicResultData.leftMidStancePercent, 2).ToString());
        form.AddField("leftPropulsive", Math.Round(dynamicResultData.leftPropulsivePercent, 2).ToString());
        form.AddField("rightContact", Math.Round(dynamicResultData.rightContactPercent, 2).ToString());
        form.AddField("rightMidStance", Math.Round(dynamicResultData.rightMidStancePercent, 2).ToString());
        form.AddField("rightPropulsive", Math.Round(dynamicResultData.rightPropulsivePercent, 2).ToString());

        WWW www = new WWW(StartBalancecheckerHandler.insertBalanceChecker, form);
        yield return www;
        StartCoroutine(deleteData());
    }

    public IEnumerator deleteData()
    {
        WWWForm form = new WWWForm();

        form.AddField("kinectid", StaticVaribleHandler.currentKinectCode);

        WWW www = new WWW(StartBalancecheckerHandler.DeleteKinectSCURL, form);
        yield return www;
        Debug.Log("Program End");

        float time = Time.time;


        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("BalanceCheckerMainScene");
    }

    //pomchecker legacy code
    public static string GenerateUID()
    {
        System.Random rand = new System.Random();
        string input = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var chars = Enumerable.Range(0, 40).Select(x => input[rand.Next(0, input.Length)]);
        return new string(chars.ToArray());
    }

    Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
    {
        int width = original.width;
        int height = original.height;
        Texture2D snap = new Texture2D(width, height);
        Color[] pixels = original.GetPixels();
        Color[] pixelsFlipped = new Color[pixels.Length];

        for (int i = 0; i < height; i++)
        {
            Array.Copy(pixels, i * width, pixelsFlipped, (height - i - 1) * width, width);
        }

        snap.SetPixels(pixelsFlipped);

        return snap;
    }

    Texture2D RotateTexture(Texture2D original, Rotate rotation)
    {
        Color32[] colorSource = original.GetPixels32();
        Color32[] colorResult = new Color32[colorSource.Length];

        int count = 0;
        int newWidth = original.height;
        int newHeight = original.width;
        int index = 0;

        for (int i = 0; i < original.width; i++)
        {
            for (int j = 0; j < original.height; j++)
            {
                if(rotation == Rotate.LEFT)
                {
                    index = (original.width * (original.height - j)) - original.width + i;
                }
                else if(rotation == Rotate.RIGHT)
                {
                    index = (original.width * (j + 1)) - (i + 1);
                }
                colorResult[count] = colorSource[index];
                count++;
            }
        }
        Texture2D snap = new Texture2D(newWidth, newHeight);

        original.Resize(newWidth, newHeight);
        snap.SetPixels32(colorResult);
        snap.Apply();

        colorResult = null;
        return snap;
    }

}

