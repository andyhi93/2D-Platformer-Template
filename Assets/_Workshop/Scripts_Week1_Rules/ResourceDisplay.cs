using UnityEngine;
using TMPro; // 需要引入 TextMeshPro 命名空間

[RequireComponent(typeof(TextMeshProUGUI))]
public class ResourceDisplay : MonoBehaviour
{
    [Tooltip("顯示在數字前面的前綴文字，例如 '金幣: ' 或 'HP: '")]
    public string prefix = "";

    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// 公開方法：接收 ResourceManager 傳來的最新整數並更新介面
    /// </summary>
    public void UpdateDisplay(int newValue)
    {
        textComponent.text = prefix + newValue.ToString();
    }
}