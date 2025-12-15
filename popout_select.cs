
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
public class popout_select : UdonSharpBehaviour
{
    private bool hasIncreased = false;
    public int amount = 10;
    public bool hideimage = true;
    private RectTransform parentRectTransform;

    private GameObject firstChild;

    private void Start()
    {
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
        firstChild = transform.parent.GetChild(0).gameObject;
        firstChild.gameObject.SetActive(false);
    }
    public void IncreaseParentPosZ()
    {
        if (!hasIncreased && transform.parent != null)
        {
            if (parentRectTransform != null)
            {
                // 
                Vector3 newPosition = parentRectTransform.anchoredPosition3D;
                newPosition.z = -amount;
                parentRectTransform.anchoredPosition3D = newPosition;

                //current game object (clickable collider) position remains
                Vector3 currentGameObjectPosition = transform.GetComponent<RectTransform>().anchoredPosition3D;
                currentGameObjectPosition.z = amount;
                GetComponent<RectTransform>().anchoredPosition3D = currentGameObjectPosition;

                hasIncreased = true;
            }
        }
        // Find the Image component in the same scope and set the GameObject active
        if (hideimage)
        {
        if (firstChild != null)
        {
            firstChild.gameObject.SetActive(true);
        }
    
        }
    }

    public void DecreaseParentPosZ()
    {
        if (hasIncreased && transform.parent != null)
        {
            if (parentRectTransform != null)
            {
                //
                Vector3 newPosition = parentRectTransform.anchoredPosition3D;
                newPosition.z = 0;
                parentRectTransform.anchoredPosition3D = newPosition;

                //current game object (clickable collider) position remains
                Vector3 currentGameObjectPosition = transform.GetComponent<RectTransform>().anchoredPosition3D;
                currentGameObjectPosition.z = 0;
                GetComponent<RectTransform>().anchoredPosition3D = currentGameObjectPosition;

                hasIncreased = false;
            }
        }
        if (firstChild != null)
        {
            firstChild.gameObject.SetActive(false);
        }
      
    }

}