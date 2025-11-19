using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConversationManager : MonoBehaviour
{
    [Header("Phone Call")]
    public GameObject phoneMenu;
    public AudioSource phoneAudioSource;
    public string momText;

    [Header("Vendor")]
    public AudioSource vendorAudioSource;
    //public string vendorQuestionText;
    public AudioClip vendorQuestionAudio;
    public AudioClip vendorSuccessAudio;
    public AudioClip vendorFailureAudio;
    public string expectedPhrase;  // the correct Mandarin phrase

    [Header("Cube Spawn")]
    public GameObject interactableCubePrefab;
    public Transform cubeSpawnParent;

    private ConversationState state = ConversationState.PhoneCall;

    private void Start()
    {
        StartPhoneCall();
    }

    private void StartPhoneCall()
    {
        phoneMenu.SetActive(true);

        var text = phoneMenu.GetComponentInChildren<TMPro.TextMeshPro>();
        if (text != null)
        {
            text.text = momText;
        }

        StartCoroutine(StartPhoneSequence());
    }

    private IEnumerator StartPhoneSequence()
    {
        yield return StartCoroutine(AnimatePhoneFromBelow());
        StartCoroutine(PhoneCallSequence());
    }

    private IEnumerator PhoneCallSequence()
    {
        phoneAudioSource.Play();

        // Additional 5 second pause
        yield return new WaitForSeconds(3f);

        // Wait for mom's audio to finish
        yield return new WaitForSeconds(phoneAudioSource.clip.length);

        // Additional 5 second pause
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(AnimatePhoneAndHide());

        BeginVendorInteraction();
    }
    private IEnumerator AnimatePhoneFromBelow()
    {
        Transform t = phoneMenu.transform;

        Vector3 end = t.localPosition;
        Vector3 start = end + new Vector3(0, -0.5f, 0);

        t.localPosition = start;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            t.localPosition = Vector3.Lerp(start, end, p);
            yield return null;
        }
    }

    private IEnumerator AnimatePhoneAndHide()
    {
        Transform t = phoneMenu.transform;

        Vector3 start = t.localPosition;
        Vector3 end = start + new Vector3(0, -.5f, 0);

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            t.localPosition = Vector3.Lerp(start, end, p);
            yield return null;
        }

        phoneMenu.SetActive(false);
    }


    private void BeginVendorInteraction()
    {
        phoneMenu.SetActive(false);

        state = ConversationState.VendorQuestion;
        AskVendorQuestion();
    }

    private void AskVendorQuestion()
    {
        vendorAudioSource.clip = vendorQuestionAudio;
        vendorAudioSource.Play();
        state = ConversationState.VendorAwaitResponse;
    }

    public void OnUserSpeech(string spokenText)
    {
        if (state != ConversationState.VendorAwaitResponse)
        {
            return;
        }

        string cleaned = spokenText.Trim().ToLower();
        string target = expectedPhrase.Trim().ToLower();

        if (cleaned.Contains(target))
        {
            HandleSuccess();
        }
        else
        {
            HandleFailure();
        }
    }

    private void HandleSuccess()
    {
        state = ConversationState.VendorSuccess;

        vendorAudioSource.clip = vendorSuccessAudio;
        vendorAudioSource.Play();

        SpawnCubes();
    }

    private void HandleFailure()
    {
        state = ConversationState.VendorFailure;

        vendorAudioSource.clip = vendorFailureAudio;
        vendorAudioSource.Play();

        // After audio, ask again
        Invoke(nameof(AskVendorQuestion), vendorAudioSource.clip.length + 0.2f);
    }

    private void SpawnCubes()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 pos = new Vector3(0, 0.75f, 0.5f);
            Instantiate(interactableCubePrefab, pos, Quaternion.identity, cubeSpawnParent);
        }
    }
}
