using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using System.Collections.Generic;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SetSkyboxLateJoin : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject[] _skyboxOptions; // List of GameObjects with Material references

    public AudioSource SoundFX;

    private VRCPlayerApi _localPlayer;

    private bool isDeserializing = false; // Flag to track deserialization process

    public int index;

    [UdonSynced]
    private int synced_index;

    [UdonSynced]
    private bool syncSkybox = false;

    //[SerializeField] private bool isGlobal = true;

    public GameObject[] activated_objects; //.SetActive(true);
    public GameObject[] disabled_objects; //.SetActive(false);

    public void OnEnable()
    {
        _localPlayer = Networking.LocalPlayer;
    }

    // Getter property for accessing skyboxOptions
    private GameObject[] skyboxOptions
    {
        get { return _skyboxOptions; }
    }

    public override void Interact()
    {
        Networking.SetOwner(_localPlayer, gameObject);

        if (Networking.IsOwner(gameObject))
        {
            
            RequestSerialization(); //use this for syncing colors and width values
        }

        synced_index = index;

        // Play sound effect
        SoundFX.Play();

        // Call the method to update the skybox for all players currently in the instance
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "toggle_skybox");

    }

    public void toggle_skybox()
    {

        foreach (GameObject obj in activated_objects)
            obj.SetActive(true);

        foreach (GameObject obj in disabled_objects)
            obj.SetActive(false);

        Material skyboxMaterial = skyboxOptions[synced_index].GetComponent<Renderer>().material;
        RenderSettings.skybox = skyboxMaterial;

        if (!syncSkybox)
        {
            syncSkybox = true; // Set the flag to true after updating the skybox
        }

        Debug.Log("sync Index after toggle skybox: ");
        Debug.Log(synced_index);

        Debug.Log("sync Skybox bool should always be true after toggling skybox: ");
        Debug.Log(syncSkybox);
    }

    public override void OnDeserialization()
    {
        Debug.Log("Sync Skybox: OnDeserialization");
        Debug.Log(syncSkybox);

        Debug.Log("isDeserializing: OnDeserialization");
        Debug.Log(isDeserializing);

        if (!isDeserializing && syncSkybox)
        {
            isDeserializing = true; // Set the flag before calling toggle_skybox
            Debug.Log("OnDeserialization synced_index: ");
            Debug.Log(synced_index);

            toggle_skybox();
        }
        

    }

}
