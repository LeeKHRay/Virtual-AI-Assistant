using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private ParticleSystem brokenBlock;
    private ParticleSystem twinkle;
    private SpriteRenderer sr;

    void Start()
    {
        brokenBlock = transform.GetChild(0).GetComponent<ParticleSystem>();
        twinkle = transform.GetChild(1).GetComponent<ParticleSystem>();
        sr = GetComponent<SpriteRenderer>();

        ParticleSystem.MainModule brokenBlockMain = brokenBlock.main;
        brokenBlockMain.startColor = sr.color;

        ParticleSystem.MainModule twinkleMain = twinkle.main;
        twinkleMain.startColor = sr.color;
    }
    
    // play animation based on the number of cleared rows
    public float Clear(int fullRowsNum)
    {
        float waitTime = 0.0f;

        switch (fullRowsNum)
        {
            case 1:
                sr.enabled = false;
                brokenBlock.Play();
                waitTime = brokenBlock.main.startLifetime.constantMax;
                Destroy(gameObject, waitTime); // destroy block after particles disappear
                break;
            case 2:
                brokenBlock.Play();
                waitTime = brokenBlock.main.startLifetime.constantMax;
                Destroy(gameObject, waitTime);
                break;
            case 3:
                brokenBlock.Play();
                waitTime = brokenBlock.main.startLifetime.constantMax;
                Destroy(gameObject, waitTime);
                break;
            case 4:
                sr.enabled = false;
                twinkle.Play();
                waitTime = twinkle.main.startLifetime.constantMax;
                Destroy(gameObject, waitTime);
                break;
        }
        return waitTime;
    }
}
