using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public class Launcher : MonoBehaviourPunCallbacks
{
    // MonoBehaviourPunCallbacks : Photon Netkwork ���¿� ���� CallBack Interface�Լ���
    // �ڵ����� ����ϰ� ����� �� �ְ� ���ִ� Ŭ����
    [Header("����")]
    public GameObject menuButtons;
    public GameObject loadingPanel;
    public TMP_Text loadingText;
    public TMP_Text currentStatus;

    [Header("�� ����")]
    public GameObject createRoomPanel;    
    public TMP_InputField roomNameInput;

    [Header("�� ����")]
    public GameObject roomPanel;
    public TMP_Text roomNameText;
    public TMP_Text playerNickNameText;

    [Header("�� �˻�")]
    public GameObject roomBroswerPanel;
    public TMP_InputField roomBroswerNameText;

    [Header("Photon RoomInfo")]
    // ���� �������� �� ���� �̸��� �����ͷ� �Ľ��ϴ� Ŭ���� RoomButton
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    private void Start()
    {
        
    }

    private void Update()
    {
        currentStatus.text = PhotonNetwork.NetworkClientState.ToString() + "\n" + "�г��� : " + PhotonNetwork.NickName;
    }

    private void CloseMenus()
    {
        menuButtons.SetActive(false);
        loadingPanel.SetActive(false);
        createRoomPanel.SetActive(false);
    }

    #region Photon Network Function

    // ��ư�� �������� public �Լ�
    // ��Ʈ��ũ ���°� ��ȭ���� �� Call back �Լ�

    public void Connect() 
    {
        CloseMenus();
        menuButtons.SetActive(true);
        loadingPanel.SetActive(true);
        loadingText.text = "������ ���� ��...";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        CloseMenus();
        menuButtons.SetActive(true);
        // �κ� ����
        PhotonNetwork.JoinLobby();

        loadingText.text = "�κ� ����..";
    }

    public void DisConnect() => PhotonNetwork.Disconnect();

    public void JoinLobby() => PhotonNetwork.JoinLobby();

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();
    }

    public void CreateRoomPanel()
    {
        CloseMenus();
        createRoomPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.LogWarning("���� ������ �ۼ����ּ���!");
            // �˾�â. �� ���� ��� �˾�
        }
        else
        {
            // ���� ����, �濡 ���� �� �ִ� �ο���, ���� ȣ��Ʈ
            RoomOptions option = new RoomOptions();
            option.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameInput.text, option);

            // �� ���� �г� �ݾ��ش�. �ε� �г��� �����ش�.
            CloseMenus();
            loadingText.text = "�� ���� ��...";
            loadingPanel.SetActive(true);

            // �� ������ �ǰ� �� ������ �ڵ�� RoomCreateCallBack
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();

        // �� ���� �г� Ȱ��ȭ
        roomPanel.SetActive(true);
        // ���� ���� : InputField ������ TMP_Text
        roomNameText.text = $"�� ���� : {PhotonNetwork.CurrentRoom.Name}";
        // �濡 ������ Client NickName ǥ�� �Ǵ� ��� Nick Name ǥ���� LayOut 

        playerNickNameText.text = PhotonNetwork.NickName;
    }

    public void JoinRoom()
    {
        //PhotonNetwork.JoinRoom(roomInfo.Name);
        PhotonNetwork.JoinRoom(roomBroswerNameText.text);

        CloseMenus();
        loadingText.text = "�� ���� ��...";
        loadingPanel.SetActive(true);
        
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

    }

    public void OpenRoomBroswer()
    {
        CloseMenus();
        // �� �˻� �г� Ȱ��ȭ.
        roomBroswerPanel.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    #endregion

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
