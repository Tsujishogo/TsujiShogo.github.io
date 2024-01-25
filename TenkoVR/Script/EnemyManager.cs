using UnityEngine;
using System.Linq;
using System;
using UniRx;
using Cysharp.Threading.Tasks;

/// <summary>
/// 敵キャラ生成を管理するクラス
/// </summary>
public class EnemyManager : ObjectPoolMonoBehaviour<EnemyManager, Bullet>
{
    [Header("敵prefabは初期状態で非アクティブにしておくこと")]
    [Header("敵prefab")]
    [SerializeField] GameObject[] _enemyList; 
    [Header("TimeManager")]
    [SerializeField] TimeManager _timeManager;

    int _enemyCount; // 敵の残数カウント用変数

    // 敵の出現タイミング、位置、種類をinspectorから指定
    [System.Serializable]
    struct SpawnObject
    {
        public float _timing;        //出現タイミング（秒）
        public Vector3 _spawnPoint;　// 出現位置
        public int _enemyType;       //敵の種類
    }
    [Header("敵の出現時間(秒)、位置、種類を登録")]
    [Header("Timingはゲーム開始から経過時間")]
    [Header("EnemyTypeはenemyListに登録しているPrefab(先頭を呼び出す場合は0)")]
    [SerializeField] SpawnObject[] _spawnerList;


    void Start()
    {
        InitiarizePool(); // プール初期化

        _timeManager = GetComponent<TimeManager>();
        // 敵の残数を初期化
        _enemyCount = _spawnerList.Length;
        // 出現タイミング順にソートしておく
        _spawnerList = _spawnerList.OrderBy(enemy => enemy._timing).ToArray();
    }

    // false状態で呼び出すようにoverride
    public override Bullet Rent()
    {
        Bullet t = base.Rent();
        t.gameObject.SetActive(false);
        return t;
    }

    // 敵prefabを生成、出現タイミングでactive化
    public void EnemySpawn() 
    {
        foreach (var spawn in _spawnerList)
        {
            // SpawnerListに登録された敵を生成
            GameObject enemyObj = Instantiate(_enemyList[spawn._enemyType], spawn._spawnPoint, _enemyList[spawn._enemyType].transform.rotation);
            enemyObj.SetActive(false);

            Observable.Timer(TimeSpan.FromSeconds(spawn._timing))// Timing（秒）後に敵をactiveに
                .Subscribe(_ =>
                {
                    enemyObj.SetActive(true); // 敵をactiveに
                    ParticleManager.PlayParticle(ParticleManager.ParticleName.SpawnSmoke, spawn._spawnPoint); // スポーンパーティクル再生

                    // enemyの各イベントを購読して取得したら処理を走らせる
                    if (enemyObj.TryGetComponent<Enemy>(out Enemy enemy))
                    {
                        enemy.OndeadAsync.Subscribe(_ => OnEnemyDead());
                        enemy.OnSlowJudge.Subscribe(_ => _timeManager.SlowChecker());
                    }
                })
                .AddTo(this);
        }
    }

    // 敵の残数管理関数
    void OnEnemyDead() 
    {
        _enemyCount--;
        if (_enemyCount <= 0) // 敵の残数が0になったらステージクリア処理
        {
            StageManager.instance.StageClear();
        }
    }

}
