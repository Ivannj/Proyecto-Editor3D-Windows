using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using System.Security.Cryptography;
using System.IO;

public class DBManager : MonoBehaviour
{

    private string loginURL = "http://localhost/editor3d/login.php";
    private string loadURL = "http://localhost/editor3d/load.php";
    private string secretKey = "Pomelo";

    public InputField user, pass;
    public Text alertInfo;

    public List<string> users;
    public GameObject loadCanvas;
    public GameObject basebtn;
    public GameObject tik;
    public Transform gridUsers, gridProjects;

    public void Login()
    {
        StartCoroutine(setLogin());
    }

    IEnumerator setLogin()
    {
        WWWForm form = new WWWForm();
        form.AddField("user", user.text);
        form.AddField("pass", Md5Sum(pass.text + secretKey));

        UnityWebRequest request = UnityWebRequest.Post(loginURL, form);
        yield return request.SendWebRequest();

        string finalText = request.downloadHandler.text;
        int adminValue = -1;
        if (int.TryParse(finalText, out adminValue))
        {
            AppManager.isAdmin = (adminValue == 1);
            AppManager.userName = user.text;
            tik.SetActive(true);
            Invoke("CargarEscena", 1.5f);

        }
        else
        {
            alertInfo.text = "Contraseña incorrecta";
            Invoke("ClearAlert", 2);
        }
    }
    void ClearAlert()
    {
        alertInfo.text = "";
    }
    void CargarEscena()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public void QuitApp()
    {
        Application.Quit();
    }

    public void LoadUsers()
    {
        users = new List<string>();
        loadCanvas.SetActive(true);
        StartCoroutine(Loading());
    }
    IEnumerator Loading()
    {
        UnityWebRequest request = UnityWebRequest.Get(loadURL);
        yield return request.SendWebRequest();

        SplitUsers(request.downloadHandler.text);

    }

    void SplitUsers(string _users)
    {
        string[] allUSers = _users.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
        for (int i = 0; i < allUSers.Length; i++)
        {
            users.Add(allUSers[i]);
        }
        PrintUsers();
    }

    void PrintUsers(string _username = "")
    {
        for (int i = gridUsers.childCount - 1; i >= 0; i--)
        {
            Destroy(gridUsers.GetChild(i).gameObject);
        }

        for (int i = 0; i < users.Count; i++)
        {
            GameObject newUser = Instantiate(basebtn, gridUsers);
            newUser.transform.GetChild(0).GetComponent<Text>().text = users[i];

            Color colorBtn = (users[i] == _username) ? Color.green : Color.white;
            newUser.GetComponent<Image>().color = colorBtn;

            string name = users[i];
            Button tempBtn = newUser.GetComponent<Button>();
            tempBtn.onClick.AddListener(delegate { PrintUsers(name); });
            tempBtn.onClick.AddListener(delegate { PrintProjects(name); });
            
        }
    }


    void PrintProjects(string username, string _projectName = "")
    {
        for (int i = gridProjects.childCount - 1; i >= 0; i--)
        {
            Destroy(gridProjects.GetChild(i).gameObject);
        }
        string path = "C:/Xammp2/htdocs/editor3d/" + username + "/";
        if (Directory.Exists(path))
        {
            string[] allFIles = Directory.GetFiles(path);
            for (int i = 0; i < allFIles.Length; i++)
            {

                string finalName = SeparateFileName(allFIles[i]);

                GameObject newProject = Instantiate(basebtn, gridProjects);
                newProject.transform.GetChild(0).GetComponent<Text>().text = finalName;

                Color colorBtn = (finalName == _projectName) ? Color.green : Color.white;
                newProject.GetComponent<Image>().color = colorBtn;

                string projectName = finalName;
                Button tempBtn = newProject.GetComponent<Button>();
                tempBtn.onClick.AddListener(delegate { PrintProjects(username, projectName); });
                tempBtn.onClick.AddListener(delegate { LoadProject(username, projectName); });

            }
        }
    }
    void LoadProject(string username, string _projectName)
    {
        GetComponent<AppManager>().LoadProject(username, _projectName);
    }
    string SeparateFileName(string _name)
    {
        string[] separate = _name.Split(new string[] { "/", "." }, System.StringSplitOptions.None);
        return separate[separate.Length - 2];
    }

    public string Md5Sum(string strToEncrypt)
    {

        UTF8Encoding ue = new UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }
        return hashString.PadLeft(32, '0');

    }
}