using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class LeaveRoomManager : MonoBehaviour
{
    public void OnClickLeaveRoom()
    {
        ServerConnectionManager.Instance.LeaveRoom();
    }
}