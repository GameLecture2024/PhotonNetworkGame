using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    // ���� ���� Scene â -> Lobby ȣ��
    // Start is called before the first frame update
    void Start()
    {
        // ����.. ������ �ȵǾ� ���� ���� LoadScene������
        if(!PhotonNetwork.IsConnected)
            SceneManager.LoadScene(0);
    }
}
