using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleBrushUI2 : UdonSharpBehaviour
{
    public GameObject ui;
    [SerializeField] private bool isGlobal = true;
    private AudioSource sound;

    private void Start()
    {
        sound = transform.GetComponent<AudioSource>();
    }
    public override void Interact()
    {
        sound.Play();

        if (ui.activeInHierarchy)
        {
            if (!isGlobal)
            {
                //Local
                ui.SetActive(false);
            }
            else if (isGlobal)
            {
                //Global
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "set_active_false");
            }
        }
        else
        {
            if (!isGlobal)
            {
                //Local
                ui.SetActive(true);
            }
            else if (isGlobal)
            {
                //Global
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "set_active_true");
            }

        }
    }

    public void set_active_true()
    {
        ui.SetActive(true);
    }

    public void set_active_false()
    {
        ui.SetActive(false);
    }


}

