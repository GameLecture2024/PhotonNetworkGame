using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MatchManager;

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kill, death;

    public PlayerInfo(string name, int actor, int kill, int death)
    {
        this.name = name;
        this.actor = actor;
        this.kill = kill;
        this.death = death;
    }
}

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
#region 싱글톤
    public static MatchManager Instance;
    public void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    #endregion

    public TMP_Text killText, deathText;

    // 3가지 이벤트. 플레이어가 방에 접속 했을 때.. 모든 플레이어한테 전송...  정보를 Update 갱신하다.
    public enum EventCodes : byte // byte로 이벤트 코드를 작성하면, 따로 형변환을 하지 않기 때문에 에러가 덜 발생한다.
    {
       NewPlayer,
       ListPlayers,
       UpdateStats
    }

    private EventCodes eventCodes;

    [SerializeField] List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;            // PhotonView.IsMine 나 자신의 Index를 저장.

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // 메인 게임 Scene 창 -> Lobby 호출
    // Start is called before the first frame update
    void Start()
    {
        // 포톤.. 연결이 안되어 있을 때만 LoadScene보내줘
        if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene(0);
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200) // 작은 숫자는 Custom Event
        {
            EventCodes eventCode = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            eventCodes = (EventCodes)photonEvent.Code;
            Debug.Log("수신받은 이벤트의 정보 " + eventCodes);

            switch (eventCode)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStats:
                    UpdateStatsReceive(data);
                    break;
            }
        }
    }

    // Send 함수에는 Photon Raise Event  
    // Receive 함수에는 매개 변수로 받은 Event Class를 인게임에 적용하는 코드.

    public void NewPlayerSend(string username)
    {
        // 닉네임 - 포톤 로그인, Actor - LcaolPlayer.ActorNumber, Kill = 0, Death = 0

        object[] playerInfo = new object[4] {username, PhotonNetwork.LocalPlayer.ActorNumber, 0, 0};

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.MasterClient
        };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer, playerInfo, raiseEventOptions, sendOptions);
    }
    public void NewPlayerReceive(object[] data) 
    {
        PlayerInfo playerInfo = new PlayerInfo((string)data[0], (int)data[1], (int)data[2], (int)data[3]);

        allPlayers.Add(playerInfo);

        ListPlayersSend();
        UpdateStatsDisPlay();
    }
    public void ListPlayersSend() // masterClient <- 새로운 플레이어가 들어오면 너가 그 정보를 기억해. 정보를 다른 Client한테 전달해주는 기능. PlayerInfo 패킷화해서 보내면된다.
    {
        object[] packet = new object[allPlayers.Count];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] info = new object[4];

            info[0] = allPlayers[i].name;
            info[1] = allPlayers[i].actor;
            info[2] = allPlayers[i].kill;
            info[3] = allPlayers[i].death;

            packet[i] = info;
        }

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent((byte)EventCodes.ListPlayers, packet, raiseEventOptions, sendOptions);
    }
    public void ListPlayersReceive(object[] data) // room <1,2,3,4... 1 12 123 1234
    {
        allPlayers.Clear();

        for (int i = 0; i< data.Length; i++)
        {
            object[] info = (object[])data[i];

            PlayerInfo player = new PlayerInfo((string)info[0], (int)info[1], (int)info[2], (int)info[3]);

            allPlayers.Add(player);

            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i;
            }
        }

    }

    // PlayerController. //TakeDamage // Spawner : Die
    /// <summary>
    /// statToUpdate 0이면 킬 , 1이면 데스
    /// </summary>
    public void UpdateStatsSend(int actorIndex, int statToUpdate, int amountToChange) 
    {
        object[] packet = new object[] { actorIndex, statToUpdate, amountToChange };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent((byte)EventCodes.UpdateStats, packet, raiseEventOptions, sendOptions);
    }
    public void UpdateStatsReceive(object[] data) 
    {
        int actor = (int)data[0];
        int stat = (int)data[1];
        int amount = (int)data[2];

        for(int i =0; i< allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (stat)
                {
                    case 0: // kills
                        allPlayers[i].kill += amount;
                        break;
                    case 1: // Deaths
                        allPlayers[i].death += amount;
                        break;
                }
            }

            if(i == index)
            {
                // UpdateView Kill, Death text 변화하는 함수
                UpdateStatsDisPlay();
            }
            break;
        }
    }

    private void UpdateStatsDisPlay()
    {
        if(allPlayers.Count > index)
        {
            killText.text = $"킬 수 : {allPlayers[index].kill}";
            deathText.text = $"데스 수 : {allPlayers[index].death}";
        }
        else
        {
            killText.text = $"킬 수 : 0";
            deathText.text = $"데스 수 : 0";
        }
    }
}
