using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class IText : MonoBehaviour
{
    public string Id;
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    void Start()
    {
        I.OnLocaleChanged.AddListener(ApplyLocale);
        text.text = I.S(Id);
    } 

    void ApplyLocale()
    {
        text.text = I.S(Id);
    }

    private void OnDestroy()
    {
        I.OnLocaleChanged.RemoveListener(ApplyLocale);    
    }
}
