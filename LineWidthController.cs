using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class LineWidthController : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject pensParent;

    public LineRenderer lineRenderer;
    public TrailRenderer trailRenderer;
    private Slider slider;
    public RectTransform rectTransform;

    [UdonSynced] public float lineWidth = 0.04f;
    [UdonSynced] private Vector3 scale;


    private void Start()
    {
        slider = transform.GetComponent<Slider>();
        slider.SetValueWithoutNotify(lineWidth);

        //set initial width value of preview plane to 0.02f
        scale = rectTransform.localScale;
        scale.z = lineWidth;
        rectTransform.localScale = scale;
    }

    public void _SetWidth()
    {
        // Set the owner of the object to the local player
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

        //set the synced variable to the slider value
        lineWidth = slider.value;

        // Send the new values to other clients
        RequestSerialization();
        ApplyWidth();

    }

   
    public override void OnDeserialization()
    {
        Debug.Log("OnDeserialization Applying Width Slider Values...");
        ApplyWidth();
    }

    public void ApplyWidth()
    {
        // Change the Line Renderer's width based on the Slider value
        scale = rectTransform.localScale;
        scale.z = slider.value;
        rectTransform.localScale = scale;

        slider.SetValueWithoutNotify(lineWidth);
        rectTransform.localScale = scale; //why duplicate?

    }
}

/*
float width = slider.value;
if (width != lineWidth)
{
    lineWidth = width;
}
SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(UpdateLineWidth));
*/

/*
   public void UpdateLineWidth()
   {
       // Take ownership of this script and update its synced color variables
       // Set the owner of the object to the local player
       Networking.SetOwner(Networking.LocalPlayer, gameObject);

       // Set the Line Renderer's width
       Vector3[] positions = new Vector3[lineRenderer.positionCount];
       lineRenderer.GetPositions(positions);
       lineRenderer.startWidth = lineWidth;
       lineRenderer.endWidth = lineWidth;
       lineRenderer.SetPositions(positions);

       if (trailRenderer != null) // Check if trailRenderer exists
       {
           trailRenderer.startWidth = lineWidth;
           trailRenderer.endWidth = lineWidth;
       }

   }*/
