using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public Transform rotationCamera, mainCamera;
    private float angleCamera;
    public float speedRotation, speedZoom, speedMove;
    void Start()
    {
        rotationCamera = transform.GetChild(0);
        mainCamera = Camera.main.transform;
        angleCamera = 0;

    }

    void Update()
    {
        //Rotation right yup
        if (Input.GetMouseButton(1))
        {
            angleCamera += Input.GetAxis("Mouse Y") * 4;
            angleCamera = Mathf.Clamp(angleCamera, -60, 0);
            rotationCamera.localEulerAngles = new Vector3(-angleCamera, 0, 0);

            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * speedRotation * Time.deltaTime);
        }

        float currentCameraZ = mainCamera.localPosition.z;
        currentCameraZ += Input.GetAxis("Mouse ScrollWheel") * speedZoom * Time.deltaTime;
        currentCameraZ = Mathf.Clamp(currentCameraZ, -20, -1);
        mainCamera.localPosition = new Vector3(0, 0, currentCameraZ);


        transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * speedMove * Time.deltaTime);
        transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * speedMove * Time.deltaTime);

    } 
}
