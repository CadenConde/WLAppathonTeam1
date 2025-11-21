using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

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

    public string expectedPhrase1;  // the correct Mandarin phrase
    public string expectedPhrase2;  // the correct Mandarin phrase
    public string expectedPhrase3;  // the correct Mandarin phrase
    public string expectedPhrase4;  // the correct Mandarin phrase

    [Header("Cube Spawn")]
    // public GameObject interactableCubePrefab;
    public GameObject bobaPrefab;
    public GameObject beefNoodlePrefab;
    public GameObject stinkyTofuPrefab;
    public GameObject scallionPancakePrefab;

    public Transform cubeSpawnParent;

    private ConversationState state = ConversationState.PhoneCall;

    private void Start()
    {
        StartPhoneCall();
    }

    void Update()
    {
        //debug
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            SpawnCubes(999, 0);
            HandleSuccess();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            Debug.Log("B pressed, resetting Scene ");

            GameObject[] spawned = GameObject.FindGameObjectsWithTag("SpawnedFood");
            foreach (GameObject g in spawned)
            {
                Destroy(g);
            }

            state = ConversationState.PhoneCall;
            StartPhoneCall();;
        }
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
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.Four));

        //yield return StartCoroutine(AnimatePhoneFromBelow());

        StartCoroutine(PhoneCallSequence());
    }

    private IEnumerator PhoneCallSequence()
    {
        phoneAudioSource.Play();
        // Wait for mom's audio to finish
        yield return new WaitForSeconds(phoneAudioSource.clip.length);

        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.Four));

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
        int n = GetNumberFromCleanedString(cleaned);
        string target1 = expectedPhrase1.Trim().ToLower();
        string target2 = expectedPhrase2.Trim().ToLower();
        string target3 = expectedPhrase3.Trim().ToLower();
        string target4 = expectedPhrase4.Trim().ToLower();

        if (n > 0 && cleaned.Contains(target1))
        {
            HandleSuccess();
            SpawnCubes(1, n);
        }
        else if (n > 0 && cleaned.Contains(target2))
        {
            HandleSuccess();
            SpawnCubes(2, n);
        }
        else if (n > 0 && cleaned.Contains(target3))
        {
            HandleSuccess();
            SpawnCubes(3, n);
        }
        else if (n > 0 && cleaned.Contains(target4))
        {
            HandleSuccess();
            SpawnCubes(4, n);
        }
        else
        {
            Debug.Log("playing failure audio");
            HandleFailure();
        }
    }

    private static readonly Dictionary<char, int> chineseToArabic = new Dictionary<char, int>()
    {
        { '一', 1 },
        { '兩', 2 },
        { '三', 3 },
        { '四', 4 },
        { '五', 5 },
        { '六', 6 },
        { '七', 7 },
        { '八', 8 },
        { '九', 9 }
    };

    public static int GetNumberFromCleanedString(string cleaned)
    {
        foreach (char c in cleaned)
        {
            if (chineseToArabic.TryGetValue(c, out int value))
            {
                return value;
            }
        }

        return -1; // not found
    }

    private void HandleSuccess()
    {
        //state = ConversationState.VendorSuccess;

        vendorAudioSource.clip = vendorSuccessAudio;
        vendorAudioSource.Play();
    }

    private void HandleFailure()
    {
        state = ConversationState.VendorFailure;

        vendorAudioSource.clip = vendorFailureAudio;
        vendorAudioSource.Play();

        // After audio, ask again
        Invoke(nameof(AskVendorQuestion), vendorAudioSource.clip.length + 0.2f);
    }

    private void SpawnCubes(int item, int n)
    {
        Debug.Log("Would spawn: " + n + " of item " + item);

        //debug
        if (item == 999)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3[] multiPos2 = {
                    new Vector3(0.434f, 0.75f, 0.692f),
                    new Vector3(0f, 0.75f, 0.784f),
                    new Vector3(-0.429f, 0.75f, 0.701f)
                };
                Instantiate(bobaPrefab, multiPos2[i], Quaternion.Euler(-90f, 0f, 0f), cubeSpawnParent);
            }
            for (int i = 0; i < 1; i++)
            {
                Vector3 pos = new Vector3(0.0858f, 0.75f, 0.625f);
                Instantiate(beefNoodlePrefab, pos, Quaternion.Euler(-90f, 0f, 0f), cubeSpawnParent);
            }
            return;
        }

        //else
        Vector3[] multiPos = {
            new Vector3(0.434f, 0.75f, 0.692f),
            new Vector3(0f, 0.75f, 0.784f),
            new Vector3(0.1f, 0.75f, 0.625f),
            new Vector3(-0.4f, 0.75f, 0.7f),
            new Vector3(-0.2f, 0.75f, 0.65f),
            new Vector3(-0.6f, 0.75f, 0.8f),
            new Vector3(-0.5f, 0.75f, 0.62f),
            new Vector3(0.2f, 0.75f, 0.7f)
        };

        int r = Mathf.CeilToInt(Random.Range(0, 8));

        Dictionary<int, GameObject> getItem = new Dictionary<int, GameObject>
        {
            { 1, bobaPrefab },
            { 2, beefNoodlePrefab },
            { 3, stinkyTofuPrefab },
            { 4, scallionPancakePrefab },
        };

        GameObject prefab = getItem[item];

        for (int i = 0; i < n; i++)
        {
            Instantiate(prefab, multiPos[(i + r) % multiPos.Length], Quaternion.Euler(-90f, 0f, 0f), cubeSpawnParent);
        }

        return;
    }
}
