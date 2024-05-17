using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject overHeatedObject;
    public TMP_Text[] overHeatedMessage;
    [Header("¹ÝÂ¦ÀÓ")]
    public float twinkleTime = 0.5f;
    private bool isOverHeat;

    [Header("°ú¿­ Slider")]
    public Slider weaponSlider;
    public bool IsOverHeat
    {
        get { return isOverHeat; }
        set { isOverHeat = value; }
    }


    public void ShowOverHeatedMessage()
    {
        ShowMessage(overHeatedMessage);
    }

    private void ShowMessage(TMP_Text[] targets)
    {
        foreach(var target in targets)
        {
            FadeInOut(target);
        }
    }

    IEnumerator FadeInOut(TMP_Text target)
    {
        while (isOverHeat)
        {
            yield return StartCoroutine(FadeMessage(target, 1, 0));
            yield return StartCoroutine(FadeMessage(target, 0, 1));
        }
    }

    IEnumerator FadeMessage(TMP_Text targets, float start, float end)
    {
        float timeCounter = 0.0f;
        float percent = 0.0f;

        while(percent < 1)
        {
            timeCounter += Time.deltaTime;
            percent = timeCounter / twinkleTime;

            Color color = targets.color;
            color.a = Mathf.Lerp(start, end, percent);
            targets.color = color;

            yield return null;
        }
    }
}
