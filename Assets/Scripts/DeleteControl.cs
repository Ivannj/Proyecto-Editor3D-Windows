using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteControl : MonoBehaviour
{

    public List<GameObject> toDelete;
    public SnapControl snaps;


    private void Start()
    {
        snaps = GetComponent<SnapControl>();
    }
    public void DeleteObjects()
    {
        if(snaps != null)
        {
            for (int i = 0; i < snaps.allSnaps.Length; i++)
            {
                Destroy(snaps.allSnaps[i]);
            }
        }
        if (toDelete != null)
        {
            for (int i = 0; i < toDelete.Count; i++)
            {
                Destroy(toDelete[i]);
            }
        }
            Destroy(gameObject);
    }
}
