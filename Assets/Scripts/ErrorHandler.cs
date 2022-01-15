using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorHandler : MonoBehaviour
{
    [SerializeField] private ExponentialWFC wfc;

    public void ResetScript()
    {
        Debug.LogError("Reset Script");
        wfc.gameObject.SetActive(false);
        wfc.gameObject.SetActive(true);
    }
}
