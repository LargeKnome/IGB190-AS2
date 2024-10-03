using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoplayer;
    public string sceneName;
    
    [Header("Cached References")]
    [SerializeField] private Image skipFill;
    [SerializeField] private GameObject skipComponent;
    private AudioSource source;
    private AsyncOperation asyncLoad = null;

    private float currentSkipTime = 0;
    private float lastButtonPress = -99999f;

    private const float TimeToSkip = 2f;
    private const float ShowSkipTime = 2f;
//VideoPlayer video;

    void Start()
    {
        videoplayer = GetComponentInParent<VideoPlayer>();
        StartCoroutine(WaitforVideoEnd());
        LoadNextSceneAsync();
    }
    
    private void LoadNextSceneAsync()
    {
        int nextSceneIndex = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        asyncLoad = SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;
    }

    private void Update()
    {
        if (Input.anyKey) lastButtonPress = Time.time;

        skipComponent.SetActive(currentSkipTime > 0 || Time.time < lastButtonPress + ShowSkipTime);

        if (Input.GetKey(KeyCode.Escape)) currentSkipTime += Time.deltaTime;
        if (Input.GetKeyUp(KeyCode.Escape)) currentSkipTime = 0;

        skipFill.fillAmount = currentSkipTime / TimeToSkip;

        if (currentSkipTime > TimeToSkip) asyncLoad.allowSceneActivation = true;
    }

    IEnumerator WaitforVideoEnd()
    {
        float videolength = (float)videoplayer.length;
        yield return new WaitForSeconds(videolength);
        asyncLoad.allowSceneActivation = true;
    }



//next scene
//void Awake()
//{
//    video = GetComponent<VideoPlayer>();
//    video.Play();
//    video.loopPointReached += CheckOver;


//}


//void CheckOver(UnityEngine.Video.VideoPlayer vp)
//{
//    SceneManager.LoadScene(1);//the scene that you want to load after the video has ended.
//}
}
