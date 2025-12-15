using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

public class HideBrushUI : UdonSharpBehaviour
{
    public GameObject[] Objects;
    public GameObject[] hide_objects;
    public AudioSource sound;
    [SerializeField] private bool isGlobal = true;

    public bool is_back;

    private bool isMaster = false;

    void Start()
    {
        if (Networking.IsMaster)
        {
            isMaster = true;
        }

    }
    public override void Interact()
    {
        sound.Play();
       
        if (is_back)
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "back_button_pressed");
        }
        else
        {
           if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "menu_button_pressed");

        }
   
    }

    public void back_button_pressed()
    {
        foreach (GameObject obj in Objects)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
            }
        }
        foreach (GameObject obj in hide_objects)
        {
            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
            }
        }
    }

    public void menu_button_pressed()
    {
        foreach (GameObject obj in Objects)
        {
            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
        }
    }
}