using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class I18nText : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private bool readIdFromContent = false;
    private TMP_Text text;

    public TMP_Text Text => text;

    public virtual void ApplyLocale()
    {
        text.text = I18n.S(id);
    }

    public void LoadComponent()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        LoadComponent();
        I18n.OnLocaleChanged += ApplyLocale;

        if (readIdFromContent)
        {
            id = text.text;
        }

        ApplyLocale();
    }

    private void OnDestroy()
    {
        I18n.OnLocaleChanged -= ApplyLocale;
    }
}
