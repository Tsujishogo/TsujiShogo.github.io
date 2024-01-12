using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip decide;�@// ���莞SE
    [SerializeField] AudioClip select; // �J�[�\���ړ���SE
    [SerializeField] float cutTime = 0.1f; // SE�̍Đ��J�n�ʒu
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.time = cutTime;
    }

   //���肳�ꂽ�Ƃ��ɉ���炷
    public void Onclick()
    {
        audioSource.PlayOneShot(decide);
    }
    

}
