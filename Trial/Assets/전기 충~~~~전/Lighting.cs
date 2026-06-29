using System.Collections;
using UnityEngine;

public class Lighting : MonoBehaviour
{
    public GameObject First;
    public GameObject Secend;

    public float shot = 0.5f;
    public float puls = 0.5f;
    //public int pulsCount = 5;
    public bool on = false;

    void Awake()
    {
        First.SetActive(false);
        Secend.SetActive(false);
    }

    void OnEnable()
    {
        OnLighting();
    }

    public void OnLighting()
    {
        StartCoroutine(Shot());
    }

    IEnumerator Plus()
    {
        yield return new WaitForSeconds(puls);
        //for (int i = 0; i < pulsCount; i++)
        while (on)
        {
            Secend.SetActive(true);
            float r = Random.Range(puls/3, puls);
            yield return new WaitForSeconds(r);
            Secend.SetActive(false);
        }
    }

    IEnumerator Shot()
    {
        First.SetActive(true);
        yield return new WaitForSeconds(shot);
        First.SetActive(false);
        StartCoroutine(Plus());
    }
}
