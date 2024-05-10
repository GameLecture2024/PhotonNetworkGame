using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public class Launcher : MonoBehaviourPunCallbacks
{
    // MonoBehaviourPunCallbacks : Photon Netkwork 상태에 따라 CallBack Interface함수를
    // 자동으로 등록하고 사용할 수 있게 해주는 클래스
    [Header("메인")]
    public GameObject menuButtons;
    public GameObject loadingPanel;
    public TMP_Text loadingText;
    public TMP_Text currentStatus;

    [Header("방 생성")]
    public GameObject createRoomPanel;    
    public TMP_InputField roomNameInput;

    [Header("방 정보")]
    public GameObject roomPanel;
    public TMP_Text roomNameText;
    public TMP_Text playerNickNameText;

    [Header("방 검색")]
    public GameObject roomBroswerPanel;
    public TMP_InputField roomBroswerNameText;

    [Header("Photon RoomInfo")]
    // 방을 생성했을 때 방의 이름을 데이터로 파싱하는 클래스 RoomButton
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    private void Start()
    {
        
    }

    private void Update()
    {
        currentStatus.text = PhotonNetwork.NetworkClientState.ToString() + "\n" + "닉네임 : " + PhotonNetwork.NickName;
    }

    private void CloseMenus()
    {
        menuButtons.SetActive(false);
        loadingPanel.SetActive(false);
        createRoomPanel.SetActive(false);
    }

    #region Photon Network Function

    // 버튼에 연결해줄 public 함수
    // 네트워크 상태가 변화했을 때 Call back 함수

    public void Connect() 
    {
        CloseMenus();
        menuButtons.SetActive(true);
        loadingPanel.SetActive(true);
        loadingText.text = "서버에 접속 중...";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        CloseMenus();
        menuButtons.SetActive(true);
        // 로비에 접속
        PhotonNetwork.JoinLobby();

        loadingText.text = "로비에 접속..";
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
            Debug.LogWarning("방의 제목을 작성해주세요!");
            // 팝업창. 방 생성 경고 팝업
        }
        else
        {
            // 방의 제목, 방에 들어올 수 있는 인원수, 방장 호스트
            RoomOptions option = new RoomOptions();
            option.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameInput.text, option);

            // 방 생성 패널 닫아준다. 로딩 패널을 열어준다.
            CloseMenus();
            loadingText.text = "방 생성 중...";
            loadingPanel.SetActive(true);

            // 방 생성이 되고 난 이후의 코드는 RoomCreateCallBack
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();

        // 방 생성 패널 활성화
        roomPanel.SetActive(true);
        // 방의 제목 : InputField 데이터 TMP_Text
        roomNameText.text = $"방 제목 : {PhotonNetwork.CurrentRoom.Name}";
        // 방에 접속한 Client NickName 표시 되는 기능 Nick Name 표시할 LayOut 

        playerNickNameText.text = PhotonNetwork.NickName;
    }

    public void JoinRoom()
    {
        //PhotonNetwork.JoinRoom(roomInfo.Name);
        PhotonNetwork.JoinRoom(roomBroswerNameText.text);

        CloseMenus();
        loadingText.text = "방 접속 중...";
        loadingPanel.SetActive(true);
        
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

    }

    public void OpenRoomBroswer()
    {
        CloseMenus();
        // 방 검색 패널 활성화.
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

        Application.Quit(); // 게임을 빌드 해야지만 테스트를 할 수 있다.
    }
    #endregion
}
