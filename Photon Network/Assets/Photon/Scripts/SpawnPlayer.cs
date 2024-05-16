using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject playerPrefab;      // ���ӿ� ���� �÷��̾� ������
    public Transform spawnPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)  // MainLobby���� Connect()�Լ� ���� ������ �Ǿ� �ִ� ����
        {
            Spawn();
        }
    }

    public void Spawn()  //  Project�� �ִ� ������ Load�ϴ� ������ �ν��Ͻ�ȭ ���
    {
        //Instantiate(playerPrefab, spawnPosition.position, Quaternion.identity); ��Ʈ��ũ ������ �۵��� ���� ����.
        // ���� : ������ playerPrefab�� ������Ʈ�� photonView�� �����ϰ� �־�� �Ѵ�.
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition.position, Quaternion.identity); // ��Ʈ��ũ ������Ʈ �ν��Ͻ�ȭ
    } 
}
