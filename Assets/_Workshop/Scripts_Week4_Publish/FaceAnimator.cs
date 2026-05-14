using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FaceExpression
{
    [Tooltip("表情名稱，用來給事件呼叫 (例如：Happy, Sad, Angry)")]
    public string expressionName;

    [Header("表情參數 (相對變化量)")]
    [Tooltip("嘴巴的開合度乘數。1=保持原狀, -1=上下顛倒(難過), 0=壓扁(無表情)")]
    [Range(-1f, 1f)] public float mouthCurve = 1f;

    [Tooltip("眼睛大小縮放乘數 (1=保持原狀, 1.5=放大1.5倍, 0.5=縮小一半)")]
    public float eyeScale = 1f;

    [Tooltip("眉毛的旋轉角度 (加上去的角度：正數=無辜/八字眉，負數=生氣/倒立眉)")]
    public float eyebrowAngle = 0f;

    [Tooltip("眉毛的高度偏移 (加上去的位移：負數=往下壓)")]
    public float eyebrowHeightOffset = 0f;
}

public class FaceAnimator : MonoBehaviour
{
    [Header("🧩 五官綁定 (拖曳子物件)")]
    public Transform leftEyeBase;
    public Transform rightEyeBase;
    public Transform leftPupil;
    public Transform rightPupil;
    public Transform mouth;
    public Transform leftEyebrow;
    public Transform rightEyebrow;

    [Header("👀 眼珠動態設定")]
    [Tooltip("打勾：眼珠會看著滑鼠游標 (需透過 UpdateMousePosition 傳入座標)。不勾：眼珠會隨著剛體慣性晃動")]
    public bool lookAtMouse = true;
    [Tooltip("眼珠最多能在眼白裡移動多少距離？")]
    public float pupilMoveRadius = 0.15f;
    [Tooltip("如果沒勾看滑鼠，請綁定玩家的剛體來讀取移動速度")]
    public Rigidbody2D playerRb;

    [Header("🎭 自訂表情清單")]
    public List<FaceExpression> expressions = new List<FaceExpression>();
    [Tooltip("所有動態的平滑過渡速度 (數字越小越像史萊姆，越大越俐落)")]
    public float lerpSpeed = 15f;

    // --- 內部目標數值 (平滑過渡用) ---
    private float targetMouthCurve = 1f;
    private float targetEyeScale = 1f;
    private float targetEyebrowAngle = 0f;
    private float targetEyebrowHeightOffset = 0f;

    // --- 儲存學員在 Editor 中設定好的「初始基礎值」 ---
    private Vector3 baseLeftEyeScale;
    private Vector3 baseRightEyeScale;
    private Vector3 baseMouthScale;
    private Vector2 baseLeftEyebrowPos;
    private Vector2 baseRightEyebrowPos;
    private float baseLeftEyebrowRotZ;
    private float baseRightEyebrowRotZ;

    // 接收從外部傳來的滑鼠世界座標
    private Vector2 currentMouseWorldPos;

    void Awake()
    {
        if (playerRb == null) playerRb = GetComponentInParent<Rigidbody2D>();

        // 📝 遊戲開始時，把學員排版好的大小跟位置「記下來」
        if (leftEyeBase != null) baseLeftEyeScale = leftEyeBase.localScale;
        if (rightEyeBase != null) baseRightEyeScale = rightEyeBase.localScale;
        if (mouth != null) baseMouthScale = mouth.localScale;

        if (leftEyebrow != null)
        {
            baseLeftEyebrowPos = leftEyebrow.localPosition;
            baseLeftEyebrowRotZ = leftEyebrow.localEulerAngles.z;
        }
        if (rightEyebrow != null)
        {
            baseRightEyebrowPos = rightEyebrow.localPosition;
            baseRightEyebrowRotZ = rightEyebrow.localEulerAngles.z;
        }

        // 開局預設套用清單中的第 0 個表情
        if (expressions.Count > 0) SetExpression(expressions[0].expressionName);
    }

    void Update()
    {
        AnimatePupils();
        AnimateExpression();
    }

    // ==========================================
    // 👇 給外部 (例如 PlayerInputController) 呼叫的滑鼠定位接口
    // ==========================================
    public void UpdateMousePosition(Vector2 mousePos)
    {
        currentMouseWorldPos = mousePos;
    }

    // ==========================================
    // 👇 動態邏輯區
    // ==========================================

