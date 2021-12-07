using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;

public class StartBalancecheckerHandler : MonoBehaviour
{
    public const string IP_KEYNAME = "127.0.0.1";

    private string UserIP
    {
        get { return PlayerPrefs.GetString(IP_KEYNAME); }
    }
    public static string PatientListURL { get { return $@"http://127.0.0.1/php/rom_server_php/patientlist.php"; } }
    public static string RegisteredListURL { get { return $@"http://127.0.0.1/php/rom_server_php/kinectsclist.php"; } }
    public static string SetMeasuring { get { return $@"http://127.0.0.1/php/rom_server_php/isMeasuring.php"; } }
    public static string insertBalanceChecker { get { return $@"http://127.0.0.1/php/rom_server_php/insertBalanceChecker.php"; } }
    public static string DeleteKinectSCURL { get { return $@"http://127.0.0.1/php/rom_server_php/kinectdelete.php"; } }


    public GameObject canvas;
    public Button staticTestButton;
    public Button dynamicTestButton;
    public Button boardTestButton;
    public Button closeButton;

    public Sprite selectedImage;
    public Sprite normalImage;
    public GameObject lastSelectedGameObject;

    public List<string> patientList;

    void goStaticScene()
    {
        SceneManager.LoadScene("StaticScene");
    }
    void goDynamicScene()
    {
        SceneManager.LoadScene("DynamicScene");
    }
    void goboardTestScene()
    {
        SceneManager.LoadScene("TestBoardScene");
    }
    void closeDevice()
    {
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {
        staticTestButton.onClick.AddListener(goStaticScene);
        dynamicTestButton.onClick.AddListener(goDynamicScene);
        boardTestButton.onClick.AddListener(goboardTestScene);
        //closeButton.onClick.AddListener(closeDevice);
        StartCoroutine(getPatientStatus());

        //Display.displays[0].Activate(1920, 1080, 60);
        //Display.displays[1].Activate(1920, 1080, 60);

    }

    private void Update()
    {
        if((int)Time.time % 5 == 0)
        {
            StartCoroutine(getPatientStatus());
        }
    }
    public GameObject patientButton1;

    public IEnumerator getPatientStatus()
    {
        WWWForm form = new WWWForm();
        WWW www = new WWW(RegisteredListURL, form);

        yield return www;

        if (www.error == null)
        {
            Dictionary<string, string>[] patientInfoMap;
            patientInfoMap = JsonConvert.DeserializeObject<Dictionary<string, string>[]>(www.text);

            for(int i = 0; i<patientInfoMap.Length;i++)
            {
                if(i == 0)
                {
                    patientButton1.gameObject.SetActive(true);
                }
                else if(!patientList.Contains(patientInfoMap[i]["kinectid"]))
                {
                    patientList.Add(patientInfoMap[i]["kinectid"]);
                    Vector3 pos = patientButton1.transform.position;
                    pos.y -= 90;
                    GameObject ins = Instantiate(patientButton1, pos, patientButton1.transform.rotation) as GameObject;
                    ins.name = "patient" + (i + 1) + "Button";
                    ins.transform.SetParent(canvas.transform);
                    //ins.GetComponent<Button>.onClick.AddListener();
                }
                string[] birth = patientInfoMap[i]["birth"].Split('-');
                int year = Int32.Parse(birth[0]);
                int month = Int32.Parse(birth[1]);
                int day = Int32.Parse(birth[2]);

                int currentYear = Int32.Parse(DateTime.Now.ToString("yyyy"));
                int currentMonth = Int32.Parse(DateTime.Now.ToString("MM"));
                int currentDay = Int32.Parse(DateTime.Now.ToString("dd"));

                int age = currentYear - year;

                if(month > currentMonth)
                {
                    age -= 1;
                }
                else if(month == currentMonth)
                {
                    age = currentDay > day ? age -= 1 : age;
                }

                GameObject patient = GameObject.Find("patient" + (i + 1) + "Button");
                Button btn = patient.transform.GetComponent<Button>();
                btn.onClick.AddListener(() => setPatient(patient));
                GameObject.Find(patient.transform.GetChild(0).name).GetComponent<Text>().text = patientInfoMap[i]["name"] + "(만 " + age + "세)";
                GameObject.Find(patient.transform.GetChild(1).name).GetComponent<Text>().text = "족저압 검사 (정적 동적)";
                patient.transform.GetChild(0).name = patientInfoMap[i]["name"];
                patient.transform.GetChild(2).name = patientInfoMap[i]["kinectid"];
                patient.transform.GetChild(3).name = patientInfoMap[i]["patientid"];
            }
            //Debug.Log(patientInfoMap.Length);
            //Debug.Log(patientInfoMap[0]["patientid"]);
            //Debug.Log(patientInfoMap[0]["name"]);
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    void setPatient(GameObject patient)
    {
        Button btn = lastSelectedGameObject.GetComponent<Button>();
        btn.image.sprite = normalImage;

        btn = patient.GetComponent<Button>();
        btn.image.sprite = selectedImage;
        lastSelectedGameObject = patient;

        StaticVaribleHandler.currentPatient = GameObject.Find(patient.transform.GetChild(0).name).GetComponent<Text>().text;
        StaticVaribleHandler.currentKinectCode = patient.transform.GetChild(2).name;
        StaticVaribleHandler.currentPatientCode = patient.transform.GetChild(3).name;
    }
}
