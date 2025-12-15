
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ColliderBounce : UdonSharpBehaviour
{
    //[UdonSynced] private float _bounceValue = 5f;
    public AudioSource soundFX;
    private VRCPlayerApi localUser;
    //public Slider bounceSlider;
    private float tempspeed;
    public float syncedSpeed = 10f;

    void Start()
    {
        localUser = Networking.LocalPlayer;
        tempspeed = syncedSpeed;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            //localUser.SetVelocity(flyDirection.rotation * Vector3.up * syncedSpeed);

            Debug.Log(player.GetVelocity());
            Debug.Log("boost val: ");
            Debug.Log(Mathf.Abs(player.GetVelocity().y) / 2);

            //syncedSpeed = 5f + Mathf.Abs(player.GetVelocity().y)/2;
           
           

            // Increment the syncedSpeed with each bounce
            syncedSpeed = tempspeed + Mathf.Abs(player.GetVelocity().y) / 2;
            //syncedSpeed = Mathf.Min(syncedSpeed, tempspeed*3); // Limit the speed to a maximum of 10 units
            // if (player.GetVelocity().y == 0)
            // {
            //     syncedSpeed = tempspeed + Mathf.Abs(1 - player.GetVelocity().y) / 2;
            // }

            // Apply the new velocity to the player
            localUser.SetVelocity(player.GetRotation() * Vector3.up * syncedSpeed);
            //localUser.SetVelocity(player.GetRotation() * Vector3.up * syncedSpeed);
            
            if (soundFX != null)
                soundFX.Play();
        }
    }
}
