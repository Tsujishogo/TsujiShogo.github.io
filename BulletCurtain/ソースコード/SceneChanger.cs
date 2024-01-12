using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// シーン推移、スコア管理クラス
public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;
    [SerializeField] Fader fade;　// フェード用キャンパスobject
    [SerializeField] float FadeTime = 1f; // シーン推移のフェードにかける時間
    [SerializeField] public int stages = 3; // 実装するステージの数
    [SerializeField] int maxScore = 5999; //初期スコア

     int[] score = new int[0]; // スコア保存しておく配列

    void Awake() // シングルトン
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
    }

    private void Start()
    {
        // ステージの数だけスコアを初期化
        //score = new int[stages];
        Array.Resize(ref score, stages);
        for (int i = 0; i < stages; i++)
        {
            score[i] = maxScore;
        }

    }

    public void SetScore(int stageNo, int newScore)
    {
        // 引数のステージのスコア更新できてたら更新
        if(score[stageNo-1] > newScore)
        {
           score[stageNo-1] = newScore;
        }
    }

    public int GetScore(int stageNo)
    {
        // 引数のステージのスコアを返す
        return score[stageNo-1];
    }

    public void SceneChange(string SceneName)
    {    
        // 引数で指定したシーンへ遷移
        StartCoroutine(LoadScene(SceneName,FadeTime));
    }


    IEnumerator LoadScene(string SceneName,float time)
    {
        var async = SceneManager.LoadSceneAsync(SceneName); //非同期でシーンロード

        async.allowSceneActivation = false;

        ParticleManager.ResetList();//パーティクルのpool初期化

        if (fade != null)// フェード処理
        {
            fade.FadeIn(time);
        }
        yield return null;

        yield return new WaitForSeconds(time);// フェードにかかる時間だけ待つ

        
        {
#if UNITY_EDITOR
            while(async.progress < 0.9f)// デバッグ用ロード進行度表示
            Debug.Log("シーンロード進行度" + async.progress);
#endif
        }

        async.allowSceneActivation = true;//シーンロード許可

        yield return async;//シーン移動

        SceneManager.LoadSceneAsync(SceneName);
　　　　OpenScene();//ロード後フェード解除
    }

    //シーンロード後のフェード解除処理
    public void OpenScene()
    {
        fade.FadeOut(FadeTime);
    }


#if UNITY_EDITOR

    //デバッグ用
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("load");
            SceneChange("iktest");
        }
    }
#endif
}
