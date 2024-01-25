using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// �x�������p�ÓI�N���X
public static class DelayAction
{
    // �����̕b�����ҋ@���Ă��珈�����s��
    public static Coroutine Play(MonoBehaviour owner, float time, UnityAction action)
        => owner.StartCoroutine(Play(time, action));

    static IEnumerator Play(float time, UnityAction action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }
}
