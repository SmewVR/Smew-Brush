using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

public class set_skybox : UdonSharpBehaviour
{
    public Material mat1;
    public AudioSource SoundFX;

    private int activeObjectIndex = 0;
    [SerializeField] private bool isGlobal = true;

    public GameObject[] activated_objects; //.SetActive(true);
    public GameObject[] disabled_objects; //.SetActive(false);

    private bool syncSkybox = false;

    private void Start()
    {
        // Set the initial skybox for all players
        night_sky();
    }

    public override void Interact()
    {
        SoundFX.Play();
        if (!isGlobal)
        {
            // Local
            toggle_skybox();
        }
        else if (isGlobal)
        {
            // Global
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "toggle_skybox");
            RequestSerialization(); // Trigger serialization to sync changes for late joiners

        }
    }

    public void night_sky()
    {
        foreach (GameObject obj in activated_objects)
            obj.SetActive(true);

        foreach (GameObject obj in disabled_objects)
            obj.SetActive(false);

        RenderSettings.skybox = mat1;

        syncSkybox = true; // Set the flag to true when updating the skybox

    }

    public void toggle_skybox()
    {
        night_sky();
        // If it's a global change, send the network event to all players.
        if (isGlobal)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "night_sky");
        }
    }

    public override void OnPostSerialization(SerializationResult result)
    {
        if (syncSkybox)
        {
            night_sky();
            syncSkybox = false;
        }

        if (!result.success)
        {
            // Handle serialization failure if needed
        }
    }

    public void SyncSkybox()
    {
        syncSkybox = true;
    }
}
