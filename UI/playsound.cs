
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class playsound : UdonSharpBehaviour
{
    public AudioSource sound;
    public void soundplay(){
        sound.Play();
    }
}
