using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// �V�[�����ځA�X�R�A�Ǘ��N���X
public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;
    [SerializeField] Fader fade;�@// �t�F�[�h�p�L�����p�Xobject
    [SerializeField] float FadeTime = 1f; // �V�[�����ڂ̃t�F�[�h�ɂ����鎞��
    [SerializeField] public int stages = 3; // ��������X�e�[�W�̐�
    [SerializeField] int maxScore = 5999; //�����X�R�A

     int[] score = new int[0]; // �X�R�A�ۑ����Ă����z��

    void Awake() // �V���O���g��
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
        // �X�e�[�W�̐������X�R�A��������
        //score = new int[stages];
        Array.Resize(ref score, stages);
        for (int i = 0; i < stages; i++)
        {
            score[i] = maxScore;
        }

    }

    public void SetScore(int stageNo, int newScore)
    {
        // �����̃X�e�[�W�̃X�R�A�X�V�ł��Ă���X�V
        if(score[stageNo-1] > newScore)
        {
           score[stageNo-1] = newScore;
        }
    }

    public int GetScore(int stageNo)
    {
        // �����̃X�e�[�W�̃X�R�A��Ԃ�
        return score[stageNo-1];
    }

    public void SceneChange(string SceneName)
    {    
        // �����Ŏw�肵���V�[���֑J��
        StartCoroutine(LoadScene(SceneName,FadeTime));
    }


    IEnumerator LoadScene(string SceneName,float time)
    {
        var async = SceneManager.LoadSceneAsync(SceneName); //�񓯊��ŃV�[�����[�h

        async.allowSceneActivation = false;

        ParticleManager.ResetList();//�p�[�e�B�N����pool������

        if (fade != null)// �t�F�[�h����
        {
            fade.FadeIn(time);
        }
        yield return null;

        yield return new WaitForSeconds(time);// �t�F�[�h�ɂ����鎞�Ԃ����҂�

        
        {
#if UNITY_EDITOR
            while(async.progress < 0.9f)// �f�o�b�O�p���[�h�i�s�x�\��
            Debug.Log("�V�[�����[�h�i�s�x" + async.progress);
#endif
        }

        async.allowSceneActivation = true;//�V�[�����[�h����

        yield return async;//�V�[���ړ�

        SceneManager.LoadSceneAsync(SceneName);
�@�@�@�@OpenScene();//���[�h��t�F�[�h����
    }

    //�V�[�����[�h��̃t�F�[�h��������
    public void OpenScene()
    {
        fade.FadeOut(FadeTime);
    }


#if UNITY_EDITOR

    //�f�o�b�O�p
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
