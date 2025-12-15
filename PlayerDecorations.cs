
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerDecorations : UdonSharpBehaviour
{

    // World capacity is 10, so we create a new array with length of 20 (Hard cap)
    VRCPlayerApi[] players = new VRCPlayerApi[20];
    //public AudioSource joinSound;
    //public AudioSource leaveSound;

    void Start()
    {
        VRCPlayerApi.GetPlayers(players);

        foreach (VRCPlayerApi player in players)
        {
            if (player == null) continue;
            player.SetVoiceGain(6.0f);
            player.SetVoiceDistanceNear(0.01f);
            player.SetVoiceDistanceFar(50.0f);
            //avatar volume control
            player.SetAvatarAudioGain(3f);
            player.SetGravityStrength(0.85f);
            //Debug.Log(player.displayName);
        }
    }
    /*
    private void OnPlayerConnected(VRCPlayerApi player)
    {
        joinSound.Play();
    }

    private void OnPlayerDisconnected(VRCPlayerApi player)
    {
        leaveSound.Play();
    }
    */

}