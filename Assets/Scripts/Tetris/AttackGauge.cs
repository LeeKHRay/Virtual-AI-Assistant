using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackGauge : MonoBehaviour
{
    public int value = 0;
    private Image attackGaugeMask;
    private ParticleSystem ps;

    void Start()
    {
        attackGaugeMask = transform.GetChild(2).GetComponent<Image>();
        ps = transform.GetChild(3).GetComponent<ParticleSystem>();
    }

    public int UpdateAttackGauge(int fullRowsNum)
    {
        value += fullRowsNum;
        int garbageRowsNum = value / 2;
        value %= 2;

        if (garbageRowsNum > 0) // play animation when attack gauge is full
        {
            StartCoroutine(Animation());
        }
        else
        {
            attackGaugeMask.fillAmount = 1 - value / 2.0f;
        }

        return garbageRowsNum;
    }

    private IEnumerator Animation()
    {
        attackGaugeMask.fillAmount = 0.0f;
        ps.Play();

        yield return new WaitForSeconds(ps.main.startLifetime.constantMax);

        attackGaugeMask.fillAmount = 1 - value / 2.0f;
    }
	
    public void Reset()
    {
        value = 0;
        attackGaugeMask.fillAmount = 1.0f;
    }
}
