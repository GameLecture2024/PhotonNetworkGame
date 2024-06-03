using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource[] allBGM;

    private int bgmIndex = 0; // 0 ~ allBGM.Length
    private bool isPlayingBGM = false;

    private void Start()
    {
        isPlayingBGM = true;
        PlayRandomBGM();
    }

    // BGM을 변경하는 기능

    public void Update()
    {
       
    }



    // Random BGM 재생
    public void PlayRandomBGM()
    {
        int randomIndex = UnityEngine.Random.RandomRange(0, allBGM.Length);
        PlayBGM(randomIndex);
    }

    // 전체 BGM 종료
    public void StopAllBGM()
    {
        foreach(var bgm in allBGM)
        {
            bgm.Stop();
        }
    }

    // BGM 재생
    public void PlayBGM(int index)
    {
        bgmIndex = index;

        StopAllBGM();

        allBGM[bgmIndex].Play();
    }
}
