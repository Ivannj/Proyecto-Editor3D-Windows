using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="Object", menuName ="Objects/Create new Object")]
public class ObjectInfo : ScriptableObject
{
    //Cada objeto las caracteristicas que tiene son estas
    public int id;
    public string name;
    public GameObject prefab;
    public ObjectType type;
   
}
 