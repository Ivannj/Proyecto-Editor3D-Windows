using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapControl : MonoBehaviour
{
    public GameObject[] allSnaps;

    public void SetSnap(GameObject[] _snap)
    {
        allSnaps = _snap;
    }
}
