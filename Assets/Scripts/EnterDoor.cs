using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterDoor : MonoBehaviour
{
    public GameObject enterArrow;

    public void ShowEnterable()
    {
        enterArrow.SetActive(true);
    }

    public void HideEnterable()
    {
        enterArrow.SetActive(false);
    }

    public virtual void Enter()
    {

    }
}
