using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEditor;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("메인")]
    public GameObject menuButtons;
    public GameObject loadingPanel;
    public TMP_Text loadingText;

    [Header("방 생성")]
    public GameObject createRoomPanel;
    public TMP_InputField roomNameText;

    #region Button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit(); // 게임을 빌드 해야지만 테스트를 할 수 있다.
    }
    #endregion
}
