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

    // BGM�� �����ϴ� ���

    public void Update()
    {
       
    }



    // Random BGM ���
    public void PlayRandomBGM()
    {
        int randomIndex = UnityEngine.Random.RandomRange(0, allBGM.Length);
        PlayBGM(randomIndex);
    }

    // ��ü BGM ����
    public void StopAllBGM()
    {
        foreach(var bgm in allBGM)
        {
            bgm.Stop();
        }
    }

    // BGM ���
    public void PlayBGM(int index)
    {
        bgmIndex = index;

        StopAllBGM();

        allBGM[bgmIndex].Play();
    }
}
