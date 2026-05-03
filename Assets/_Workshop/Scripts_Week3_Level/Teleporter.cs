using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Teleporter : MonoBehaviour
{
    [Header("傳送設定")]
    [Tooltip("要把玩家傳送到哪個物件的位置？ (請拉一個空物件過來)")]
    public Transform destination;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true; // 強制防呆
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && destination != null)
        {
            // 1. 【關鍵】解除相機的防抖冷卻，確保等一下目的地的 RoomTrigger 絕對能呼叫相機
            if (RoomCameraController.Instance != null)
            {
                RoomCameraController.Instance.ResetCooldown();
            }

            // 2. 瞬間移動玩家到目標點
            other.transform.position = destination.position;

            // 3. 更新全域重生點
            GameFlowManager.UpdateRespawnPoint(destination.position);
        }
    }
}