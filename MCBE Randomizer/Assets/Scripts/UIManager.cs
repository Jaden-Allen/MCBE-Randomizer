using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{


    public void SetActive(CustomToggle customToggle)
    {
        customToggle.onText.SetActive(customToggle.toggle.isOn);
        customToggle.offText.SetActive(!customToggle.toggle.isOn);
    }
}
