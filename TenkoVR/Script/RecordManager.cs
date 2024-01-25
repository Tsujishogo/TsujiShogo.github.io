using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using UniRx;
using UnityEngine.UI;

/// <summary>
/// リプレイ管理クラス
/// </summary>
public class RecordManager : MonoBehaviour
{
    [Header ("動きを記録したいobjectを登録")]
    [SerializeField] List<GameObject> _recordList; // 動きを記録しておくリスト
    
    [SerializeField,Header("記録する間隔")] float _frameInterval = 0.0166666666666f; 
    [SerializeField, Header("リプレイのループ間隔")] float _replayInterval = 0.5f;

    // private変数群
    Dictionary<int, GameObject> _target = new Dictionary<int, GameObject>(); //初期化
    List<int> _objectIDs;
    List<GameObject> _recordingObjects;
    List<FrameData> _frames;
    RecordedObjectsData _recordData;
    RecordedPoolObjectsData _recordPoolData;
    float _time = 0; // 現在のフレーム数を保持する変数
    Enemy _enemy;　// 記憶した弾を発射させるenemy
    int _replayIndex = 0; // リプレイの再生位置
    bool _isRecording = false; // 記録中かどうか
    bool _isLoop = true; // リプレイをループするかどうか
    bool _isReplay = false; // リプレイ再生中かどうか

    public static RecordManager instance; // インスタンス
    public enum PoolObjectType // 記録しておくobjectの種類
    {
        Bullet,
        ElectricalSparks,
        SpawnSmoke,
    }


    // 記録データ格納クラス群
    public class RecordedObjectsData
    {
        public List<FrameData> frames;
    }

    [Serializable]
    public class FrameData
    {
        public float time; // フレーム位置
        public List<ObjectPoseData> objectPoses;
    }

    [Serializable]
    public class ObjectPoseData
    {
        public Vector3 position; // position
        public Quaternion rotation; // rotation
        public int id; // dictionary用key
    }

    // poolオブジェクト用格納class
    public class RecordedPoolObjectsData
    {
        public List<PoolFrameData> poolFrames;
    }

    public class PoolFrameData
    {
        public float playtime; // フレーム位置
        public List<PoolObjectPoseData> poolObjectPoses;
    }

    public class PoolObjectPoseData
    {
        // 状態
        public Vector3 position;
        public Vector3 direction;
        public PoolObjectType type;
    }


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
        _enemy = GetComponent<Enemy>();

        // リスト内のgameobjectをdictionaryに追加
        for (int i = 0; i < _recordList.Count; i++)
        {
            _target.Add(i, _recordList[i]);
        }

        // リプレイを記憶するclassを初期化
        _recordData = new RecordedObjectsData();
        _recordData.frames = new List<FrameData>();
        _recordPoolData = new RecordedPoolObjectsData();
        _recordPoolData.poolFrames = new List<PoolFrameData>();

