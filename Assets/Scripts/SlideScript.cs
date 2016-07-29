using UnityEngine;
using System.Collections;

public class SlideScript : MonoBehaviour
{

    public float Speed = 1f;

    int leftIndex = -1;
    int rightIndex = -1;
    
    public Transform CameraHead;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (leftIndex == -1) //Always map out left controller
        {
            leftIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost, Valve.VR.ETrackedDeviceClass.Controller);
        }
        //if (rightIndex == -1) //Map right controller
        //{
        //    rightIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, Valve.VR.ETrackedDeviceClass.Controller);
        //}
        //if (rightIndex == leftIndex) //But unmap it if we mapped it to the same left controller
        //{
        //    rightIndex = -1;
        //}

        if (leftIndex != -1)
        {
            var isTouched = SteamVR_Controller.Input(leftIndex).GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
            var pad = SteamVR_Controller.Input(leftIndex).GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
            //Debug.Log("Pad input: " + pad + ", " + isTouched, this);
            if (isTouched)
            {
                var fwd = CameraHead.forward;
                fwd.y = 0;
                fwd = fwd.normalized * pad.y;

                var right = CameraHead.right;
                right.y = 0;
                right = right.normalized * pad.x;

                transform.position = transform.position + (fwd + right) * Speed;
            }
        }


    }
}
