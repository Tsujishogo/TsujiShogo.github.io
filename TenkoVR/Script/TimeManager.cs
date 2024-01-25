using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

/// <summary>
/// スロー演出管理クラス
/// </summary>
public class TimeManager : MonoBehaviour
{
    [Header("スローフラグがなくなってもスローを保持しておく時間")]
    [SerializeField] float _timeKeepCount = 0.1f;
    [Header("スロー時のtimescaleの値")]
    [SerializeField] float _slowSpeed = 0.05f;
    [Header("スローになりきるまでの時間")]
    [SerializeField] float _slowDuration = 0.5f; 
    [SerializeField] PostProcessVolume _postProcess; // エフェクトをかけるポストプロセス
    [SerializeField] AudioEffecter _audioEffecter;

    // private変数
    float _toggleTimer; // スロー判定がなくなっても一定時間スローを維持しておく時間
    bool _isSlow = false; // スロー判定
    float _currentSpeed; // 現在のTimeScaleの値
    Tweener _tweener; 
    Vignette _vignette;


    void Start()
    {
        // 変数の初期化とポストプロセスのvignetteを取得
        _currentSpeed = 1f;
        _vignette = _postProcess.profile.GetSetting<Vignette>();
    }

    //update関数はtimescaleの影響を受ける
    void Update()
    {
        // timeKeepCountが0になるまでスローを保持してからスロー解除
        if (_toggleTimer > 0)
        {
            _toggleTimer -= Time.deltaTime;
        }
        else if(_toggleTimer <= 0 && _isSlow)
        {
            SlowToggle(false);
        }
    }

    public void SlowChecker()// スロー処理発火の公開関数
    {
        SlowToggle(true);
        _toggleTimer = _timeKeepCount;

    }
    void SlowToggle(bool slow)
    {
        
        if(slow && !_isSlow) // 遅くする
        {
            _tweener.Kill(); // 速度を戻す処理を中断
            _isSlow = true;
            //ゆっくり速度変更
            _tweener = DOVirtual.Float(_currentSpeed, _slowSpeed, _slowDuration, value => 
               {
                   Time.timeScale = value;
                   _currentSpeed = value;
                   // vignetteの値を変更
                   _vignette.intensity.Override(1.0f - value);
                   AudioSlower(value);
               });
        }
        else if(!slow && _isSlow) // 速度を戻す
        {
            _tweener.Kill();　// 速度をおそく処理を中断
            _isSlow = false;
            //ゆっくり速度変更
            _tweener = DOVirtual.Float(_currentSpeed, 1f, _slowDuration, value =>
            {
                Time.timeScale = value;
                _currentSpeed = value;
                // vignetteの値を変更
                _vignette.intensity.Override(1.0f - value);
                AudioSlower(value);
            });
        }

        void AudioSlower(float slowValue)// AudioManagerにタイムスケールの値を送り効果音にも処理を行う
        {
            _audioEffecter.SetSlowEffect(slowValue);
        }
    }
}
