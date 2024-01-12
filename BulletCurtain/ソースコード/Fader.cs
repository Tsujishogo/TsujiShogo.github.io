using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    [SerializeField] Image fadeImage;
    [SerializeField] Texture maskTexture;
    [SerializeField] Material material;
    [SerializeField, Range(0, 1)] 
    private float fadeRange;  //�l��0�̎��t�F�[�h��

    private void Awake()
    {
        material.SetTexture("_MaskTex", maskTexture); // �t�F�[�h�p�e�N�X�`���K�p
    }

    public void FadeIn(float time)// �t�F�[�h�C������
    {
        // ����time�̎��Ԃ������ăt�F�[�h������
        DOVirtual.Float(1f, 0, time, value =>
        {
            fadeRange =  value;
            fadeImage.material.SetFloat("_Range",  value);
        });
    }

    
    public void FadeOut(float time)// �t�F�[�h�A�E�g����
    {
        if(fadeRange == 0)// �t�F�[�h���̂�
        {
            // ����time�̎��Ԃ������ăt�F�[�h����
            DOVirtual.Float(0, 1f, time, value =>
            {
                fadeRange = value;
                fadeImage.material.SetFloat("_Range", value);
            });
        }
    }

}
