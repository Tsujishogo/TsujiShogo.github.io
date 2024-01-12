using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;


public class UIManager : MonoBehaviour
{
    // ボタン登録
    [SerializeField] Button startButton;
    [SerializeField] Button backButton;
    [SerializeField] Button endButton;
    [SerializeField] GameObject stageSelect;
    [SerializeField] float popSpeed = 0.3f;// メニュー画面のポップアップ速度
    [SerializeField] Button[] stageButton;
　　[SerializeField] TextMeshProUGUI[] score;

    void Start()
    {
        // ボタンのイベント登録
        backButton.onClick.AddListener(() => stageSelect.transform.DOScale(Vector3.zero, popSpeed)
                                                                  .OnComplete(() =>stageSelect.SetActive(false)));
        startButton.onClick.AddListener(() => stageSelect.transform.DOScale(Vector3.one, popSpeed));
        endButton.onClick.AddListener(EndGame);

        // ステージの数だけイベント登録
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
        // ゲームプレイ終了
        UnityEditor.EditorApplication.isPlaying = false;
#else
        //ゲームプレイ終了
        Application.Quit();
#endif
    }

    IEnumerator UpdateScore()
    {
        yield return null;
        //すべてのスコア更新
        for (int i = 1; i <= SceneChanger.instance.stages; i++)
        {
            score[i-1].text = TimeConvert(SceneChanger.instance.GetScore(i));
        }
        SceneChanger.instance.OpenScene();

    }
    private String TimeConvert(int t)//秒数を分と秒に
    {
        var min = t / 60;
        var sec = t % 60;
        string time = min.ToString("00") + ":" + sec.ToString("00");
        return time;
    }


    
}
