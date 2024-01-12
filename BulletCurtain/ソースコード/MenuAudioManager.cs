using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip decide;　// 決定時SE
    [SerializeField] AudioClip select; // カーソル移動時SE
    [SerializeField] float cutTime = 0.1f; // SEの再生開始位置
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.time = cutTime;
    }

   //決定されたときに音を鳴らす
    public void Onclick()
    {
        audioSource.PlayOneShot(decide);
    }
    

}