        // dictionaryのデータをリストに格納
        _objectIDs = _target.Keys.ToList();
        _recordingObjects = _target.Values.ToList();
    }

    void Update()
    {
#if UNITY_EDITOR
        // デバッグ用
        if (Input.GetKey(KeyCode.R))
        {
            StartRecording();
        }
        if (Input.GetKey(KeyCode.P))
        {
            StartReplay();
        }
#endif
    }

    public async void StartRecording() //録画開始処理
    {
        _isRecording = true;
        Debug.Log("記録開始");
        RecordTask().Forget();　//記録用コルーチン起動
    }

    public void StopRecording() // 録画停止用処理
    {
        _isRecording = false;
    }

    public async void StartReplay() // リプレイ再生用関数
    {
        _isRecording = false; // 録画を停止
        //StopCoroutine(recordCoroutine); // 録画用コルーチンを停止
        foreach(GameObject record in _recordList) // 記録対象objectを自身の子にしてリプレイに動きが干渉しないようにする
        {
            record.transform.parent = transform;
        }
        _isReplay = true;
        Debug.Log("リプレイ開始");
        ReplayTask().Forget(); // 再生開始
    }

    async UniTask RecordTask() // 記録処理
    {
        _time = 0; // 現在フレームを初期化
        while (true)
        {
            if (_isRecording)
            {
                // 現在フレームの情報を記憶するリストを初期化
                FrameData frame = new FrameData();
                frame.time = _time;
                frame.objectPoses = new List<ObjectPoseData>();

                // 同期させるために都度記憶リストも初期化
                PoolFrameData poolFrame = new PoolFrameData();
                poolFrame.playtime = _time;
                poolFrame.poolObjectPoses = new List<PoolObjectPoseData>();
                _recordPoolData.poolFrames.Add(poolFrame);

                for (int i = 0; i < _recordingObjects.Count; i++)
                {
                    // 記録対象object毎に状態を記録
                    ObjectPoseData pose = new ObjectPoseData();
                    pose.position = _recordingObjects[i].transform.position;
                    pose.rotation = _recordingObjects[i].transform.rotation;
                    pose.id = _objectIDs[i];
                    frame.objectPoses.Add(pose);
                }
                _recordData.frames.Add(frame);
            }
            // frameIntervarl毎に繰り返す
            await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval));
            _time += _frameInterval;
        }
    }
    async UniTask ReplayTask()
    {
        while (_isLoop)
        {
            while (_replayIndex < _recordData.frames.Count) // 格納されているフレーム数の分だけ処理
            {
                // 現在フレームのデータをとりだす
                FrameData frame = _recordData.frames[_replayIndex];
                for (int i = 0; i < frame.objectPoses.Count; i++) // 録画対象のobject分それぞれ処理
                {
                    ObjectPoseData pose = frame.objectPoses[i];
                    GameObject obj = _target[pose.id];

                    // フレームから取得した pose に基づいてオブジェクトの位置と回転を反映
                    obj.transform.position = pose.position;
                    obj.transform.rotation = pose.rotation;
                }

                // プールobject用の再生も同様に
                PoolFrameData poolFrame = _recordPoolData.poolFrames[_replayIndex];
                for (int i = 0; i < poolFrame.poolObjectPoses.Count; i++)
                {
                    PoolObjectPoseData poolPose = poolFrame.poolObjectPoses[i];
                    // 弾の場合とパーティクルでそれぞれ処理を分ける
                    switch(poolPose.type)
                    {
                        case PoolObjectType.Bullet: // 弾
                            _enemy.RecordedShot(poolPose.position, poolPose.direction);
                            break;
                        case PoolObjectType.ElectricalSparks: // パーティクル
                            ParticleManager.PlayParticle(ParticleManager.ParticleName.ElectricalSparks, poolPose.position);
                            break;
                        case PoolObjectType.SpawnSmoke: //パーティクル
                            ParticleManager.PlayParticle(ParticleManager.ParticleName.SpawnSmoke, poolPose.position);
                            break;
                    }
                }
                //１フレーム待って次のフレームへ
                _replayIndex++;
                await UniTask.Delay(TimeSpan.FromSeconds(_frameInterval));
            }
            if (!_isLoop)
            {
                break;
            }

            _replayIndex = 0; // 再生位置初期化
            await UniTask.Delay(TimeSpan.FromSeconds(_replayInterval),true);  // リプレイループ
        }
    }
 
    // 弾、パーティクル記憶関数
    public void PoolObjectRecorder(Vector3 pos, Vector3 dir, PoolObjectType type)
    {
        if (_isRecording)
        {
            // objectの状態を記録
            PoolObjectPoseData poolPose = new PoolObjectPoseData();
            poolPose.position = pos;
            poolPose.direction = dir;
            poolPose.type = type;

            // 現在のフレーム数の位置に記録したデータを格納
            _recordPoolData.poolFrames[_recordPoolData.poolFrames.Count - 1].poolObjectPoses.Add(poolPose);
        }
    }
}
