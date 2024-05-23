using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject playerPrefab;      // ���ӿ� ���� �÷��̾� ������
    public Transform[] spawnPositions;   
    private GameObject player;           // �÷��̾ ����, �ı��� �� ����� ���� ����

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)  // MainLobby���� Connect()�Լ� ���� ������ �Ǿ� �ִ� ����
        {
            Spawn();
        }
    }

    private Transform GetSpawnPosition()
    {
        int randomIndex = Random.Range(0, spawnPositions.Length);

        return spawnPositions[randomIndex];
    }

    public void Spawn()  //  Project�� �ִ� ������ Load�ϴ� ������ �ν��Ͻ�ȭ ���
    {
        //Instantiate(playerPrefab, spawnPosition.position, Quaternion.identity); ��Ʈ��ũ ������ �۵��� ���� ����.
        // ���� : ������ playerPrefab�� ������Ʈ�� photonView�� �����ϰ� �־�� �Ѵ�.
        player = PhotonNetwork.Instantiate(playerPrefab.name, GetSpawnPosition().position, Quaternion.identity); // ��Ʈ��ũ ������Ʈ �ν��Ͻ�ȭ
    } 
}
