using UnityEngine;

/// <summary>
/// 观察者组件基类：统一管理启用/禁用时的订阅生命周期。
/// 派生类在 SubscribeEvents / UnsubscribeEvents 中分别订阅和取消订阅。
/// 不要在 Update 中重复订阅事件。
/// </summary>
public abstract class WuzhongEventObserver : MonoBehaviour
{
    private bool subscribed;

    protected virtual void OnEnable()
    {
        SubscribeOnce();
    }

    protected virtual void OnDisable()
    {
        UnsubscribeOnce();
    }

    /// <summary>订阅《雾钟》事件。派生类在这里使用 +=。</summary>
    protected abstract void SubscribeEvents();

    /// <summary>取消订阅《雾钟》事件。必须与 SubscribeEvents 一一对应。</summary>
    protected abstract void UnsubscribeEvents();

    private void SubscribeOnce()
    {
        if (subscribed)
            return;

        SubscribeEvents();
        subscribed = true;
    }

    private void UnsubscribeOnce()
    {
        if (!subscribed)
            return;

        UnsubscribeEvents();
        subscribed = false;
    }
}
