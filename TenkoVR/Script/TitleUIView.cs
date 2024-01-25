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
    [Header("���ꂼ���Button,UI��o�^")]
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

    [SerializeField,Header("UI�̃X�P�[���ύX�Ɋ|���鎞��")] float _scaleDuration = 0.2f; 

    // pribate�ϐ�
    List<GameObject> _previousUIs = new List<GameObject>(); // ���܂łɊJ����UI��ێ����Ă������X�g
    GameObject _currentUI; // ���݂̊J���Ă�UI

    void Start()
    {
        var token = this.GetCancellationTokenOnDestroy(); // Destroy���ɃL�����Z�������token����
        OpenUI(_mainUI, token); // ���C��UI��\��
        _currentUI = _mainUI;

        // ���ꂼ���Button�ɏ�����o�^
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

    async UniTask OpenUI(GameObject ui, CancellationToken token) // ������UI���A�N�e�B�u�ɂ��ăX�P�[����傫��
    {
        ui.SetActive(true);
        await ui.transform.DOScale(Vector3.one, _scaleDuration);        
    }

    async UniTask CloseUI(GameObject ui, CancellationToken token) // ������UI�����������Ĕ�A�N�e�B�u��
    {
        await ui.transform.DOScale(Vector3.zero, _scaleDuration);
        ui.SetActive(false);
    }

    async UniTask BackUI(CancellationToken token) // ��O��UI���J������
    {
        await CloseUI(_currentUI, token); // ���݂�UI�����
        OpenUI(_previousUIs.Last(), token).Forget(); // �ЂƂO��UI���J�� 
        _currentUI = _previousUIs.Last(); // // �J���Ă�UI��ێ�
        _previousUIs.Remove(_previousUIs.Last()); // �����̍Ō���폜
    }
    async UniTask SwitchUI(GameObject nextUI, CancellationToken token) // ����UI���J������
    {
        // ���݂�UI����Ď���UI���J��
        await CloseUI(_currentUI, token);
        await OpenUI(nextUI, token);
        _previousUIs.Add(_currentUI); // �ЂƂO��UI�����X�g���L��
        _currentUI = nextUI; // �J���Ă�UI��ێ�
    }

    void EndGame()
    {
#if UNITY_EDITOR
        // �Q�[���I��
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
