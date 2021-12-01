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

        public string leftImage;
        public string rightImage;
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
        if(StaticVaribleHandler.isEndDynamic && !exportDataFlag)
        {
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
            Debug.Log(StaticHandler.StaticframeCount);

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

            byte[] imageData = rawImageTexture.EncodeToPNG();
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
                for (int i = 0; i < DynamicHandler.rightIdx; i++)
                {

                    if (i < DynamicHandler.leftIdx)
                    {
                        dynamicJsonData.isLeftRight = "LEFT";
                        dynamicJsonData.frameNo = i;
                    }
                    else
                    {
                        dynamicJsonData.isLeftRight = "RIGHT";
                        dynamicJsonData.frameNo = i - DynamicHandler.leftIdx;
                    }
                    dynamicJsonData.COPX = DynamicHandler.COPXHistory[i];
                    dynamicJsonData.COPY = DynamicHandler.COPYHistory[i];
                    dynamicJsonData.peakForceX = DynamicHandler.PeakForceXHistory[i];
                    dynamicJsonData.peakForceY = DynamicHandler.PeakForceYHistory[i];
                    imageObj = GameObject.Find("Frame_" + i).GetComponent<RawImage>();
                    rawImageTexture = (Texture2D)imageObj.texture;

                    imageData = rawImageTexture.EncodeToPNG();
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


                staticResultObj = GameObject.Find("leftCropImage").GetComponent<Image>();
                rawImageTexture = (Texture2D)staticResultObj.mainTexture;

                imageData = rawImageTexture.EncodeToPNG();
                png = Convert.ToBase64String(imageData);
                dynamicResultData.leftImage = png;


                staticResultObj = GameObject.Find("rightCropImage").GetComponent<Image>();
                rawImageTexture = (Texture2D)staticResultObj.mainTexture;

                imageData = rawImageTexture.EncodeToPNG();
                png = Convert.ToBase64String(imageData);
                dynamicResultData.rightImage = png;

                string dynamicResultjson = JsonUtility.ToJson(dynamicResultData);

                resultJSon += "\"DynamicResult\" : [";
                resultJSon += dynamicResultjson;
                resultJSon += "]}";

                string fullpth = "C:/AutoSet10/public_html/balanceCheckerImage/" + StaticVaribleHandler.currentKinectCode.Substring(0, 7) + ".json";
                Debug.Log(fullpth);
                if (!File.Exists(fullpth))
                {
                    var file = File.CreateText(fullpth);
                    file.Close();
                }

                StreamWriter sw = new StreamWriter(fullpth);
                sw.WriteLine(resultJSon);
                sw.Flush();
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
}

