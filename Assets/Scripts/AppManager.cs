using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Xml;
using System.Xml.Serialization;


public enum ObjectType { None, Wall, Floor, Asset }
public class AppManager : MonoBehaviour
{

    static public bool isAdmin;
    static public string userName;
    public GameObject loadBtn;


    //Creamos los objetos y/o botones dentro del grid 
    public GameObject objectBtn, snap, floorGizmo;
    public List<GameObject> floorVertex;
    public Transform grid;
    private ObjectInfo[] allProps;
    public List<ObjectControl> savedObjects;
    private ObjectInfo currentOnject;
    private ObjectType currenType;
    private GameObject currentPrefab, currentSelection;
    private bool wallMinimized;
    public InputField selectionField;
    private ObjectInfo selection;
    private Quaternion saveRotation;

    private RaycastHit hitGround;
    private Ray rayGround;
    private Vector3 endSnap;

    private float vRight, vLeft, vUp, vDown, floorWidth, floorHeight;

    void Start()
    {
       loadBtn.SetActive(isAdmin);
        //Rellenamos el array de AllProps con los objetos que tenemos en la carpeta Resources/Objects
        allProps = Resources.LoadAll<ObjectInfo>("Objects");

        PrintObjects();
    }
    //Imprimimos los objetos fisicos o botones fisicos 
    private void PrintObjects(int _id = 0)
    {
        saveRotation = Quaternion.Euler(0, 0, 0);
        selection = null;
        currentPrefab = null;
        for (int i = floorVertex.Count - 1; i >= 0; i--)
        {
            Destroy(floorVertex[i]);
        }
        floorVertex = new List<GameObject>();


        //Eliminamos todos los objetos que haya en el grid con un for reversivo
        for (int i = grid.childCount - 1; i >= 0; i--)
        {
            Destroy(grid.GetChild(i).gameObject);
        }
        //Instanciamos los botones(Objetos) que esten ya creados en el grid.
        for (int i = 0; i < allProps.Length; i++)
        {
            GameObject newBtn = Instantiate(objectBtn, grid);
            newBtn.transform.GetChild(0).GetComponent<Text>().text = allProps[i].name;
            Color colorBtn = (allProps[i].id == _id) ? Color.grey : Color.white;
            newBtn.GetComponent<Image>().color = colorBtn;

            int currentId = allProps[i].id;
            newBtn.GetComponent<Button>().onClick.AddListener(delegate { PrintObjects(currentId); });

            if (allProps[i].id == _id)
            {
                currenType = allProps[i].type;
                currentOnject = allProps[i];
            }
        }
            if(_id == 0)
            {
                currenType = ObjectType.None;
            }
       
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(currentPrefab);
            PrintObjects();
        }

