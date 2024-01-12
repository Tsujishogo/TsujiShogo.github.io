using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;


public class UIManager : MonoBehaviour
{
    // �{�^���o�^
    [SerializeField] Button startButton;
    [SerializeField] Button backButton;
    [SerializeField] Button endButton;
    [SerializeField] GameObject stageSelect;
    [SerializeField] float popSpeed = 0.3f;// ���j���[��ʂ̃|�b�v�A�b�v���x
    [SerializeField] Button[] stageButton;
�@�@[SerializeField] TextMeshProUGUI[] score;

    void Start()
    {
        // �{�^���̃C�x���g�o�^
        backButton.onClick.AddListener(() => stageSelect.transform.DOScale(Vector3.zero, popSpeed)
                                                                  .OnComplete(() =>stageSelect.SetActive(false)));
        startButton.onClick.AddListener(() => stageSelect.transform.DOScale(Vector3.one, popSpeed));
        endButton.onClick.AddListener(EndGame);

        // �X�e�[�W�̐������C�x���g�o�^
        for (int i = 0; i < SceneChanger.instance.stages; i++)
        {
            string stage = "stage" + (i + 1);
            stageButton[i].onClick.AddListener(() => SceneChanger.instance.SceneChange(stage));
        }
        StartCoroutine("UpdateScore");
    }

    public void EndGame()
    {
#if UNITY_EDITOR
        // �Q�[���v���C�I��
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //�Q�[���v���C�I��
        Application.Quit();
#endif
    }

    IEnumerator UpdateScore()
    {
        yield return null;
        //���ׂẴX�R�A�X�V
        for (int i = 1; i <= SceneChanger.instance.stages; i++)
        {
            score[i-1].text = TimeConvert(SceneChanger.instance.GetScore(i));
        }
        SceneChanger.instance.OpenScene();

    }
    private String TimeConvert(int t)//�b���𕪂ƕb��
    {
        var min = t / 60;
        var sec = t % 60;
        string time = min.ToString("00") + ":" + sec.ToString("00");
        return time;
    }


    
}