    private void AnimatePupils()
    {
        Vector2 targetPupilOffset = Vector2.zero;

        if (lookAtMouse)
        {
            // 1. 計算臉部中心點 (世界座標)
            Vector2 faceCenter = (leftEyeBase != null && rightEyeBase != null) ?
                (leftEyeBase.position + rightEyeBase.position) / 2f : (Vector2)transform.position;

            // 2. 算出世界座標下的方向
            Vector2 worldDirection = (currentMouseWorldPos - faceCenter).normalized;

            // 🌟 關鍵修正：將「世界方向」轉換成「臉部的本地方向」！
            // 這樣不管角色怎麼翻轉 (Y軸180度)，眼珠都會看對方向
            Vector3 localDirection = transform.InverseTransformDirection(worldDirection);

            targetPupilOffset = new Vector2(localDirection.x, localDirection.y) * pupilMoveRadius;
        }
        else if (playerRb != null)
        {
            // 物理模式：一樣要把世界的速度轉換成本地方向
            Vector2 worldVelocity = playerRb.linearVelocity;
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

            targetPupilOffset = new Vector2(-localVelocity.x, -localVelocity.y) * 0.05f;
            targetPupilOffset = Vector2.ClampMagnitude(targetPupilOffset, pupilMoveRadius);
        }

        // 平滑移動眼珠
        if (leftPupil != null) leftPupil.localPosition = Vector2.Lerp(leftPupil.localPosition, targetPupilOffset, Time.deltaTime * lerpSpeed);
        if (rightPupil != null) rightPupil.localPosition = Vector2.Lerp(rightPupil.localPosition, targetPupilOffset, Time.deltaTime * lerpSpeed);
    }

    private void AnimateExpression()
    {
        // 1. 嘴巴變形 (以初始 Scale Y 去乘上目標倍率)
        if (mouth != null)
        {
            float targetY = baseMouthScale.y * targetMouthCurve;
            float currentMouthY = Mathf.Lerp(mouth.localScale.y, targetY, Time.deltaTime * lerpSpeed);
            mouth.localScale = new Vector3(baseMouthScale.x, currentMouthY, baseMouthScale.z);
        }

        // 2. 眼睛縮放 (以初始 Scale 去乘上目標倍率)
        if (leftEyeBase != null) leftEyeBase.localScale = Vector3.Lerp(leftEyeBase.localScale, baseLeftEyeScale * targetEyeScale, Time.deltaTime * lerpSpeed);
        if (rightEyeBase != null) rightEyeBase.localScale = Vector3.Lerp(rightEyeBase.localScale, baseRightEyeScale * targetEyeScale, Time.deltaTime * lerpSpeed);

        // 3. 眉毛旋轉與高度下壓 (加上去)
        if (leftEyebrow != null)
        {
            Quaternion targetLeftRot = Quaternion.Euler(0, 0, baseLeftEyebrowRotZ + targetEyebrowAngle);
            leftEyebrow.localRotation = Quaternion.Lerp(leftEyebrow.localRotation, targetLeftRot, Time.deltaTime * lerpSpeed);

            Vector2 targetLeftPos = baseLeftEyebrowPos + new Vector2(0, targetEyebrowHeightOffset);
            leftEyebrow.localPosition = Vector2.Lerp(leftEyebrow.localPosition, targetLeftPos, Time.deltaTime * lerpSpeed);
        }

        if (rightEyebrow != null)
        {
            // 右眉毛的角度要反轉 (對稱)
            Quaternion targetRightRot = Quaternion.Euler(0, 0, baseRightEyebrowRotZ - targetEyebrowAngle);
            rightEyebrow.localRotation = Quaternion.Lerp(rightEyebrow.localRotation, targetRightRot, Time.deltaTime * lerpSpeed);

            Vector2 targetRightPos = baseRightEyebrowPos + new Vector2(0, targetEyebrowHeightOffset);
            rightEyebrow.localPosition = Vector2.Lerp(rightEyebrow.localPosition, targetRightPos, Time.deltaTime * lerpSpeed);
        }
    }

    // ==========================================
    // 👇 給 UnityEvent 呼叫的公開接口
    // ==========================================

    public void SetExpression(string expName)
    {
        foreach (var exp in expressions)
        {
            if (exp.expressionName == expName)
            {
                targetMouthCurve = exp.mouthCurve;
                targetEyeScale = exp.eyeScale;
                targetEyebrowAngle = exp.eyebrowAngle;
                targetEyebrowHeightOffset = exp.eyebrowHeightOffset;
                return;
            }
        }
        Debug.LogWarning($"[{gameObject.name}] 找不到名叫 '{expName}' 的表情！");
    }

    [ContextMenu("▶️ 測試表情：開心 (Happy)")]
    public void SetHappy() { targetMouthCurve = 1f; targetEyeScale = 1.2f; targetEyebrowAngle = 10f; targetEyebrowHeightOffset = 0.1f; }

    [ContextMenu("▶️ 測試表情：難過 (Sad)")]
    public void SetSad() { targetMouthCurve = -1f; targetEyeScale = 0.8f; targetEyebrowAngle = -15f; targetEyebrowHeightOffset = -0.1f; }

    [ContextMenu("▶️ 測試表情：平常心 (Neutral)")]
    public void SetNeutral() { targetMouthCurve = 0.1f; targetEyeScale = 1f; targetEyebrowAngle = 0f; targetEyebrowHeightOffset = 0f; }

    [ContextMenu("▶️ 測試表情：生氣 (Angry)")]
    public void SetAngry() { targetMouthCurve = -0.5f; targetEyeScale = 0.9f; targetEyebrowAngle = -30f; targetEyebrowHeightOffset = -0.2f; }

    [ContextMenu("▶️ 測試表情：驚訝 (Surprised)")]
    public void SetSurprised() { targetMouthCurve = 0f; targetEyeScale = 1.5f; targetEyebrowAngle = 20f; targetEyebrowHeightOffset = 0.3f; }
}