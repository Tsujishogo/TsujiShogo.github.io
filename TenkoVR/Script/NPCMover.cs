using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;

/// <summary>
/// タイトル画面用NPC動作クラス
/// </summary>

public class NPCMover : MonoBehaviour
{
    [Header("まばたきの最短間隔")]
    [SerializeField] float _blinkMinTime;
    [Header("まばたきの最長間隔")]
    [SerializeField] float _blinkMaxTime;
    [Header("まばたきの速度")]
    [SerializeField] float _blinkSpeed = 0.1f; 
    [Header("表情変更時にもまばたきをさせるかどうか")]
    [SerializeField] bool _isForceBlink = false; 
    [Header("AnimatorのblinkLayerの位置")]
    [SerializeField] int _blinkLayer = 3; 
    [Header("移動先の位置リスト")]
    [SerializeField] List<Transform> _wayPoint; 
    [Header("歩く速度")]
    [SerializeField] float _walkSpeed; 
    [Header("走る速度")]
    [SerializeField] float _runSpeed; 
    [Header("移動の加速度")]
    [SerializeField] float _acceleration = 1f; 
    [Header("回転に掛ける時間")]
    [SerializeField] float _rotateDuration = 1f; 
    [Header("wayPoint内での初期位置")]
    [SerializeField] int _startPos;
    [Header("移動後の待機時間")]
    [SerializeField] float _actionWaitTime = 2f; 
    [Header("移動後到達判定のしきい値")]
    [SerializeField] float _arrivalThreshold = 0.1f;
    [System.Serializable]
    struct ActionList
    {
        [Header("行動リスト（ランダムで選択）")]
        List<Transform> _movePosition;　　// 移動先
        float _moveSpeed;　// 移動にかける時間
        int _actionPattern; //行動のの種類
    }
    [Header("行動を登録")]
    [SerializeField] ActionList[] _actionList;

    // private変数
    float _blinkTimer; // まばたきカウント用変数
    float _currentSpeed = 0;
    int _currentPos = 0; // movePositionの現在位置
    CancellationTokenSource _cancellationTokenSource;
    Animator _animator;


    float MoveSpeed //animatorの移動速度を制御するプロパティ
    {
        set => _animator.SetFloat("MoveSpeed", value); 
    }

    float BlinkWeight // animatorのまばたきを制御するプロパティ
    {
        set =>_animator.SetFloat("BlinkWeight", value); 
    }

    int Facial // animatorの表情の制御用プロパティ
    {
        get => _animator.GetInteger("Facial");
        set =>_animator.SetInteger("Facial", value); 
    }


    void Awake()
    {
        // component取得
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource(); // キャンセルトークン生成
        CancellationToken token = this.GetCancellationTokenOnDestroy();
        _blinkTimer = Random.Range(_blinkMinTime, _blinkMaxTime); //初回のまばたきするまでの時間を計算
        transform.position = _wayPoint[_startPos].position; // 初期位置へ移動して現在位置を初期化

        Blink(token).Forget(); // まばたき処理
        Move(token).Forget(); // 移動処理
    }

    void OnDestroy()
    {
        // Destroy時にキャンセルする
        _cancellationTokenSource?.Cancel();
    }

    // まばたき処理
    async UniTaskVoid Blink(CancellationToken token)
    {
        while(!token.IsCancellationRequested) // キャンセルされてなければ繰り返す
        {
            // blinkMin,Maxの変数の時間の間で繰り返す
            await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(_blinkMinTime, _blinkMaxTime)),cancellationToken : token);
            // デフォルト表情のときだけまばたきさせる。もしくはisForceBlinkがtrueの場合は強制まばたき
            if (Facial == 0 || !_isForceBlink)
            {
                _animator.SetLayerWeight(_blinkLayer, 1); // まばたき処理のレイヤーを有効にする
                // blinkSpeedの速度で目を閉じて目を開ける
                await DOVirtual.Float(0, 1, _blinkSpeed, value =>
                {
                    BlinkWeight = value;
                }).WithCancellation(token);
                
                await DOVirtual.Float(1, 0, _blinkSpeed, value =>
                {
                    BlinkWeight = value;
                }).WithCancellation(token); ;
                _animator.SetLayerWeight(_blinkLayer, 0);
            }
        } 
    }

    // 移動処理
    async UniTask Move(CancellationToken token)
    {
        // ランダムに次の行先を今いる場所以外で決める
        int ran;
        do { 
            ran = Random.Range(0, _wayPoint.Count); 
        } while (_currentPos == ran);
        _currentPos = ran;

        // 移動速度もランダムで決定
        float maxSpeed = Random.value < 0.5f ? _walkSpeed : _runSpeed;
        Vector3 dir = (_wayPoint[ran].position - transform.position).normalized; 

        Debug.Log("次は" + _currentPos + "に" + maxSpeed + "の速度で行くよ");
        // 行き先の方を向く
        transform.DOLookAt(_wayPoint[ran].position, _rotateDuration).SetEase(Ease.OutCubic);

        IDisposable disposable = this.UpdateAsObservable() // 移動処理
            .Subscribe(_ =>
            {
                // 最大速度まで加速しながら移動
                _currentSpeed = Mathf.Clamp(_currentSpeed + _acceleration * Time.deltaTime, 0f, maxSpeed);
                transform.position +=  _currentSpeed * Time.deltaTime * dir;
                // animatorに速度を反映
                MoveSpeed = _currentSpeed;
            });
        //tokenでキャンセルされたらsubscribeも一緒に止める
        token.Register(() => disposable.Dispose());

        // 比較用サイズ
        float sqrthreshold = _arrivalThreshold * _arrivalThreshold;

        //目的地につくまで待機
        await UniTask.WaitUntil(() =>
        {
            return (_wayPoint[ran].position - transform.position).sqrMagnitude <= sqrthreshold;
            //return Vector3.Distance(transform.position, _wayPoint[ran].position) <= _arrivalThreshold;
        }, cancellationToken: token);

        // 停止
        _currentSpeed = 0;
        MoveSpeed = _currentSpeed;
        disposable.Dispose(); 

        // 移動完了したら次の行動
        await UniTask.Delay(TimeSpan.FromSeconds(_actionWaitTime), cancellationToken: token);
        Move(token).Forget();
    }
}
