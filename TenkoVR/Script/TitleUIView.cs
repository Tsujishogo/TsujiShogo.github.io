using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using UniRx;
using System.Linq;

public class TitleUIView: MonoBehaviour
{
    [Header("それぞれのButton,UIを登録")]
    [SerializeField] Button _exitButton;
    [SerializeField] Button _stageButton;
    [SerializeField] Button _easyButton;
    [SerializeField] Button _normalButton;
    [SerializeField] Button _hardButton;
    [SerializeField] Button _manualButton;
    [SerializeField] GameObject _manualUI;
    [SerializeField] GameObject _stageUI;
    [SerializeField] GameObject _mainUI;
    [SerializeField] List<Button> _backButtons;
    [SerializeField] Toggle _SubCameraToggle;

    [SerializeField,Header("UIのスケール変更に掛ける時間")] float _scaleDuration = 0.2f; 

    // pribate変数
    List<GameObject> _previousUIs = new List<GameObject>(); // 今までに開いたUIを保持しておくリスト
    GameObject _currentUI; // 現在の開いてるUI

    void Start()
    {
        var token = this.GetCancellationTokenOnDestroy(); // Destroy時にキャンセルされるtoken生成
        OpenUI(_mainUI, token); // メインUIを表示
        _currentUI = _mainUI;

        // それぞれのButtonに処理を登録
        _exitButton.OnClickAsObservable().Subscribe(_ => EndGame());
        _stageButton.OnClickAsObservable().Subscribe(_ => SwitchUI(_stageUI, token).Forget());
        _manualButton.OnClickAsObservable().Subscribe(_ => SwitchUI(_manualUI, token).Forget());
        _easyButton.OnClickAsObservable().Subscribe(_ => SceneChanger.instance.SceneChange("EasyStage").Forget());
        _normalButton.OnClickAsObservable().Subscribe(_ => SceneChanger.instance.SceneChange("NormalStage").Forget()) ;
        _hardButton.OnClickAsObservable().Subscribe(_ => SceneChanger.instance.SceneChange("HardStage").Forget());
        _SubCameraToggle.OnValueChangedAsObservable().Subscribe(value => SceneChanger.instance.IsSubCamera = value);
        foreach (Button backButton in _backButtons)
        {
            backButton.OnClickAsObservable().Subscribe(_ => BackUI(token).Forget());
        }
    }

    async UniTask OpenUI(GameObject ui, CancellationToken token) // 引数のUIをアクティブにしてスケールを大きく
    {
        ui.SetActive(true);
        await ui.transform.DOScale(Vector3.one, _scaleDuration);        
    }

    async UniTask CloseUI(GameObject ui, CancellationToken token) // 引数のUIを小さくして非アクティブに
    {
        await ui.transform.DOScale(Vector3.zero, _scaleDuration);
        ui.SetActive(false);
    }

    async UniTask BackUI(CancellationToken token) // 一つ前のUIを開く処理
    {
        await CloseUI(_currentUI, token); // 現在のUIを閉じる
        OpenUI(_previousUIs.Last(), token).Forget(); // ひとつ前のUIを開く 
        _currentUI = _previousUIs.Last(); // // 開いてるUIを保持
        _previousUIs.Remove(_previousUIs.Last()); // 履歴の最後を削除
    }
    async UniTask SwitchUI(GameObject nextUI, CancellationToken token) // 次のUIを開く処理
    {
        // 現在のUIを閉じて次のUIを開く
        await CloseUI(_currentUI, token);
        await OpenUI(nextUI, token);
        _previousUIs.Add(_currentUI); // ひとつ前のUIをリストを記憶
        _currentUI = nextUI; // 開いてるUIを保持
    }

    void EndGame()
    {
#if UNITY_EDITOR
        // ゲーム終了
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
