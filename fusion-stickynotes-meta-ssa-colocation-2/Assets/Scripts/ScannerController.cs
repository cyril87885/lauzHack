using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion.Addons.TextureDrawing;

public class ScannerController : MonoBehaviour
{

    private static readonly Dictionary<string, int> StringToNumberMap = new Dictionary<string, int>
    {
        { "D4", 0 },
        { "E4", 1 },
        { "F4", 2 },
        { "G4", 3 },
        { "A4", 4 },
        { "B4", 5 },
        { "C5", 6 },
        { "D5", 7 },
        { "E5", 8 },
        { "F5", 9 },
        { "G5", 10 },
    };

    public Vector3 startPoint;
    public Vector3 endPoint;
    private float currPosition;
    public float bpm = 120f;
    private float speed;

    public TextureDrawing drawing;

    public AudioSource audioSource;

    public AudioClip[] audioClips = new AudioClip[11];
    // Start is called before the first frame update
    void Start()
    {
        float totalTime = (19 * 60) / bpm;
        speed = (endPoint.x - startPoint.x) / totalTime;
        currPosition = startPoint.x;

        transform.localPosition = new Vector3(currPosition, transform.localPosition.y, transform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        currPosition += speed * Time.deltaTime;
        currPosition = (currPosition > endPoint.x) ? startPoint.x : currPosition;

        transform.localPosition = new Vector3(currPosition, transform.localPosition.y, transform.localPosition.z);


        foreach (var note in drawing.musicSheet)
        {
            
            float scaledValue = Mathf.Lerp(-4f, 4f, note.Center.x);
            //Debug.Log("note: "+ note.Note + " scaled value: " + scaledValue +  " current scannerpos: " + transform.localPosition.x);
            if (AreFloatsEqual(transform.localPosition.x, scaledValue, 0.04f))
            {
                PlayMusic(note.Note);
            }
        }
        
    }

    void PlayMusic(string note)
    {
        //audioSource.PlayOneShot(audioClips[MapNoteToIndex(note)]);
        audioSource.PlayOneShot(audioClips[MapNoteToIndex(note)]);
        Debug.Log("Playing note: " + note);
    } 

    bool AreFloatsEqual(float a, float b, float epsilon = 1e-1f)
    {
        return Mathf.Abs(a - b) <= epsilon;
    }

    public static int MapNoteToIndex(string input)
    {
        if (StringToNumberMap.TryGetValue(input, out int value))
        {
            return value;
        }
        else
        {
            throw new KeyNotFoundException($"The key '{input}' is not defined in the mapping.");
        }
    }

}
