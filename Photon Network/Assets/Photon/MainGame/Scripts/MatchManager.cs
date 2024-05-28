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
#region �̱���
    public static MatchManager Instance;
    public void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    #endregion

    public TMP_Text killText, deathText;

    // 3���� �̺�Ʈ. �÷��̾ �濡 ���� ���� ��.. ��� �÷��̾����� ����...  ������ Update �����ϴ�.
    public enum EventCodes : byte // byte�� �̺�Ʈ �ڵ带 �ۼ��ϸ�, ���� ����ȯ�� ���� �ʱ� ������ ������ �� �߻��Ѵ�.
    {
       NewPlayer,
       ListPlayers,
       UpdateStats
    }

    private EventCodes eventCodes;

    [SerializeField] List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;            // PhotonView.IsMine �� �ڽ��� Index�� ����.

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // ���� ���� Scene â -> Lobby ȣ��
    // Start is called before the first frame update
    void Start()
    {
        // ����.. ������ �ȵǾ� ���� ���� LoadScene������
        if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene(0);
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200) // ���� ���ڴ� Custom Event
        {
            EventCodes eventCode = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            eventCodes = (EventCodes)photonEvent.Code;
            Debug.Log("���Ź��� �̺�Ʈ�� ���� " + eventCodes);

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

    // Send �Լ����� Photon Raise Event  
    // Receive �Լ����� �Ű� ������ ���� Event Class�� �ΰ��ӿ� �����ϴ� �ڵ�.

    public void NewPlayerSend(string username)
    {
        // �г��� - ���� �α���, Actor - LcaolPlayer.ActorNumber, Kill = 0, Death = 0

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
    public void ListPlayersSend() // masterClient <- ���ο� �÷��̾ ������ �ʰ� �� ������ �����. ������ �ٸ� Client���� �������ִ� ���. PlayerInfo ��Ŷȭ�ؼ� ������ȴ�.
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
    /// statToUpdate 0�̸� ų , 1�̸� ����
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
                // UpdateView Kill, Death text ��ȭ�ϴ� �Լ�
                UpdateStatsDisPlay();
            }
            break;
        }
    }

    private void UpdateStatsDisPlay()
    {
        if(allPlayers.Count > index)
        {
            killText.text = $"ų �� : {allPlayers[index].kill}";
            deathText.text = $"���� �� : {allPlayers[index].death}";
        }
        else
        {
            killText.text = $"ų �� : 0";
            deathText.text = $"���� �� : 0";
        }
    }
}
