using System.Collections;
using System;
using UnityEngine;

/// <summary>
///  ゲームステージ全体を管理するクラス
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [Header("ゲーム開始してから説明UIが出るまでの時間")]
    [SerializeField] float _showUITime = 5f;
    [Header("クリア後リプレイ再生が開始されるまでの時間")]
    [SerializeField] float _replayPlayTime = 1f;
    [Header("プレイヤーの狙われる位置（SnipePoint")]
    [SerializeField] Transform _player;
    [Header("UIのみ表示するCamera")]
    [SerializeField] Camera _interactionCamera;
    [Header("デスクトップ用サブCamera")]
    [SerializeField] GameObject _subCamera;
    [Header("刀に誘導するガイドライン")] 
    [Header("リザルトUI")]
    [SerializeField] GameObject _resultUI;
    [Header("ManagerClass群")]
    [SerializeField] EnemyManager _enemyManager;
    [SerializeField] TimeManager _timeManager;

    public bool IsShowUI { get; private set; }


    void Awake()
    {
        if (instance == null)// シングルトン
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        // UI表示用のカメラをアクティブに
        DelayAction.Play(this, _showUITime, () => 
        {
            _interactionCamera.enabled = true;
            IsShowUI = true;
        });

        // サブカメラを表示可否を反映
        _subCamera.SetActive(SceneChanger.instance.IsSubCamera); 
    }

     public void GameStart()
     {
        // UIを消して敵のリスポーン開始
        _interactionCamera.enabled = false;
        _enemyManager.EnemySpawn();
        // リプレイの記録開始
        RecordManager.instance.StartRecording(); 
     }

    public void StageClear()
    {
        //クリア時処理
        DelayAction.Play(this, _showUITime, () =>
        {
            Debug.Log("clear");
            ExplanationUI(true); // UI、コントローラー表示
            _resultUI.SetActive(true); // リザルト画面表示
            DelayAction.Play(this, _replayPlayTime, () => RecordManager.instance.StartReplay());// リプレイ再生
        });
    }

    void ExplanationUI(bool value) // UI表示用カメラONOFF
    {
        _interactionCamera.enabled = value;
    }

    public Transform GetPlayerTransform() // プレイヤーのtranformを返す
    {
        return _player;
    }

    public void RayHit() // スロー処理 
    {
        _timeManager.SlowChecker();
    }
}
