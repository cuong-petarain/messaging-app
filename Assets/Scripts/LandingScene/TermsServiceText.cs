using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TermsServiceText : MonoBehaviour, IPointerClickHandler
{
    private string termLink = "https://google.com";
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Application.OpenURL(termLink);
    }
}
