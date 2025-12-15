
using UnityEngine;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VolumeToggle : UdonSharpBehaviour
{
    [Tooltip("List of objects to toggle on and off")]
    public GameObject[] toggleObjects;
    public AudioSource sound;

    public override void Interact()
    {

        if (sound)
            sound.Play();

        foreach (GameObject toggleObject in toggleObjects)
        {
            if (toggleObject != null)
            {
                toggleObject.SetActive(!toggleObject.activeSelf);
            }
        }
    }
}

