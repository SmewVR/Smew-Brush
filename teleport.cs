using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class teleport : UdonSharpBehaviour
{
    public AudioSource soundFX;
    public GameObject destination;
    //public double cooldownTime;
    //private double lastTeleportTime;
    //private double currentTime;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        //currentTime = Networking.GetServerTimeInSeconds();
        //Debug.Log("currentTime: ");
        //Debug.Log(currentTime);

        //if (currentTime - lastTeleportTime >= cooldownTime && player == Networking.LocalPlayer) {
        if (player == Networking.LocalPlayer) {
            Networking.LocalPlayer.TeleportTo(destination.transform.position, player.GetRotation()); //player.GetPosition() * 1.1f
            soundFX.Play();
            //lastTeleportTime = currentTime;
        }
 
    }

}

/*
 
     
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        double currentTime = Networking.GetServerTimeInSeconds();

        if (currentTime - lastTeleportTime >= cooldownTime &&  player == Networking.LocalPlayer)
        {
            Networking.LocalPlayer.TeleportTo(destination.transform.position, player.GetRotation()); //player.GetPosition() * 1.1f
            soundFX.Play();
            lastTeleportTime = currentTime;
        }
    }
     
     */

