using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Toggle_Brush_UI : UdonSharpBehaviour
{
    public GameObject[] ui_objects;
    public bool isGlobal;
    public AudioSource sound;
    private bool isOn;

    private void Start()
    {
        isOn = true;

        /*
        #if UNITY_ANDROID
                if (gameObject.GetComponent<MeshRenderer>().materials[1]) { 
                    gameObject.GetComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().materials[1];
                }
        #else
                gameObject.GetComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().materials[0];
        #endif
        */
        // transform list holds sounds 
    }
    public override void Interact()
    {
        sound.Play();
        // Check the state dynamically
        bool anyActive = false;
        foreach (GameObject ui in ui_objects)
        {
            if (ui.activeInHierarchy)
            {
                anyActive = true;
                break;
            }
        }

        if (isGlobal)
        {
            if (anyActive)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "toggle_objects_off");
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "toggle_objects_on");
            }
        }
        else
        {
            if (anyActive)
            {
                toggle_objects_off();
            }
            else
            {
                toggle_objects_on();
            }
        }
    }
    public void toggle_objects_off()
    {
        foreach (GameObject ui in ui_objects)
        {
            if (ui.activeInHierarchy)
            {
                ui.SetActive(false);
            }
        }
    }
     public void toggle_objects_on()
    {
        foreach (GameObject ui in ui_objects)
        {
            if (!ui.activeInHierarchy)
            {
                ui.SetActive(true);
            }
        }
    }
}

