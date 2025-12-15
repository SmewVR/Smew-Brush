
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GlobalColorSliderController : UdonSharpBehaviour
{
     
    [UdonSynced]
    private float syncedValue = 1;
    //private bool deserializing;
    private Slider slider;
    // private void Awake()
    // {
    //     pickup = GetComponentInParent<VRC_Pickup>();
    // }
    
    private VRC_Pickup pickup;

    //public GameObject material;
    
        /*
    public Slider slider_Hue;
    public Slider slider_Saturation;
    public Slider slider_Value;
    */

    /*
    public Color[] customThemeColors = new Color[4]{
                    Color.yellow,
                    Color.blue,
                    Color.red,
                    Color.green
                };
                */
    private void Start()
    {
        
        slider = transform.GetComponent<Slider>();
        //set the initial slider's value to the 
        slider.SetValueWithoutNotify(slider.value);
        
        pickup = GetComponentInParent<VRC_Pickup>();
        
        //syncedValue = slider.value;
        //deserializing = false;

        //if (Networking.IsOwner(gameObject))
        //    RequestSerialization();
    }
    public void disablePickup()
        {
            pickup.pickupable = false;
        }
        public void enablePickup()
        {
            pickup.pickupable = true;
        }
    public void _SetSyncedValue()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

        syncedValue = slider.value;

        RequestSerialization();
        SlideUpdate();
    }
    public override void OnDeserialization()
    {

        //slider.value = syncedValue;
        Debug.Log("OnDeserialization Applying HSV Value...");

        SlideUpdate();
    }

    public void SlideUpdate()
    {
        /*
        if (slider == null)
            return;
        if (deserializing)
            return;
        */
        
        // When the synced values change, update the slider values and preview material color
        slider.SetValueWithoutNotify(syncedValue);
        
        //in future, set preview value real time like Color Slider script line 64 

        //syncedValue = slider.value;
        //RequestSerialization();
        //ApplyPreviewColor();
    }
    /*
    private void ApplyPreviewColor()
    {
        // update HSV sliders
        float H, S, V;

        Color.RGBToHSV(customThemeColors[0], out H, out S, out V);

        slider_Hue.value = H;
        slider_Saturation.value = S;
        slider_Value.value = V;

        material.GetComponent<Renderer>().material.color = Color.HSVToRGB(H, S, V);

    }*/

}