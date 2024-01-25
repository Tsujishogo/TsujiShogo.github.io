using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// �t�F�[�h���o�Ǘ��N���X
/// </summary>
public class Fader : MonoBehaviour
{
    [SerializeField] Image _fadeImage;
    [SerializeField] Texture _maskTexture;
    [SerializeField] Material _material;
    [SerializeField, Range(0, 1)] 
    float _fadeRange;  //�l��0�̎��t�F�[�h��

    void Awake()
    {
        _material.SetTexture("_MaskTex", _maskTexture); // �t�F�[�h�pmask�e�N�X�`���K�p
    }

    public async UniTask FadeIn(float time, CancellationToken token)// �t�F�[�h�C������
    {
        // ����time�̎��Ԃ������ăt�F�[�h������
        await DOVirtual
            .Float(1f, 0, time, value =>
            {
                _fadeRange = value;
                _fadeImage.material.SetFloat("_Range", value);
            })
            .WithCancellation(token);

        return;
    }
    
    public async UniTask FadeOut(float time, CancellationToken token)// �t�F�[�h�A�E�g����
    {
        if (_fadeRange > 0) return; // �t�F�[�h���̂�
        // ����time�̎��Ԃ������ăt�F�[�h����
        await DOVirtual
            .Float(0, 1f, time, value =>
            {
                _fadeRange = value;
                _fadeImage.material.SetFloat("_Range", value);
            })
            .WithCancellation(token);


    }
}
