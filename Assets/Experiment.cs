using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

//#define WINDOWS_UWP
#if WINDOWS_UWP
using System;
using Windows.Storage;
using System.Threading.Tasks;
#endif

public class Experiment : MonoBehaviour {

	public struct Run { public ShadowType shadowType; public int dist; };

    public int preTests = 10;
    public int repetitions = 3;
    public GameObject modele;
    public bool logInFile = false;
    public GameObject planeRef;

    private bool experimentRunning = false;
    private string fileName ="";
    private string log;
    private List<Run> refExp;
    private Plane[] planes;
    private string totalLog ="";
    private TextMesh display;
    private TextToSpeech textToSpeech;

    public bool ExperimentRunning
    {
        get
        {
            return experimentRunning;
        }
    }

    // Use this for initialization
    void Start ()
    {
        //Define each factors' possibilities
		ShadowType[] shadows = new ShadowType[] { ShadowType.Off, ShadowType.Round, ShadowType.Hard, ShadowType.Soft};
		int[] dists = new int[] { 3,4,5,6 };

        //Create a List containing each trials
        refExp = new List<Run>();
		foreach (ShadowType shadow in shadows) {
			foreach (int d in dists) {
				var run = new Run ();
				run.shadowType = shadow;
				run.dist = d;

				refExp.Add (run);
			}
		}

        display = FindObjectOfType<TextMesh>();
        display.gameObject.SetActive(false);

        textToSpeech = FindObjectOfType<TextToSpeech>();
        textToSpeech.Voice = TextToSpeechVoice.Default;
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(1) || Input.GetMouseButtonUp(1) && Input.GetMouseButton(0) || Input.GetMouseButtonUp(1) && Input.GetMouseButtonUp(0))
        {
            FlushExpLog();
        }

        if (Input.GetMouseButtonUp(2) && !experimentRunning)
        {
            experimentRunning = true;
            Debug.Log("Experiment Started");

            PlayerPrefs.SetInt("expId", PlayerPrefs.GetInt("expId", 0) + 1);
            fileName = PlayerPrefs.GetInt("expId", 0).ToString("D3");

            StartCoroutine(runExperiment());
        }
    }
    
    IEnumerator runExperiment()
    {
        planeRef.SetActive(false);
        display.gameObject.SetActive(true);
        display.text = fileName;

        yield return new WaitForEndOfFrame();

        while (!Input.GetMouseButtonDown(2))
        {
            yield return new WaitForEndOfFrame();
        }

        display.gameObject.SetActive(false);


        bool intro = true;
        expLog("n,rep,i;dist,shadow;answer,time");

        for (int rep = 0; rep <= repetitions; rep++)
        {
            int i = 1;

            if (intro)
                expLog("Intro");
            else
                expLog("Rep " + rep);

            var rnd = new System.Random();
            var currentExp = refExp.OrderBy(item => rnd.Next());

            foreach (Run run in currentExp)
            {
                yield return new WaitForSeconds(1);

                if (intro && i > preTests)
                {
                    intro = false;

                    textToSpeech.StartSpeaking("This is the end of the practice session. Let's start the experiment. ");

                    while (!Input.GetMouseButton(2))
                    {
                        yield return new WaitForEndOfFrame();
                    }
                                        
                    break;
                }

                Debug.Log("" + i);

                log = fileName + "," + rep + "," + i + ";";
                float time = Time.time;

                var dist = UnityEngine.Random.Range(3.0f, 6.0f);
                var rot = UnityEngine.Random.Range(0, 360);

                var g = Instantiate(modele, this.gameObject.transform);
                g.transform.position = gameObject.transform.position;
                g.transform.Translate(gameObject.transform.TransformDirection(Vector3.forward) * dist, Space.World);
                g.transform.Rotate(Vector3.up * rot,Space.World);

                switch (run.shadowType)
                {
                    case ShadowType.Off:
                        QualitySettings.shadows = ShadowQuality.Disable;
                        break;
                    case ShadowType.Round:
                        QualitySettings.shadows = ShadowQuality.All;
                        g.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        g.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        break;
                    case ShadowType.Hard:
                        QualitySettings.shadows = ShadowQuality.HardOnly;
                        g.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        g.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        break;
                    case ShadowType.Soft:
                        QualitySettings.shadows = ShadowQuality.All;
                        g.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        g.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        break;
                }

                log += "" + run.dist + "," + run.shadowType.ToString() + ";";

                textToSpeech.StartSpeaking("Target " + (run.dist - 2).ToString());

                //Answer
                while (!Input.GetMouseButton(2))
                {
                    yield return new WaitForEndOfFrame();
                }

                log += g.transform.localPosition.z + "," + (Time.time - time);

                Destroy(g);

                expLog(log);

                i++;
            }
        }

        experimentRunning = false;
        planeRef.SetActive(true);
        FlushExpLog();
    }

    private void expLog(string s)
    {
        if (logInFile)
        {
            totalLog += s + "\n";
        }
        else
        {
            Debug.Log(s);
        }
    }

    void OnDestroy()
    {
        FlushExpLog();
    }

    void OnApplicationQuit()
    {
        FlushExpLog();
    }

    private void FlushExpLog()
    {
        if (totalLog == "" || fileName == "")
            return;

        //Debug.Log("Flush : "+totalLog);

#if WINDOWS_UWP
            Task task = new Task(
                async () =>
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(totalLog);
                    StorageFolder storageFolder = KnownFolders.PicturesLibrary;
                    StorageFile storageFile = await storageFolder.CreateFileAsync(fileName + ".txt", CreationCollisionOption.ReplaceExisting  );
                    using (var s = await storageFile.OpenStreamForWriteAsync())
                    {
                        await s.WriteAsync(data, 0, data.Length);
                    }
                });
            task.Start();
            task.Wait();
#else
        using (StreamWriter sr = new StreamWriter(Path.Combine(fileName + ".txt"), true))
        {
            sr.AutoFlush = true;
            sr.WriteLine(totalLog);
        }
#endif
        totalLog = "";
        //Debug.Log("Done");
    }
}
