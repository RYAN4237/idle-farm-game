using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 简单的抽屉式菜单控制脚本
/// </summary>
public class DrawerMenu : MonoBehaviour
{
    [SerializeField] private RectTransform drawerPanel; // 菜单面板
    [SerializeField] private Button toggleButton; // 打开/关闭按钮
    [SerializeField] private Button closeButton; // 菜单内的关闭按钮

    [SerializeField] private float animationDuration = 0.3f; // 动画时长
    [SerializeField] private Vector2 openPosition = Vector2.zero; // 打开时的位置
    [SerializeField] private Vector2 closedPosition = new Vector2(-400, 0); // 关闭时的位置

    private bool isOpen = false;
    private Coroutine animationCoroutine;

    private void Start()
    {
        // 初始化菜单位置（关闭状态）
        drawerPanel.anchoredPosition = closedPosition;

        // 绑定按钮事件
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleDrawer);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDrawer);
    }

    /// <summary>
    /// 切换菜单打开/关闭
    /// </summary>
    public void ToggleDrawer()
    {
        if (isOpen)
            CloseDrawer();
        else
            OpenDrawer();
    }

    /// <summary>
    /// 打开菜单
    /// </summary>
    public void OpenDrawer()
    {
        if (isOpen) return;

        // 如果有正在进行的动画，停止它
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateDrawer(closedPosition, openPosition));
        isOpen = true;
    }

    /// <summary>
    /// 关闭菜单
    /// </summary>
    public void CloseDrawer()
    {
        if (!isOpen) return;

        // 如果有正在进行的动画，停止它
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateDrawer(openPosition, closedPosition));
        isOpen = false;
    }

    /// <summary>
    /// 菜单滑动动画
    /// </summary>
    private IEnumerator AnimateDrawer(Vector2 from, Vector2 to)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / animationDuration);

            // 使用 Lerp 平滑插值
            drawerPanel.anchoredPosition = Vector2.Lerp(from, to, progress);

            yield return null;
        }

        // 确保最后的位置是准确的
        drawerPanel.anchoredPosition = to;
    }
}
