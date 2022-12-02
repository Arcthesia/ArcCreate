using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class IText : MonoBehaviour
{
    [SerializeField] private string id;
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        I18n.OnLocaleChanged.AddListener(ApplyLocale);
        text.text = I18n.S(id);
    }

    private void ApplyLocale()
    {
        text.text = I18n.S(id);
    }

    private void OnDestroy()
    {
        I18n.OnLocaleChanged.RemoveListener(ApplyLocale);
    }
}
