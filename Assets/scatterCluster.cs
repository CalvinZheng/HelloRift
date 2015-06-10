﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class StairCase
{
	public bool stereo;
	public bool montion;
	public bool density;
	public bool uniform;
	public bool PLC;
	public int reversalCount;
	public float[] results;
	public float finalResult;
	public int currentStep;
	public StreamWriter fileWriter;

	static int reversalMax = 14;
	static float stepDownRatio = 0.8f;
	static int finalResultCount = 6;

	private bool lastFeedback;

	public StairCase(bool stereo, bool montion, bool density, bool uniform, bool PLC)
	{
		if (uniform && PLC)
		{
			Debug.Log("ERROR!");
		}

		this.stereo = stereo;
		this.montion = montion;
		this.density = density;
		this.uniform = uniform;
		this.PLC = PLC;

		results = new float[500];
		for (int i = 0; i < 500; i++)
		{
			results[i] = 0;
		}
		reversalCount = 0;
		finalResult = -1;
		currentStep = 0;
	}

	public float currentDistance()
	{
		return results [currentStep];
	}

	public bool finished()
	{
		return finalResult != -1;
	}

	public void feedbackWrong()
	{
		if (finished())
		{
			Debug.Log("Error! Doing a finished staircase!");
			return;
		}

		if (currentStep > 0 && lastFeedback != false)
		{
			reversalCount++;
		}

		lastFeedback = false;

		if (reversalCount >= reversalMax)
		{
			outputResult();
			return;
		}
		else
		{
			results[currentStep+1] = results[currentStep]/stepDownRatio;
		}

		currentStep++;
	}
	
	public void feedbackRight()
	{
		if (finished())
		{
			Debug.Log("Error! Doing a finished staircase!");
			return;
		}
		
		if (currentStep > 0 && lastFeedback != true)
		{
			reversalCount++;
		}

		lastFeedback = true;
		
		if (reversalCount >= reversalMax)
		{
			outputResult();
			return;
		}
		else
		{
			results[currentStep+1] = results[currentStep]*stepDownRatio;
		}
		
		currentStep++;
	}

	public void outputResult()
	{
		if (currentStep >= finalResultCount)
		{
			finalResult = 0;
			for (int i = currentStep; i > currentStep - finalResultCount; i--)
			{
				finalResult += results[i];
			}
			finalResult /= finalResultCount;

			fileWriter.WriteLine((stereo?"stereo,":"")+(montion?"montion,":"")+(density?"density,":"")+(uniform?"uniform,":"")+(PLC?"PLC":""));
			for (int i = 0; i <= currentStep; i++)
			{
				fileWriter.Write("{0},", results[i]);
			}
			fileWriter.Write("\n");
			fileWriter.WriteLine("Final result:, {0}", finalResult);

			Debug.Log("One experiment done!");
		}
		else
		{
			Debug.Log("ERROR! not enough results!");
		}
	}

	public float completeRate()
	{
		return (float)reversalCount / reversalMax;
	}
}

public class scatterCluster : MonoBehaviour {

	public Transform fragment;
	public Transform redCube;
	public Transform character;
	public Transform monitorCube;
	public Transform maskCube;
	public int fragCount;
	public float maxHeight;
	public float tunnelWidth;
	public GameObject statusDisplay;
	public GameObject feedbackDisplay;
	public GameObject distanceDisplay;
	public GameObject progressDisplay;
	public GameObject completeDisplay;
	public Material transparentMat;
	public Light lightSource;
	public bool continousMode;
	public int blockCases;
	public float timeUntilRest;

	private Transform[] cluster;
	private Transform redCube1, redCube2;
	private Vector3 lastPos;
	private bool stereo, montion, density, tunneling, size, transparent, PLC;
	private StreamWriter fileWriter;
	private StairCase[] staircases;
	private StairCase currentStaircase;
	private int blockCaseCount;
	private System.DateTime clusterTimestamp;
	private System.DateTime restTimestamp;
	private bool gracePeriod;
	private bool restPeriod;
	private bool completed;