        switch (currenType)
        {
            case ObjectType.None:
                UpdateNone();
                break;
            case ObjectType.Wall:
                updateWall();
                break;
            case ObjectType.Floor:
                updateFloor();
                break;
            case ObjectType.Asset:
                UpdateAsset();
                break;
            
        }
    }

    void UpdateAsset()
    {
        if(currentPrefab == null)
        {
            currentPrefab = Instantiate(currentOnject.prefab, Vector3.zero, saveRotation);

        }
        else
        {
            rayGround = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayGround, out hitGround, 50))
            {
                if (Input.GetMouseButton(0))
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        currentPrefab.transform.position = hitGround.point;
                    }
                    else
                    {
                        Vector3 lookPoint = hitGround.point;
                        lookPoint.y = currentPrefab.transform.position.y;
                        currentPrefab.transform.LookAt(lookPoint);
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    ObjectControl newObj = currentPrefab.AddComponent<ObjectControl>();
                    newObj.id = currentOnject.id;
                    savedObjects.Add(newObj);

                    currentPrefab.GetComponent<Collider>().enabled = true;
                    currentPrefab = null;
                }
                else
                {
                    currentPrefab.transform.position = hitGround.point;
                }
            }
            if(Input.GetMouseButtonDown(1))
            {
                Destroy(currentPrefab);
                PrintObjects();
            }
        }
    }
    public void MoveObject()
    {
        if(selection.type == ObjectType.Asset)
        {
            saveRotation = currentSelection.transform.rotation;
            savedObjects.Remove(currentSelection.GetComponent<ObjectControl>());
            Destroy(currentSelection);
            currenType = ObjectType.Asset;
        }
    }
    void UpdateNone()
    {
        if (Input.GetMouseButtonDown(0))
        {
            rayGround = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayGround, out hitGround, 50, (1 << 0)))
            {
                currentSelection = hitGround.collider.gameObject;
                int id = currentSelection.GetComponent<ObjectControl>().id;
                selection = GetObjectByID(id);
                selectionField.text = selection.name;
            }
        }
    }
    ObjectInfo GetObjectByID(int id)
    {
        for (int i = 0; i < allProps.Length; i++)
        {
            if(allProps[i].id == id)
            {
                return allProps[i];
            }
        }
        return null;
    }

    void updateFloor()
    {
     
        if ( Input.GetMouseButtonDown(0))
        {
            rayGround = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(rayGround, out hitGround, 50, (1<<6) | (1<<7)))
            {
                Vector3 finalPosition = hitGround.point;
                if (hitGround.collider.tag == "Snap") finalPosition = hitGround.collider.transform.position;
                GameObject newGizmo = Instantiate(floorGizmo, finalPosition, Quaternion.identity);
                floorVertex.Add(newGizmo);
                bool tempCheck = CheckVertexDistance(newGizmo);
                if (tempCheck)
                {
                    floorVertex.RemoveAt(floorVertex.Count - 1);
                    Destroy(newGizmo);
                    GenerateFloorGeometry();
                }
            }
        }
        if(Input.GetMouseButtonDown(1) && floorVertex.Count >= 3)
        {
            
            GenerateFloorGeometry();
        }

    }
    void GenerateFloorGeometry()
    {
        Vector3 center = GetCenterGeometry();
        GameObject newFloor = new GameObject("Floor Mesh");
        MeshFilter filter = newFloor.AddComponent<MeshFilter>();
        MeshRenderer renderer = newFloor.AddComponent<MeshRenderer>();
        renderer.material = Resources.Load<Material>("Floor");
        renderer.material.mainTextureScale = new Vector2(floorWidth, floorHeight);

        Mesh mesh = new Mesh();
        List<Vector3> vertex = new List<Vector3>();
        for (int i = 0; i < floorVertex.Count; i++)
        {
            vertex.Add(floorVertex[i].transform.position);
        }
        vertex.Add(center);
        int lastVertex = vertex.Count - 1;
        mesh.vertices = vertex.ToArray();

        List<int> tris = new List<int>();
        for (int i = 0; i < floorVertex.Count; i++)
        {
            int currentVertex = i;
            int nextVertex = i + 1;
            if (nextVertex >= floorVertex.Count) nextVertex = 0;

            tris.Add(lastVertex);
            tris.Add(currentVertex);
            tris.Add(nextVertex);
        }

        mesh.triangles = tris.ToArray();

        List<Vector3> normals = new List<Vector3>();
        for (int i = 0; i < vertex.Count; i++)
        {
            normals.Add(Vector3.up);
        }
        mesh.normals = normals.ToArray();

        List<Vector2> uv = new List<Vector2>();
        for (int i = 0; i < vertex.Count; i++)
        {
            Vector2 newCoord = getCoord(vertex[i]);
            uv.Add(newCoord);
        }

        mesh.uv = uv.ToArray();

        mesh.RecalculateNormals();
        if (mesh.normals[0].y < 0)
        {
            tris.Reverse();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
        }
        filter.mesh = mesh;
        newFloor.AddComponent<MeshCollider>();

        for (int i = floorVertex.Count - 1; i >= 0; i--)
        {
            Destroy(floorVertex[i]);
        }
        floorVertex = new List<GameObject>();

        ObjectControl newObj = newFloor.AddComponent<ObjectControl>();
        newObj.id = 2;
        vertex.RemoveAt(vertex.Count - 1);
        newObj.vertices = vertex;
        savedObjects.Add(newObj);
        newFloor.AddComponent<DeleteControl>();
    }
    Vector2 getCoord(Vector3 pos)
        {
            float distanceX = vRight - pos.x;
            float percent = distanceX / floorWidth;

            float distanceZ = vUp - pos.z;
            float percentZ = distanceZ / floorHeight;

            return new Vector2(percent, percentZ);
        }
    Vector3 GetCenterGeometry()
        {
            vRight = -Mathf.Infinity;
            for (int i = 0; i < floorVertex.Count; i++)
            {
                if (floorVertex[i].transform.position.x > vRight)
                {
                    vRight = floorVertex[i].transform.position.x;
                }
            }
            vLeft = Mathf.Infinity;
            for (int i = 0; i < floorVertex.Count; i++)
            {
                if (floorVertex[i].transform.position.x < vLeft)
                {
                    vLeft = floorVertex[i].transform.position.x;
                }
            }
            vUp = -Mathf.Infinity;
            for (int i = 0; i < floorVertex.Count; i++)
            {
                if (floorVertex[i].transform.position.z > vUp)
                {
                    vUp = floorVertex[i].transform.position.z;
                }
            }
            vDown = Mathf.Infinity;
            for (int i = 0; i < floorVertex.Count; i++)
            {
                if (floorVertex[i].transform.position.z < vDown)
                {
                    vDown = floorVertex[i].transform.position.z;
                }
            }

            float centerX = vLeft + ((vRight - vLeft) / 2);
            float centerZ = vDown + ((vUp - vDown) / 2);
            Vector3 center = new Vector3(centerX, 0, centerZ);

            floorWidth = vRight - vLeft;
            floorHeight = vUp - vDown;

            return center;
        }
    bool CheckVertexDistance(GameObject vertex)
        {
            for (int i = 0; i < floorVertex.Count; i++)
            {
                if (floorVertex[i] != vertex)
                {
                    float tempDistance = Vector3.Distance(vertex.transform.position, floorVertex[i].transform.position);

                    if (tempDistance <= 0.2f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    void updateWall()
        {
            if (currentPrefab == null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    rayGround = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(rayGround, out hitGround, 50, (1 << 6) | (1 << 7)))
                    {
                        endSnap = hitGround.point;
                        if (hitGround.collider.tag == "Snap") endSnap = hitGround.collider.transform.position;
                        currentPrefab = Instantiate(currentOnject.prefab, endSnap, Quaternion.identity);
                        currentPrefab.transform.localScale = new Vector3(1, wallMinimized ? 0.1f : 1f, 1);
                        currentPrefab.SetActive(false);
                    }
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    rayGround = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(rayGround, out hitGround, 50, (1 << 6) | (1 << 7)))
                    {
                        endSnap = hitGround.point;
                        if (hitGround.collider.tag == "Snap") endSnap = hitGround.collider.transform.position;

                        currentPrefab.transform.LookAt(endSnap);
                        float distance = Vector3.Distance(currentPrefab.transform.position, endSnap);
                        currentPrefab.transform.localScale = new Vector3(1, wallMinimized ? 0.1f : 1f, distance);
                        currentPrefab.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(distance, 1f);
                        if(distance > 0.2f && currentPrefab.activeSelf == false)
                    {
                        currentPrefab.SetActive(true);

                    }
                       
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                //Imprimir SNap
                if (currentPrefab.transform.localScale.z < 0.5)
                {
                    Destroy(currentPrefab);
                }
                else
                {
                    GameObject initSnap = Instantiate(snap, currentPrefab.transform.position, Quaternion.identity);
                    GameObject finalSnap = Instantiate(snap, endSnap, Quaternion.identity);
                    currentPrefab.GetComponent<SnapControl>().SetSnap(new GameObject[] { initSnap, finalSnap });

                    ObjectControl tempObj = currentPrefab.AddComponent<ObjectControl>();
                    tempObj.id = currentOnject.id;
                    savedObjects.Add(tempObj);
                }
                    currentPrefab = null;
                }
            }
        }
    public void MinimizeWall(bool minimize)
        {
            wallMinimized = minimize;
            for (int i = 0; i < savedObjects.Count; i++)
            {
                if (savedObjects[i].id == 1)
                {
                    Vector3 currentSize = savedObjects[i].transform.localScale;
                    savedObjects[i].transform.localScale = new Vector3(currentSize.x, minimize ? 0.1f : 1f, currentSize.z);
                }
            }
        }
    public void DeleteObject()
     {
        if (currentSelection != null)
        {
            savedObjects.Remove(currentSelection.GetComponent<ObjectControl>());
            currentSelection.GetComponent<DeleteControl>().DeleteObjects();
            selectionField.text = "";
            selection = null;
        }
     }

    public void ClearScene()
    {
        for (int i = 0; i < savedObjects.Count; i++)
        {
            savedObjects[i].GetComponent<DeleteControl>().DeleteObjects();
        }
        savedObjects = new List<ObjectControl>();
    }

    public void LoadProject(string username, string projectName)
    {
        ClearScene();

        XmlSerializer serializer = new XmlSerializer(typeof(SaveProp));
        string filePath = "C:/Xammp2/htdocs/editor3d/" + username + "/" + projectName + ".campusnet";
        string readFile = File.ReadAllText(filePath);
        if(readFile.Length > 0)
        {
            using (StringReader sr = new StringReader(readFile))
            {
                SaveProp newLoad = serializer.Deserialize(sr) as SaveProp;
                LoadObjects(newLoad);
            }
        }
    }
    void LoadObjects(SaveProp _objects)
    {
        for (int i = 0; i < _objects.allObjects.Count; i++)
        {
            ObjectInfo newObj = GetObjectByID(_objects.allObjects[i].id);
            switch (newObj.type)
            {
                case ObjectType.Wall:
                    LoadWall(_objects.allObjects[i], newObj);
                    break;
                case ObjectType.Floor:
                    LoadFloor(_objects.allObjects[i], newObj);
                    break;
                case ObjectType.Asset:
                    LoadAsset(_objects.allObjects[i], newObj);
                    break;
            }
        }
    }

    void LoadWall(SaveObjectsInfo _wall, ObjectInfo _obj)
    {
        GameObject newWall = Instantiate(_obj.prefab, _wall.position, _wall.rotation);
        newWall.transform.localScale = _wall.scale;
        newWall.transform.GetChild(0).GetComponent<Renderer>().material.mainTextureScale = new Vector2(_wall.scale.z, 2.6f);

        ObjectControl tempObj = newWall.AddComponent<ObjectControl>();
        tempObj.id = _obj.id;
        savedObjects.Add(tempObj);
    }
    void LoadFloor(SaveObjectsInfo _floor, ObjectInfo _obj)
    {
        for (int i = 0; i < _floor.vertices.Count; i++)
        {
            GameObject newvertex = new GameObject("");
            newvertex.transform.position = _floor.vertices[i];
            floorVertex.Add(newvertex);
        }
        GenerateFloorGeometry();
    }
    void LoadAsset(SaveObjectsInfo _asset, ObjectInfo _obj)
    {
        GameObject newAsset = Instantiate(_obj.prefab, _asset.position, _asset.rotation);
        newAsset.GetComponent<Collider>().enabled = true;
        ObjectControl tempObj = newAsset.AddComponent<ObjectControl>();
        tempObj.id = _obj.id;
        savedObjects.Add(tempObj);
    }

    public void SaveProject()
    {
        SaveProp newSave = new SaveProp();
        newSave.allObjects = new List<SaveObjectsInfo>();

        for (int i = 0; i < savedObjects.Count; i++)
        {
            SaveObjectsInfo newObj = new SaveObjectsInfo()
            {
                id = savedObjects[i].id,
                position = savedObjects[i].transform.position,
                rotation = savedObjects[i].transform.rotation,
                scale = savedObjects[i].transform.localScale,
                vertices = savedObjects[i].vertices,
            };
            newSave.allObjects.Add(newObj);
        }

        XmlSerializer serializer = new XmlSerializer(typeof(SaveProp));

        using(StringWriter sw= new StringWriter())
        {
            serializer.Serialize(sw, newSave);
            string path = "C:/Xammp2/htdocs/editor3d/" + userName + "/";
            if(Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            int fileCount = Directory.GetFiles(path).Length;
            File.WriteAllText("C:/Xammp2/htdocs/editor3d/" + userName + "/editor_"+ fileCount.ToString("000") + ".campusnet", sw.ToString());
            Application.OpenURL("C:/Xammp2/htdocs/editor3d/"+ userName +"/");
           // print(sw.ToString());
        }
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}

public class SaveProp
{
    public List<SaveObjectsInfo> allObjects;
}

public class SaveObjectsInfo
{

    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public List<Vector3> vertices;

    public SaveObjectsInfo() { }

    public SaveObjectsInfo(int _id, Vector3 _pos, Quaternion _rot, Vector3 _scale, List<Vector3> _vertex)
    {
        id = _id;
        position = _pos;
        rotation = _rot;
        scale = _scale;
        vertices = _vertex;
    }


}
