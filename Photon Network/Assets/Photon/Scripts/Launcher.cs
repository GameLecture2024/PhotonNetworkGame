using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEditor;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("����")]
    public GameObject menuButtons;
    public GameObject loadingPanel;
    public TMP_Text loadingText;

    [Header("�� ����")]
    public GameObject createRoomPanel;
    public TMP_InputField roomNameText;

    #region Button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit(); // ������ ���� �ؾ����� �׽�Ʈ�� �� �� �ִ�.
    }
    #endregion
}