	// Use this for initialization
	void Start ()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;

		cluster = new Transform[fragCount];
		lastPos = Vector3.zero;
		System.IO.Directory.CreateDirectory("output");
		fileWriter = File.CreateText(string.Format("output/output-{0}.txt", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss")));
		if (continousMode)
		{
			fileWriter.WriteLine("This is continues mode.");
		}
		else
		{
			fileWriter.WriteLine("This is staircase mode.");
		}
		
		stereo = true;
		montion = true;
		density = true;
		tunneling = true;
		size = true;
		transparent = false;
		PLC = false;
		gracePeriod = false;
		restPeriod = false;
		completed = false;
		restTimestamp = System.DateTime.Now;

		staircases = new StairCase[24];
		for (int i = 0; i < 24; i++)
		{
			staircases [i] = new StairCase (i%2==1, i/2%2==1, i/4%2==1, i/8==1, i/8==2);
			staircases[i].results[0] = maxHeight * 0.8f;
			staircases[i].fileWriter = fileWriter;
		}
		currentStaircase = staircases [Random.Range (0, 24)];

		resetCluster ();
	}

	bool checkIfAvailableForSameBlock()
	{
		for (int i = 0; i < 24; i++)
		{
			if (staircases[i].finished())
				continue;
			if (staircases[i].stereo == currentStaircase.stereo && staircases[i].montion == currentStaircase.montion)
				return true;
		}

		return false;
	}
	
	bool checkIfAnyAvailable()
	{
		for (int i = 0; i < 24; i++)
		{
			if (!staircases[i].finished())
				return true;
		}
		
		return false;
	}

	void resetCluster()
	{
		bool uniform = false;

		float currentDistance;
		if (continousMode)
		{
			currentDistance = 0.8f;
		}
		else
		{
			if ((System.DateTime.Now - restTimestamp).Minutes >= timeUntilRest)
			{
				restPeriod = true;
				return;
			}
			else if (!checkIfAnyAvailable())
			{
				fileWriter.Close();
				completed = true;
				return;
			}

			if (blockCaseCount < blockCases && checkIfAvailableForSameBlock())
			{
				while(true)
				{
					int randIndex = Random.Range(0,24);
					if (!staircases[randIndex].finished()
					    && staircases[randIndex].stereo == currentStaircase.stereo
					    && staircases[randIndex].montion == currentStaircase.montion)
					{
						currentStaircase = staircases[randIndex];
						break;
					}
				}
			}
			else
			{
				blockCaseCount = 0;
				while(true)
				{
					int randIndex = Random.Range(0,24);
					if (!staircases[randIndex].finished())
					{
						currentStaircase = staircases[randIndex];
						break;
					}
				}
			}

			stereo = currentStaircase.stereo;
			montion = currentStaircase.montion;
			density = currentStaircase.density;
			tunneling = false;
			size = true;
			transparent = false;
			uniform = currentStaircase.uniform;
			PLC = currentStaircase.PLC;
			currentDistance = currentStaircase.currentDistance();
			
			blockCaseCount++;
		}

		statusDisplay.GetComponent<TextMesh>().text = 
			(stereo?"stereo,":"")+(montion?"montion,":"")+(density?"density,":"")+(tunneling?"tunneling,":"")+(size?"size,":"")+(transparent?"transparent,":"")+(uniform?"uniform,":"")+(PLC?"PLC":"");

		//OVRManager.instance.monoscopic = !stereo;
		//OVRCameraRig.disablePositionTracking = !montion;
		if (PLC || uniform)
		{
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.ambientSkyColor = Color.white;
			RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
			lightSource.enabled = false;
		}
		else
		{
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
			RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
			lightSource.enabled = true;
		}
		int realFragCount = density ? 1131 : 64;

		bool leftFirst = Random.value > 0.5;

		if (redCube1 != null)
		{
			Destroy(redCube1.gameObject);
		}
		redCube1 = Instantiate (redCube) as Transform;
		redCube1.gameObject.SetActive (true);
		redCube1.position = new Vector3 (-maxHeight/4 + (Random.value-0.5f)*maxHeight/3,
		                                 Random.value*maxHeight,
		                                 maxHeight/2+(leftFirst?1:-1)*currentDistance/2);

		if (redCube2 != null)
		{
			Destroy(redCube2.gameObject);
		}
		redCube2 = Instantiate (redCube) as Transform;
		redCube2.gameObject.SetActive (true);
		redCube2.position = new Vector3 (maxHeight/4 + (Random.value-0.5f)*maxHeight/3,
		                                 Random.value*maxHeight,
		                                 maxHeight/2+(!leftFirst?1:-1)*currentDistance/2);

		for (int i = 0; i < fragCount; i++)
		{
			if (cluster[i] != null)
			{
				Destroy(cluster[i].gameObject);
			}
		}

		for (int i = 0; i < realFragCount; i++)
		{
			cluster[i] = Instantiate(fragment) as Transform;
			cluster[i].gameObject.SetActive(true);
			while(true)
			{
				cluster[i].position = new Vector3(Random.value*maxHeight-maxHeight/2, Random.value*maxHeight, Random.value*maxHeight);
				if (!tunneling)
					break;

				Vector3 fragV = cluster[i].position - character.position;
				Vector3 cube1V = redCube1.position - character.position;
				Vector3 cube2V = redCube2.position - character.position;
				float dist1 = Mathf.Sqrt(Mathf.Pow(fragV.magnitude,2) - Mathf.Pow(Vector3.Dot(fragV, cube1V)/cube1V.magnitude, 2));
				float dist2 = Mathf.Sqrt(Mathf.Pow(fragV.magnitude,2) - Mathf.Pow(Vector3.Dot(fragV, cube2V)/cube2V.magnitude, 2));
				if ((fragV.magnitude > cube1V.magnitude+tunnelWidth || dist1 > tunnelWidth)
				    && (fragV.magnitude > cube2V.magnitude+tunnelWidth || dist2 > tunnelWidth))
					break;
			}
			cluster[i].rotation = Random.rotation;
			if (PLC || transparent)
			{
				float greyScale = PLC ? 1-cluster[i].position.z/maxHeight : 1;
				cluster[i].GetComponent<Renderer>().material.color = new Color(greyScale, greyScale, greyScale, 1);
			}
		}

		clusterTimestamp = System.DateTime.Now;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (lastPos != Vector3.zero && lastPos != character.position)
		{
			Debug.DrawLine(lastPos + 5*Vector3.back, character.position + 5*Vector3.back, Color.yellow, 100);
		}
		lastPos = character.position;

		if (size)
		{
			redCube1.localScale = redCube.localScale * (character.position.z - redCube1.position.z) / character.position.z;
			redCube2.localScale = redCube.localScale * (character.position.z - redCube2.position.z) / character.position.z;
			for (int i = 0; i < fragCount; i++)
			{
				if (cluster[i] != null)
				{
					cluster[i].localScale = fragment.localScale * (character.position.z - cluster[i].position.z) / character.position.z;
				}
			}
		}
		else
		{
			redCube1.localScale = redCube.localScale;
			redCube2.localScale = redCube.localScale;
			for (int i = 0; i < fragCount; i++)
			{
				if (cluster[i] != null)
				{
					cluster[i].localScale = fragment.localScale;
				}
			}
		}

		if (completed)
		{
			maskCube.gameObject.SetActive(true);
			completeDisplay.GetComponent<TextMesh> ().text = "Experiment complete! Thank you for your time!";
		}
		else if (restPeriod)
		{
			maskCube.gameObject.SetActive(true);
			completeDisplay.GetComponent<TextMesh> ().text = "Please take a rest! You can resume anytime by pressing space key!";
		}
		else if (!gracePeriod && (System.DateTime.Now - clusterTimestamp).Seconds > 4)
		{
			gracePeriod = true;
			clusterTimestamp = System.DateTime.Now;
			maskCube.gameObject.SetActive(true);
			completeDisplay.GetComponent<TextMesh> ().text = "Too slow! Please try to make selection within 4 seconds!";
		}
		else if (gracePeriod && (System.DateTime.Now - clusterTimestamp).Seconds > 2)
		{
			gracePeriod = false;
			maskCube.gameObject.SetActive(false);
			resetCluster();
		}

		if (gracePeriod || completed)
		{
			// gracePeriod has no reaction
		}
		else if (restPeriod)
		{
			if (Input.GetKeyDown ("space"))
			{
				restPeriod = false;
				maskCube.gameObject.SetActive(false);
				restTimestamp = System.DateTime.Now;
				resetCluster();
			}
		}
		else if (!continousMode && Input.GetKeyDown ("left")) 
		{
			if(redCube1.position.z <= redCube2.position.z)
			{
				feedbackDisplay.GetComponent<TextMesh>().text = "Correct!";
				currentStaircase.feedbackRight();
			}
			else
			{
				feedbackDisplay.GetComponent<TextMesh>().text = "Wrong!";
				currentStaircase.feedbackWrong();
			}

			resetCluster();
		}
		else if (!continousMode && Input.GetKeyDown("right"))
		{
			if(redCube1.position.z >= redCube2.position.z)
			{
				feedbackDisplay.GetComponent<TextMesh>().text = "Correct!";
				currentStaircase.feedbackRight();
			}
			else
			{
				feedbackDisplay.GetComponent<TextMesh>().text = "Wrong!";
				currentStaircase.feedbackWrong();
			}

			resetCluster();
		}
		else if (continousMode && Input.GetKey("up"))
		{
			redCube1.position -= Vector3.forward*maxHeight*0.003f;
			redCube2.position += Vector3.forward*maxHeight*0.003f;
		}
		else if (continousMode && Input.GetKey("down"))
		{
			redCube1.position += Vector3.forward*maxHeight*0.003f;
			redCube2.position -= Vector3.forward*maxHeight*0.003f;
		}
		else if (continousMode && Input.GetKeyDown("space"))
		{
			fileWriter.WriteLine("{0}", (redCube1.position.z-redCube2.position.z)*1000);
			resetCluster();
		}
		else if (Input.GetKeyDown("escape"))
		{
			fileWriter.Close();
			Application.Quit();
		}
		else if (Input.GetKeyDown("1"))
		{
			stereo = !stereo;
			resetCluster();
		}
		else if (Input.GetKeyDown("2"))
		{
			montion = !montion;
			resetCluster();
		}
		else if (Input.GetKeyDown("3"))
		{
			density = !density;
			resetCluster();
		}
		else if (Input.GetKeyDown("4"))
		{
			tunneling = !tunneling;
			resetCluster();
		}
		else if (Input.GetKeyDown("5"))
		{
			size = !size;
			resetCluster();
		}
		else if (Input.GetKeyDown("6"))
		{
			transparent = !transparent;
			resetCluster();
		}
		else if (Input.GetKeyDown("7"))
		{
			PLC = !PLC;
			resetCluster();
		}
		else if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			//OVRManager.display.RecenterPose();
		}
		else if (Input.GetKeyDown(KeyCode.RightControl))
		{
			monitorCube.GetComponent<Renderer>().enabled = !monitorCube.GetComponent<Renderer>().enabled;
		}

		if (!continousMode)
		{
			distanceDisplay.GetComponent<TextMesh> ().text = string.Format("{0}", currentStaircase.currentDistance());
			float progress = 0;
			foreach (StairCase aStaircase in staircases)
			{
				progress += aStaircase.completeRate();
			}
			progress /= staircases.Length;
			progressDisplay.GetComponent<TextMesh> ().text = string.Format("{0}%", progress*100);
		}
	}

	void copyMaterial(GameObject anObject)
	{
		anObject.GetComponent<Renderer>().material = new Material (anObject.GetComponent<Renderer>().material);
	}
}
