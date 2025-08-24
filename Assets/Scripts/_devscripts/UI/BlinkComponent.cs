using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlinkComponent : MonoBehaviour
{
    public Behaviour Component;
    private AudioSource _source;

    public float BlinkTime;
    // Start is called before the first frame update
    void Awake()
    {
        _source = GetComponent<AudioSource>();
        
    }

    void OnEnable()
    {
        StartCoroutine(Blink());
    }
    IEnumerator Blink()
    {
        while (true)
        {
            _source?.Play();

            Component.enabled = !Component.enabled;
            yield return new WaitForSeconds(BlinkTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
