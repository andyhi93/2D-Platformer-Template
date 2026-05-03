using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomTrigger : MonoBehaviour
{
    [Header("房間重生點設定")]
    [Tooltip("玩家死掉後要在哪裡復活？(請拖曳一個空的子物件進來。如果留空，則自動設為玩家剛踏入此房間時的位置)")]
    public Transform customRespawnPoint;

    [Header("房間觸發事件 (給機關使用)")]
    public UnityEvent OnRoomEnter;
    public UnityEvent OnRoomStay;
    public UnityEvent OnRoomExit;

    private BoxCollider2D roomCollider;

    void Awake()
    {
        roomCollider = GetComponent<BoxCollider2D>();
        roomCollider.isTrigger = true; // 強制防呆，確保一定是 Trigger
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 紀錄重生點
            // 如果企劃有手動拉一個重生點，就用企劃拉的；如果沒有，就用玩家"踩線"進來的當下座標
            Vector3 respawnPos = (customRespawnPoint != null) ? customRespawnPoint.position : other.transform.position;

            GameFlowManager.UpdateRespawnPoint(respawnPos);

            // 2. 呼叫相機移動到這個房間的中心點 (BoxCollider2D 的中心)
            if (RoomCameraController.Instance != null)
            {
                // 用 transform.position (物件中心) 來對齊相機
                RoomCameraController.Instance.MoveCameraTo(transform.position);
            }

            // 3. 觸發進入事件 (可以接「關門」、「生怪」等)
            OnRoomEnter.Invoke();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnRoomStay.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnRoomExit.Invoke();
        }
    }

    // 👇 讓企劃在 Scene 視窗可以清楚看到房間範圍與重生點
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }

        if (customRespawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(customRespawnPoint.position, 0.3f);
        }
    }
}