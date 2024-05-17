using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float muzzleDisplayTime = 0.01666f;
    private float muzzleCounter;

    private void OnEnable()
    {
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        DeActivateMuzzle();
    }

    private void Reset()
    {
        muzzleCounter = muzzleDisplayTime;
    }

    private void DeActivateMuzzle()
    {
        if (gameObject.activeSelf)
        {
            muzzleCounter -= Time.deltaTime;

            if(muzzleCounter <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }

}
