using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectControl : MonoBehaviour
{

    [SerializeField]
    private int idObject;

    public int id
    {
        get { return idObject; }
        set { idObject = value; }
    }


    private List<Vector3> meshVertex;
    public List<Vector3> vertices
    {
        get { return meshVertex; }
        set { meshVertex = value; }
    }

}
