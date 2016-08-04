using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(Canvas))]
public class MenuScript : MonoBehaviour {

    public bool HostOnStartup;

    State state;

    Canvas canvas;

    public Button ButtonHost;
    public Button ButtonExternal;
    public Button ButtonStart;
    public Button ButtonExit;

    public Text Title;
    public Text Hostname;
    public Text PlayerList;

    public Camera ExternalCamera;

    public void Off()
    {
        canvas.enabled = false;
    }

    public void On()
    {
        canvas.enabled = true;
    }

    public void Toggle()
    {
        if (state == State.ingame)
        {
            canvas.enabled = !canvas.enabled;
        }
    }

    public void SwitchMode(bool main)
    {
        Title.gameObject.SetActive(main);
        ButtonHost.gameObject.SetActive(main);
        
        ButtonStart.gameObject.SetActive(!main);
        Hostname.gameObject.SetActive(!main);
        PlayerList.gameObject.SetActive(!main);
    }
    
	// Use this for initialization
	void Start () {
        state = State.main;
        canvas = GetComponent<Canvas>();
        if (HostOnStartup)
        {
            StartCoroutine(OnStartupHost());
        }
	}
	
    IEnumerator OnStartupHost()
    {
        OnButtonHost();
        yield return new WaitForSeconds(15);
        OnButtonStart();
    }

	// Update is called once per frame
	void Update () {
        var li = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost, Valve.VR.ETrackedDeviceClass.Controller);
        var ri = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost, Valve.VR.ETrackedDeviceClass.Controller);
        
        if (SteamVR_Controller.Input(li).GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu) || SteamVR_Controller.Input(ri).GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            Toggle();
        }
    }

    enum State
    {
        main,
        waiting,
        ingame
    }

    public void OnButtonHost()
    {
        if (state == State.main)
        {
            PrevailServer.Instance.StartHost();
            state = State.waiting;

            SwitchMode(false);
        }
    }

    public void OnButtonExternal()
    {
        if (ExternalCamera.gameObject.activeSelf)
        {
            ExternalCamera.gameObject.SetActive(false);
        }
        else
        {
            ExternalCamera.gameObject.SetActive(true);
        }
    }

    public void OnButtonStart()
    {
        if (state == State.waiting)
        {
            GameScript.Instance.StartGame();
            state = State.ingame;

            ButtonStart.gameObject.SetActive(false);
            Off();
        }
    }

    public void OnButtonExit()
    {
        if(state == State.ingame || state == State.waiting)
        {
            PrevailServer.Instance.StopHost();
            state = State.main;

            SwitchMode(true);
        }
        else if (state == State.main)
        {
            Application.Quit();
        }
    }

}
