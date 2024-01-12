using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    [SerializeField] Image fadeImage;
    [SerializeField] Texture maskTexture;
    [SerializeField] Material material;
    [SerializeField, Range(0, 1)] 
    private float fadeRange;  //値が0の時フェード中

    private void Awake()
    {
        material.SetTexture("_MaskTex", maskTexture); // フェード用テクスチャ適用
    }

    public void FadeIn(float time)// フェードイン処理
    {
        // 引数timeの時間をかけてフェードさせる
        DOVirtual.Float(1f, 0, time, value =>
        {
            fadeRange =  value;
            fadeImage.material.SetFloat("_Range",  value);
        });
    }

    
    public void FadeOut(float time)// フェードアウト処理
    {
        if(fadeRange == 0)// フェード中のみ
        {
            // 引数timeの時間をかけてフェード解除
            DOVirtual.Float(0, 1f, time, value =>
            {
                fadeRange = value;
                fadeImage.material.SetFloat("_Range", value);
            });
        }
    }

}
