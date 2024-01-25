using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
///  シーン推移、それに伴うフェード処理管理クラス
/// </summary>
public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    [SerializeField, Header("Faderクラス")] Fader fader;
    [SerializeField, Header("フェード処理にかける時間")] float fadeTime = 2f;

    CancellationToken token;
    public bool IsSubCamera { get; set; } = false; // サブカメラのオンオフ状態

void Awake() //シングルトン 
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
        // Destroy時にキャンセルされるトークン生成
        token = this.GetCancellationTokenOnDestroy();
    }

    public async UniTask SceneChange(string SceneName)
    {
        // シーン読み込み開始
        var async = SceneManager.LoadSceneAsync(SceneName);
        async.allowSceneActivation = false;

        // シーン読み込みの進捗を監視
        float progress = 0f;
        async.ToUniTask(Progress.Create<float>(n =>
        {
            Debug.Log($"現在{n * 100}%");
            progress = n;
        })).Forget();

        // ロードが90%未満の間待機
        UniTask loadTask = UniTask.WaitWhile (() => progress < 0.9f,cancellationToken: token);

        // 並列でフェード処理
        UniTask fadeTask = fader.FadeIn(fadeTime, token);

        // 両方の処理の完了を待つ
        await UniTask.WhenAll(loadTask, fadeTask); 

        // シーン移動を許可してフェードアウト
        async.allowSceneActivation = true;
        fader.FadeOut(fadeTime, token).Forget();
    }


#if UNITY_EDITOR
    void Update()
    {
        // デバッグ用
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("testok");
            SceneChange("iktest");
        }
    }
#endif
}
