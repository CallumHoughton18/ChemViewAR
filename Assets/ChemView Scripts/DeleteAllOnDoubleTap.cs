using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteAllOnDoubleTap : MonoBehaviour, IPointerClickHandler
{
    float lastClick = 0f;
    float interval = 0.4f;
    public ChemViewARController MainController;


    public void OnPointerClick(PointerEventData eventData)
    {
        if ((lastClick + interval) > Time.time)
        {
            MainController.DeleteAllMolecules();
        }

        else
        {
            lastClick = Time.time;
        }
    }

}
