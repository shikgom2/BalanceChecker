using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class StaticButtonHandler : MonoBehaviour {

    #region dllimport
    //get Device idx
    [DllImport("Multi-TeeTester-API")]
    private static extern uint GetDevicesNum();

    //Open Device
    [DllImport("Multi-TeeTester-API")]
    private static extern bool Open(uint deDeviceIndex);

    //set sensivity
    [DllImport("Multi-TeeTester-API")]
    private static extern bool SetPowA(byte bPowA);

    //Close Device 
    [DllImport("Multi-TeeTester-API")]
    private static extern void Close();
    #endregion

    #region public variables
    public enum Extension { PNG, JPG }
    public enum Mode { Screenshot, Snapshot, Debug }
    public Button startButton;
    public Button staticButton;
    public Button dynamicButton;
    public Button resetButton;
    public Button mainButton;
    public Button captureButton;
    public Button closeButton;

    public Text waitingText;
    public Text patientStatusText;

    public GameObject moniter2;
    public GameObject moniter2Notice;
    public GameObject loadingShape;
    public GameObject programLogo;
    public GameObject checkImage;

    public Sprite button1Disable;
    public Sprite button2Disable;
    public Sprite button1Active;
    public Sprite button2Active;

    public bool isTimeover = false;
    /* Inspector Elements */
    public Mode mode = Mode.Screenshot;
    public KeyCode captureKey;
    public Extension extension = Extension.PNG;
    public int quality = 75; //JPG property
    public int frame = 3; //Only in sanpshot
    public float time = 5; //Only in sanpshot
    public string textLog; //Only in Debug
    public int textSaveOption = 2; //0: Don't Save, 1: Capture Start, 2: Capture Over (Only in  Debug)
    public Text textLog_Text; //Only in Debug
    public bool textClear = true; //Only in Debug
    public bool activePause = true; //Onlt in Debug - When capture is start Save Option
    public string savePath;
    public bool isRecording = false; //Snapshot
    public bool isDebuging = false; //Debug
    public static byte sensivity = 40;

    [HideInInspector] public bool inPlayMode = false;
    private bool isCapturing = false;
    private float lastTimeScale;
    private int recordNumber = 0;

    #endregion
    private void Awake () {
        inPlayMode = true;
        StaticVaribleHandler.isEndStatic = false;
        StaticHandler.isStart = false;
        StaticHandler.isWaiting = false;
        staticButton.GetComponent<Image>().sprite = button1Active;
        GameObject.Find("btn1").GetComponent<Image>().sprite = button1Active;
        dynamicButton.onClick.AddListener(switchDynamicScene);
        mainButton.onClick.AddListener(goMain);
        startButton.onClick.AddListener(startDevice);
        closeButton.onClick.AddListener(closeDevice);
        captureButton = GameObject.Find("captureButton").GetComponent<Button>();
        captureButton.onClick.AddListener(Capture); //Capture

        if(StaticVaribleHandler.currentPatient == "")
        {
            //DEMO SETTING
            StaticVaribleHandler.currentPatient = "데모용 (만 21세)";
            StaticVaribleHandler.currentKinectCode = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StaticVaribleHandler.currentPatientCode = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        }
        GameObject.Find("currentName").GetComponent<Text>().text = StaticVaribleHandler.currentPatient;
        GameObject.Find("currentDate").GetComponent<Text>().text = DateTime.Now.ToString("yyyy-MM-dd");
        string[] name = StaticVaribleHandler.currentPatient.Split('(');

        patientStatusText.text = "<b>" + name[0] + "</b>님, 안녕하세요.\n\n족저압 검사를 시작합니다.\n예상 소요시간은 <b>약 1분 30초</b> 입니다.";

        waitingText.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);
        SliderHandler(25);

        //init dll
        uint ConnectIdx = 0;
        ConnectIdx = GetDevicesNum();
        Open(ConnectIdx);
        SetPowA(sensivity);    //set sensivity
        StaticHandler.StaticframeCount = 0;

        StartCoroutine(SetCurrentMeasuring(true));

    }

    public Image Display2;
    public Image Display2Blank;

    float z = 0;
    private void Update () {
        if (isTimeover)
        {
            waitingText.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);
            countdownText2.gameObject.SetActive(false);
            StaticHandler.isStart = true;
            measuringText.gameObject.SetActive(true);
            loadingShape.gameObject.SetActive(true);
        }
        //loading
        if(loadingShape.activeInHierarchy)
        {
            z += Time.deltaTime * 400f;
            var rot = loadingShape.transform.rotation;
            rot.eulerAngles = new Vector3(0, 0, z);
            loadingShape.transform.rotation = rot;
        }

        if (StaticHandler.StaticframeCount == StaticVaribleHandler.MaximumStaticFrameCount)
        {
            waitingText.gameObject.SetActive(true);
            endText.gameObject.SetActive(true);
            measuringText.gameObject.SetActive(false);
            checkImage.SetActive(true);
            loadingShape.gameObject.SetActive(false);
            programLogo.SetActive(false);
            waitingText.text = "측정이 완료되었습니다!";
            staticButton.GetComponent<Image>().sprite = button1Disable;
            GameObject.Find("btn1").GetComponent<Image>().sprite = button1Disable;
            StaticVaribleHandler.isEndStatic = true;
            Close();
            //StartCoroutine("goDynamicScene");
        }
        //EventHandler();
    }


    IEnumerator goDynamicScene()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("DynamicScene");
    }
    void closeDevice()
    {
        Application.Quit();
    }
    void goImport()
    {
        SceneManager.LoadScene("StaticCSVScene");
    }
    void startDevice()
    {
        startButton.gameObject.SetActive(false);

        //init dll
        uint ConnectIdx = 0;
        ConnectIdx = GetDevicesNum();
        Open(ConnectIdx);
        SetPowA(sensivity);    //set sensivity
        StaticHandler.StaticframeCount = 0;

        StaticHandler.isWaiting = true;
        waitingText.gameObject.SetActive(true);
        Display2Blank.gameObject.SetActive(false);

        StartCoroutine("waiting");
    }
    void resetData()
    {
        startButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(false);
        StaticHandler.StaticframeCount = 0;
        StaticHandler.StaticFrameRecordArray.Initialize();
        StaticHandler.isStart = false;
        StaticHandler.isWaiting = false;
        
        waitingText.gameObject.SetActive(true);
        moniter2.SetActive(true);
        moniter2Notice.SetActive(false);

        endText.gameObject.SetActive(false);
        measuringText.gameObject.SetActive(false);
        loadingShape.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        Close();
        //StartCoroutine("waiting");
    }
    void goMain()
    {
        SceneManager.LoadScene("BalanceCheckerMainScene");
    }
    void DeviceHandler()
    {

    }
    void SliderHandler(float value)
    {
        sensivity = (byte)Math.Ceiling(value);
        SetPowA(sensivity);
    }

    void switchDynamicScene()
    {
        SceneManager.LoadScene("DynamicScene");
    }

    private void EventHandler () { //Key press detect method
        if (Input.GetKeyDown (captureKey)) StartCoroutine ("Rendering");
        if (textLog_Text != null) textLog_Text.text = textLog;
        if (textSaveOption != 1) activePause = true;
    }

    public Text countdownText;
    public Text countdownText2;
    public Text endText;
    public Text measuringText;
    IEnumerator waiting()
    {

        patientStatusText.text = "잠시 후 <b>정적 족저압 측정</b>이 시작됩니다.";
        yield return new WaitForSeconds(5);
        patientStatusText.gameObject.SetActive(false);
        programLogo.SetActive(false);
        moniter2Notice.SetActive(true);

        yield return new WaitForSeconds(5);
        //moniter2.SetActive(true);
        moniter2Notice.SetActive(false);
        waitingText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        countdownText2.gameObject.SetActive(true);
        Display2.gameObject.SetActive(false);
        Display2Blank.gameObject.SetActive(true);

        float currentTime = 0f;
        while (true)
        {
            if (currentTime >= 5f)
            {
                isTimeover = true;
                yield break;
            }
            else {
                currentTime += Time.deltaTime;
                float t = 6 - currentTime;
                if(t > 1f)
                {
                    countdownText.text = (6 - currentTime).ToString("N1");
                    countdownText2.text = (6 - currentTime).ToString("N1");
                }
            }
            yield return null;
        }
    }

    // 현재 측정중인지 여부를 웹에 띄워주는 부분
    public IEnumerator SetCurrentMeasuring(bool isMeasuring)
    {
        WWWForm form = new WWWForm();

        if (isMeasuring == true)
        {
            form.AddField("MeasuringTrue", 0);
        }
        else
        {
            form.AddField("MeasuringFalse", 0);
        }
        WWW www = new WWW(StartBalancecheckerHandler.SetMeasuring, form);
        yield return www;
        Debug.Log("isMeasuring 세팅 결과: " + www.text);
    }
    IEnumerator Rendering () {
        if (!isCapturing && !isRecording && !isDebuging) {
            isCapturing = true; //Capture start

            if (savePath != null && !savePath.Equals ("")) {

                string fileExt = GetExtension ();
                string date = GetDate ();

                switch (mode) {
                    /*
                    case Mode.Screenshot:
                        yield return new WaitForEndOfFrame (); //Capture must be taken when end of frame
                        string path = savePath + @"\" + date + fileExt;
                        Capture (Screen.width, Screen.height, path);
                        Debug.Log (path + " has been saved"); //Print Log
                        isCapturing = false; //End of capture
                        break;
                    */
                    case Mode.Snapshot:
                        string dirPath = savePath + @"\" + date;
                        Directory.CreateDirectory (dirPath);

                        Debug.Log ("Record Start");
                        isRecording = true;
                        int getRecordNumber = recordNumber; //Prevent overlap stop record
                        StartCoroutine (StopRecording (getRecordNumber));

                        int seqNum = 1; //Image sequence number
                        float delay = (float)1 / frame; //Frame delay
                        while (isRecording) {
                            yield return new WaitForEndOfFrame ();
                            //Capture (Screen.width, Screen.height, dirPath + @"\sequence" + seqNum + fileExt , dirPath);
                            seqNum += 1;
                            yield return new WaitForSecondsRealtime (delay);
                        }  break; //End of capture sequence

                    case Mode.Debug:
                        if (activePause) { //When Pause is active
                            isDebuging = true;
                            lastTimeScale = Time.timeScale; //Save las time scale
                            Time.timeScale = 0; //Game pause
                        }

                        if (textSaveOption == 1) { //When capture is start
                            yield return new WaitForEndOfFrame ();
                            SaveImageWithText (true);
                            if (!activePause) isCapturing = false; //Capture over when pause is deactive
                        } break;
                }

            } else Debug.LogError ("Can not found save path.");
        } else if (isCapturing && isRecording) {
            isRecording = false;
            isCapturing = false;
            Debug.Log ("Recording is canceled"); //Record cancel
            recordNumber += 1; //Prevent overlap stop record

        } else if (isCapturing && isDebuging) {
            if (textSaveOption == 2 || textSaveOption == 0) { //When capture is over
                yield return new WaitForEndOfFrame ();
                if (textSaveOption == 2) SaveImageWithText (true);
                else SaveImageWithText (false);
            }

            isDebuging = false;
            isCapturing = false;
            Time.timeScale = lastTimeScale; //Game resume
        }
    }

    IEnumerator StopRecording (int localRecordNumber) {
        yield return new WaitForSecondsRealtime (time);
        if (localRecordNumber == recordNumber) { //Compare record number
            isRecording = false;
            isCapturing = false; //End of capture
            Debug.Log ("Recording is complete");
            recordNumber += 1;
        }
    }

    public void Capture () {
        int width = Screen.width;
        int height = Screen.height;
        string path = "./";
        byte[] imgBytes = null;

        Texture2D texture = new Texture2D (width - 880, height - 200, TextureFormat.RGB24, false);
        texture.ReadPixels (new Rect (880, 200, width - 880, height - 200), 0, 0, false);
        texture.Apply (); //Make sreen texture

        /*
        using (var writer = new CsvFileWriter(dirPath + "/test.csv"))
        {
            List<string> columns = new List<string>();

            int cnt = 0;
            for(int i = 0; i< StaticHandler.HEIGHT; i++)
            {
                for(int j = 0; j< StaticHandler.WIDTH; j++)
                {
                    columns.Add(StaticHandler.tmpImageBuffer[cnt].ToString());
                    cnt++;
                }
                writer.WriteRow(columns);
                columns.Clear();
            }
        }
        */
        switch (extension) {
            case Extension.PNG: imgBytes = texture.EncodeToPNG (); break;
            case Extension.JPG: imgBytes = texture.EncodeToJPG (quality); break;
        } //Encoding file extension

        File.WriteAllBytes (path, imgBytes); //Write image file
    }

    public string GetTextName () { //For debug mode
        string line = textLog.Split ('\n')[0].Trim (); //line change detect

        int subStringLastIndex;
        if (line.Length > 40) subStringLastIndex = 40;
        else subStringLastIndex = line.Length; //Name words limit

        return line.Substring (0, subStringLastIndex);
    }

    public void SaveTextLog (string name, bool printLog) { //For debug mode
        if (textLog != null && !textLog.Equals ("")) { //Check contents blank

            string path = savePath + @"\" + name + ".txt"; //Set path
            File.WriteAllText (path, textLog); //Write text file

            if (textClear) textLog = ""; //Clear text area
            if (printLog) Debug.Log (path + " has been saved"); //Print log if called by pressing button
        } 
    }

    private void SaveImageWithText (bool saveText) { //For Save Text Option

        string fileExt = GetExtension ();
        bool isTextSaved = true; //For log

        string name = GetTextName ();
        if (name == null || name.Equals ("")) {
            name = GetDate ();
            isTextSaved = false;
        } //Text area blank check

        string imagePath = savePath + @"\" + name + fileExt; //Set path
        //Capture (Screen.width, Screen.height, imagePath, savePath); //Capture
        if (saveText) SaveTextLog (name, false); //Save text file

        if (textLog_Text != null && textClear && textSaveOption == 0) textLog = "";
        //Clear text area if there's Text object, Text Clear is true, Save Option is 'Don't save' 

        if (isTextSaved && saveText) Debug.Log (savePath + @"\" + name + " " + fileExt + " and .txt has been saved"); //Print Log
        else Debug.Log (imagePath + " has been saved"); //When text is not saved
    }

    //Get file extension name
    public string GetExtension () {
        switch (extension) {
            case Extension.PNG: return ".png";
            case Extension.JPG: return ".jpg";
            default: return ".png";
        }
    }

    //Get date for name
    public string GetDate () {
        DateTime dt = DateTime.Now;
        return String.Format ("{0:yyyy/MM/dd} {0:HH_mm_ss}", dt, dt);
    }
}
