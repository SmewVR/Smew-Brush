
// Calculate bobbing motion
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class FloatAround : UdonSharpBehaviour
{
    public float FloatStrength = 5f; // Adjust the float strength
    public float BobbingHeight = 5f; // Desired bobbing height
    public float BobbingSpeed = 2f; // Adjust the bobbing speed
    public Rigidbody rb;
    public bool isUp = true;
    private bool isPickedUp = false;
    private float initialY;
    private float bobbingOffset = 0f;
    private float previousBobbingOffset = 0f;
    private Vector3 newPosition;

    void Start()
    {
        initialY = transform.position.y;
        // Add force to make the object float up
    }

    public override void OnPickup()
    {
        rb.isKinematic = true;
        isPickedUp = true;
        previousBobbingOffset = bobbingOffset; // Store the current bobbing offset
    }

    public override void OnDrop()
    {
        rb.isKinematic = false;
        isPickedUp = false;
        bobbingOffset = previousBobbingOffset; // Restore the previous bobbing offset
    }

    private float bobbingTime = 0f;

    void Update()
    {
        if (!isPickedUp)
        {
            if (isUp)
            {
                rb.AddForce(Vector3.up * FloatStrength);
            }
            else
            {
                newPosition = new Vector3(transform.position.x, initialY + bobbingOffset, transform.position.z);
                rb.MovePosition(newPosition);
                // Calculate bobbing motion
                bobbingOffset = Mathf.Sin(bobbingTime * BobbingSpeed) * BobbingHeight;
                bobbingTime += 0.02f; // Increment bobbingTime by a fixed value instead of Time.deltaTime
                // Update the object's position for bobbing effect
            }
        }
        else
        {
            initialY = transform.position.y;
            bobbingTime = 0f;
        }

    }

    //[UdonSynced] private Vector3 syncedPosition;
    // public override void OnDeserialization()
    // {
    //     // Update the position when the synced variable changes
    //     transform.position = syncedPosition;
    // }

    // void LateUpdate()
    // {
    //     if (!isPickedUp)
    //     {
    //         // Sync the position for all players in the current instance
    //         syncedPosition = transform.position;
    //         RequestSerialization();
    //     }
    // }
}
        