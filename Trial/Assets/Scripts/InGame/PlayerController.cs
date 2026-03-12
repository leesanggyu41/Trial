using UnityEngine;
using Fusion;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    [Header("카메라 설정")]
    public Transform HeadCameraPoint;
    public float mouseSensitivity = 2f;
    [Header("카메라 제한")]
    public float Xlimit = 60f;
    public float MinYlimit = -30f;
    public float MaxYlimit = 30f;
    [Header("닉네임")]
    public TMP_Text NameText;
    public Transform NamePoint;

    private Camera _camera;
    private float CameraX;
    private float CameraY;


    public override void Spawned()
    {
        //내 캐릭터에만 카메라 붙이기
        if (HasInputAuthority)
        {
            _camera = Camera.main;
            _camera.transform.SetParent(HeadCameraPoint);
            _camera.transform.localPosition = Vector3.zero;
            _camera.transform.localRotation = Quaternion.identity;

            Cursor.lockState = CursorLockMode.Locked;
        }
        PlayerData playerData = GetComponent<PlayerData>();
        if(playerData != null)
        {
            NameText.text = playerData.Nickname.ToString();

            Transform spawnPoint = GameSceneManager.Instance.GetSpawnPoint(GetComponent<PlayerObject>().PlayerIndex);

            if (spawnPoint != null)
            {
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;
            }
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        if(!HasInputAuthority) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

         CameraX += mouseX;
         CameraY -= mouseY;

         CameraX = Mathf.Clamp(CameraX , -Xlimit ,Xlimit);
         CameraY = Mathf.Clamp(CameraY , MinYlimit, MaxYlimit);

         HeadCameraPoint.localRotation = Quaternion.Euler(CameraY, CameraX , 0f);
    }

    private void FixedUpdate()
    {
        if(NamePoint == null || _camera == null) return;

        NamePoint.LookAt(NamePoint.position+ _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
    }

}
