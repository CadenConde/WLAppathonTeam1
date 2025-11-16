using UnityEngine;
using Meta.WitAi.Dictation;
using TMPro;
using Oculus.Voice.Dictation;

public class DictationLogger : MonoBehaviour
{
    public AppDictationExperience dictation;
    public TextMeshPro display;

    void Start()
    {
        if (dictation == null)
        {
            dictation = FindObjectOfType<AppDictationExperience>();
        }

        if (display == null)
        {
            display = FindObjectOfType<TextMeshPro>();
        }

        dictation.DictationEvents.OnFullTranscription.AddListener(OnFull);
        dictation.DictationEvents.OnError.AddListener((err, msg) => Debug.LogError("Dictation error: " + err + " | " + msg));
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("A pressed, activating dictation");
            dictation.Activate();
        }

        if (OVRInput.GetUp(OVRInput.Button.One))
        {
            Debug.Log("A released, deactivating dictation");
            dictation.Deactivate();
        }
    }

    private void OnFull(string text)
    {
        Debug.Log("Full: " + text);
        if (display != null) display.text = text;
    }
}
