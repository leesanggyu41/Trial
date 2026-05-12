using UnityEngine;
using UnityEngine.InputSystem;
public class testArm : MonoBehaviour
{
    public RobotArmFixer armController;
    public Transform targetItem; // Inspector에서 아이템 직접 할당

    public Transform itemTransform; // 아이템의 Transform을 직접 할당

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            armController.MoveToItem(targetItem);
        }
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            // 아이템에서 멀어지는 동작을 추가할 수 있습니다.
            armController.MoveToItem(itemTransform); // 예시로 아이템 위치로 이동
        }
    }
}
