using UnityEngine;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Expression;
using System.Collections.Generic;
using System.Collections;

public class AssistantBehaviour : MonoBehaviour
{
    private CubismEyeBlinkController cubismEyeBlinkController;
    private CubismExpressionController cubismExpressionController;
    private Animator animator;
    private AudioSource audioSource;
    private int expressionIndex;

    public Dictionary<string, int> expressionIndices;
    public Dictionary<string, Dictionary<string, float>> audioEmotionConf;

    void Start()
    {
        cubismEyeBlinkController = GetComponent<CubismEyeBlinkController>();
        cubismExpressionController = GetComponent<CubismExpressionController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        expressionIndex = 0;

        expressionIndices = new Dictionary<string, int>() 
            { { "smile", 0 }, { "happy", 1 }, { "angry", 2 }, { "sad", 3  }, { "shy", 4} };

        // audio configurations of different emotions
        audioEmotionConf = new Dictionary<string, Dictionary<string, float>>() {
            { "happy", new Dictionary<string, float>() { { "pitch", 3 } } }, 
            { "angry", new Dictionary<string, float>() { { "pitch", -2 }, { "volumeGainDb", 5 } } }, 
            { "sad", new Dictionary<string, float>() { { "speakingRate", 0.85f }, { "pitch", 2f } } },
            { "shy", new Dictionary<string, float>() { { "pitch", 2 } } }
        };
    }

    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Farewell")) // check if animation "Farewell" is playing
        {
            cubismEyeBlinkController.enabled = false;
            SetExpressionIndex("happy");
            cubismExpressionController.CurrentExpressionIndex = expressionIndex;
        }
    }

    public void Speak(AudioClip clip)
    {
        StartCoroutine(AssistantSpeak(clip));
    }

    private IEnumerator AssistantSpeak(AudioClip clip)
    {
        cubismExpressionController.CurrentExpressionIndex = expressionIndex;
        audioSource.clip = clip;
        audioSource.Play();
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        SetExpressionIndex("smile");
        cubismExpressionController.CurrentExpressionIndex = expressionIndex;
    }

    public void SetExpressionIndex(string expression)
    {
        expressionIndex = expressionIndices[expression];
    }
}
