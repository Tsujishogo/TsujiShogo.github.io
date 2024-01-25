using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// フェード演出管理クラス
/// </summary>
public class Fader : MonoBehaviour
{
    [SerializeField] Image _fadeImage;
    [SerializeField] Texture _maskTexture;
    [SerializeField] Material _material;
    [SerializeField, Range(0, 1)] 
    float _fadeRange;  //値が0の時フェード中

    void Awake()
    {
        _material.SetTexture("_MaskTex", _maskTexture); // フェード用maskテクスチャ適用
    }

    public async UniTask FadeIn(float time, CancellationToken token)// フェードイン処理
    {
        // 引数timeの時間をかけてフェードさせる
        await DOVirtual
            .Float(1f, 0, time, value =>
            {
                _fadeRange = value;
                _fadeImage.material.SetFloat("_Range", value);
            })
            .WithCancellation(token);

        return;
    }
    
    public async UniTask FadeOut(float time, CancellationToken token)// フェードアウト処理
    {
        if (_fadeRange > 0) return; // フェード中のみ
        // 引数timeの時間をかけてフェード解除
        await DOVirtual
            .Float(0, 1f, time, value =>
            {
                _fadeRange = value;
                _fadeImage.material.SetFloat("_Range", value);
            })
            .WithCancellation(token);


    }
}
