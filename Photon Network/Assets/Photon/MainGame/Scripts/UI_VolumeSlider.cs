using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_VolumeSlider : MonoBehaviour
{
    public Slider slider;
    public string parameter; // Master, BGM, SFX

    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private float multiplier;

    public void SliderValue(float value)
    {
        audioMixer.SetFloat(parameter, Mathf.Log10(value) * multiplier);
    }
}
