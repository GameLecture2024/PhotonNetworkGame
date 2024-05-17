using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool isAutomatic;
    public float fireCoolTime = .1f, heatPerShot = 1f;
    public GameObject MuzzleFlash;
}
