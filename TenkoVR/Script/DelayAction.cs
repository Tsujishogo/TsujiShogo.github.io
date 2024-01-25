using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// 遅延処理用静的クラス
public static class DelayAction
{
    // 引数の秒数分待機してから処理を行う
    public static Coroutine Play(MonoBehaviour owner, float time, UnityAction action)
        => owner.StartCoroutine(Play(time, action));

    static IEnumerator Play(float time, UnityAction action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }
}
