using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    // 메인 게임 Scene 창 -> Lobby 호출
    // Start is called before the first frame update
    void Start()
    {
        // 포톤.. 연결이 안되어 있을 때만 LoadScene보내줘
        if(!PhotonNetwork.IsConnected)
            SceneManager.LoadScene(0);
    }
}
