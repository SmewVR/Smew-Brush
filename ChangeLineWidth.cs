using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;
namespace QvPen.UdonScript.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ChangeLineWidth: UdonSharpBehaviour
    {

        [SerializeField]
        private GameObject pensParent;

        [UdonSynced] private float customInkWidth;

        public AudioSource sound;
        public TrailRenderer tr;
        public Slider slider;

        private VRC_Pickup pickup;

        public void Start()
        {
            customInkWidth = slider.value;
            pickup = GetComponentInParent<VRC_Pickup>();
        }

        public void startWidth()
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

            RequestSerialization();
            UpdateWidth();
            sound.Play();
        }

        public void enablePickup()
        {
            pickup.pickupable = true;
        }
        
        public void disablePickup()
        {
            pickup.pickupable = false;
        }
        
        public override void OnDeserialization()
        {
            Debug.Log("OnDeserialization 'Updating Pen Width'");
            UpdateWidth();
        }
          
        // This method is called by the ColorSlider script when the slider values change
        public void UpdateWidth()
        {
            customInkWidth = slider.value;

            pensParent.GetComponent<QvPen_PenManager>().ChangeLineWidth(customInkWidth);
            // update  audio pitch with slider value
            sound.pitch = 1 - customInkWidth; 
            if (tr != null) // Check if trailRenderer exists
            {
                tr.startWidth = customInkWidth;
                tr.endWidth = customInkWidth;
            }
        }

    }
}
