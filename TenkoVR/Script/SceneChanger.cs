using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
///  �V�[�����ځA����ɔ����t�F�[�h�����Ǘ��N���X
/// </summary>
public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    [SerializeField, Header("Fader�N���X")] Fader fader;
    [SerializeField, Header("�t�F�[�h�����ɂ����鎞��")] float fadeTime = 2f;

    CancellationToken token;
    public bool IsSubCamera { get; set; } = false; // �T�u�J�����̃I���I�t���

void Awake() //�V���O���g�� 
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Destroy���ɃL�����Z�������g�[�N������
        token = this.GetCancellationTokenOnDestroy();
    }

    public async UniTask SceneChange(string SceneName)
    {
        // �V�[���ǂݍ��݊J�n
        var async = SceneManager.LoadSceneAsync(SceneName);
        async.allowSceneActivation = false;

        // �V�[���ǂݍ��݂̐i�����Ď�
        float progress = 0f;
        async.ToUniTask(Progress.Create<float>(n =>
        {
            Debug.Log($"����{n * 100}%");
            progress = n;
        })).Forget();

        // ���[�h��90%�����̊ԑҋ@
        UniTask loadTask = UniTask.WaitWhile (() => progress < 0.9f,cancellationToken: token);

        // ����Ńt�F�[�h����
        UniTask fadeTask = fader.FadeIn(fadeTime, token);

        // �����̏����̊�����҂�
        await UniTask.WhenAll(loadTask, fadeTask); 

        // �V�[���ړ��������ăt�F�[�h�A�E�g
        async.allowSceneActivation = true;
        fader.FadeOut(fadeTime, token).Forget();
    }


#if UNITY_EDITOR
    void Update()
    {
        // �f�o�b�O�p
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("testok");
            SceneChange("iktest");
        }
    }
#endif
}
