using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class StairCase
{
	public bool stereo;
	public bool montion;
	public bool density;
	public bool PLC;
	public int reversalCount;
	public float[] results;
	public float[] reversalResults;
	public float finalResult;
	public int currentStep;
	public string filename;

	static int reversalMax = 14;
	static float stepDownRatio = 0.8f;
	static int finalResultCount = 10;

	private bool lastFeedback;

	public StairCase(bool stereo, bool montion, bool density, bool PLC)
	{
		this.stereo = stereo;
		this.montion = montion;
		this.density = density;
		this.PLC = PLC;

		results = new float[500];
		for (int i = 0; i < 500; i++)
		{
			results[i] = 0;
		}
		reversalCount = 0;
		finalResult = -1;
		currentStep = 0;
		reversalResults = new float[reversalMax];
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
			reversalResults[reversalCount] = results[currentStep];
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
			reversalResults[reversalCount] = results[currentStep];
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
			
			StreamWriter sw = File.AppendText("output/staircase-"+filename);
			string label = "("+(stereo?"s+":"")+(montion?"m+":"")+(density?"d+":"")+(PLC?"P":"")+")";
			sw.Write(label+",");
			for (int i = 0; i <= currentStep; i++)
			{
				sw.Write("{0},", results[i]);
			}
			sw.Write("\n");
			sw.Close();

			sw = File.AppendText("output/result-"+filename);
			sw.WriteLine(label+",{0}", finalResult);
			sw.Close();


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
	private bool stereo, montion, density, tunneling, size, transparent, PLC;
	private string filename;
	private StairCase[] staircases;
	private StairCase currentStaircase;
	private int blockCaseCount;
	private System.DateTime clusterTimestamp;
	private bool gracePeriod;
	private bool restPeriod;
	private bool movementWarning;
	private bool completed;
	private byte[] positionData;
	private int positionDataCount;
	private System.DateTime startTime;
	private float leftMostX;
	private float rightMostX;

	// Use this for initialization
	void Start ()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;

		cluster = new Transform[fragCount];
		System.IO.Directory.CreateDirectory("output");
		filename = string.Format("{0}.csv", System.DateTime.Now.ToString("yyyy-MMM-dd-HH-mm-ss"));
		if (continousMode)
		{
			// continous mode no longer supported
		}
		else
		{
			StreamWriter sw = File.AppendText("output/staircase-"+filename);
			sw.WriteLine("The following is the staircase data.");
			sw.Close();
			sw = File.AppendText("output/result-"+filename);
			sw.WriteLine("The following is the result data.");
			sw.Close();
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
		movementWarning = false;
		completed = false;

		staircases = new StairCase[16];
		for (int i = 0; i < 16; i++)
		{
			staircases [i] = new StairCase (i%2==1, i/2%2==1, i/4%2==1, i/8%2==1);
			staircases[i].results[0] = maxHeight * 0.4f;
			staircases[i].filename = filename;
		}
		currentStaircase = staircases [Random.Range (0, 16)];

		positionData = new byte[60 * 60 * 60 * 4 * 4];
		positionDataCount = 0;
		startTime = System.DateTime.Now;

		resetCluster ();
	}

	bool checkIfAvailableForSameBlock()
	{
		for (int i = 0; i < 16; i++)
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
		for (int i = 0; i < 16; i++)
		{
			if (!staircases[i].finished())
				return true;
		}
		
		return false;
	}

	void resetCluster()
	{
		float currentDistance;
		if (continousMode)
		{
			currentDistance = 0.8f;
		}
		else
		{
//			if ((System.DateTime.Now - restTimestamp).Minutes >= timeUntilRest)
//			{
//				restPeriod = true;
//				return;
//			}

			if (!checkIfAnyAvailable())
			{
				completed = true;
				byte[] dataToWrite = new byte[positionDataCount];
				System.Array.Copy(positionData, 0, dataToWrite, 0, positionDataCount);
				File.WriteAllBytes("output/tracking-"+filename, dataToWrite);
				return;
			}

			if (blockCaseCount == 0)
			{
				while(true)
				{
					int randIndex = Random.Range(0,16);
					if (!staircases[randIndex].finished())
					{
						currentStaircase = staircases[randIndex];
						break;
					}
				}
			}
			else if (blockCaseCount < blockCases && checkIfAvailableForSameBlock())
			{
				while(true)
				{
					int randIndex = Random.Range(0,16);
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
				restPeriod = true;
				return;
			}

			stereo = currentStaircase.stereo;
			montion = currentStaircase.montion;
			density = currentStaircase.density;
			tunneling = false;
			size = false;
			transparent = false;
			PLC = currentStaircase.PLC;
			currentDistance = currentStaircase.currentDistance();
			
			blockCaseCount++;
		}

		statusDisplay.GetComponent<TextMesh>().text = 
			(stereo?"stereo,":"")+(montion?"montion,":"")+(density?"density,":"")+(PLC?"PLC":"");

		OVRManager.instance.monoscopic = !stereo;
		OVRCameraRig.disablePositionTracking = !montion;
		if (montion)
		{
			leftMostX = character.position.x;
			rightMostX = character.position.x;
		}
//		if (PLC || uniform)
//		{
//			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
//			RenderSettings.ambientSkyColor = Color.white;
//			RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
//			lightSource.enabled = false;
//		}
//		else
//		{
//			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
//			RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
//			lightSource.enabled = true;
//		}
		int realFragCount = density ? 1131 : 64;

		bool leftFirst = Random.value > 0.5;

		if (redCube1 != null)
		{
			Destroy(redCube1.gameObject);
		}
		redCube1 = Instantiate (redCube) as Transform;
		redCube1.gameObject.SetActive (true);
		redCube1.position = new Vector3 (-maxHeight/4 + (Random.value-0.5f)*maxHeight/3,
		                                 maxHeight/2 + (Random.value-0.5f)*maxHeight/3*2,
		                                 maxHeight/2+(leftFirst?1:-1)*currentDistance/2);

		if (redCube2 != null)
		{
			Destroy(redCube2.gameObject);
		}
		redCube2 = Instantiate (redCube) as Transform;
		redCube2.gameObject.SetActive (true);
		redCube2.position = new Vector3 (maxHeight/4 + (Random.value-0.5f)*maxHeight/3,
		                                 maxHeight/2 + (Random.value-0.5f)*maxHeight/3*2,
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
			else
			{
				float greyScale = Random.value;
				cluster[i].GetComponent<Renderer>().material.color = new Color(greyScale, greyScale, greyScale, 1);
			}
		}

		clusterTimestamp = System.DateTime.Now;
	}

	bool checkMovedEnough()
	{
		if (!montion || rightMostX - leftMostX > 0.05)
			return true;
		else
			return false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 currentPosition = character.position;
		System.Array.Copy (System.BitConverter.GetBytes ((float)(System.DateTime.Now - startTime).TotalSeconds), 0,
		                   positionData, positionDataCount, sizeof(float));
		positionDataCount += sizeof(float);
		System.Array.Copy (System.BitConverter.GetBytes (currentPosition.x), 0,
		                   positionData, positionDataCount, sizeof(float));
		positionDataCount += sizeof(float);
		System.Array.Copy (System.BitConverter.GetBytes (currentPosition.y), 0,
		                   positionData, positionDataCount, sizeof(float));
		positionDataCount += sizeof(float);
		System.Array.Copy (System.BitConverter.GetBytes (currentPosition.z), 0,
		                   positionData, positionDataCount, sizeof(float));
		positionDataCount += sizeof(float);

		if (currentPosition.x < leftMostX)
		{
			leftMostX = currentPosition.x;
		}
		if (currentPosition.x > rightMostX)
		{
			rightMostX = currentPosition.x;
		}

		if (size)
		{
			redCube1.localScale = redCube.localScale * (character.position.z - redCube1.position.z) / (character.position.z - maxHeight/2);
			redCube2.localScale = redCube.localScale * (character.position.z - redCube2.position.z) / (character.position.z - maxHeight/2);
			for (int i = 0; i < fragCount; i++)
			{
				if (cluster[i] != null)
				{
					cluster[i].localScale = fragment.localScale * (character.position.z - cluster[i].position.z) / (character.position.z - maxHeight/2);
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
		
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			OVRManager.display.RecenterPose();
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
		else if (movementWarning && (System.DateTime.Now - clusterTimestamp).TotalSeconds < 2)
		{
			maskCube.gameObject.SetActive(true);
			completeDisplay.GetComponent<TextMesh> ().text = "Please move your head to have better judgment!";
		}
		else if (movementWarning && (System.DateTime.Now - clusterTimestamp).TotalSeconds >= 2)
		{
			maskCube.gameObject.SetActive(false);
			movementWarning = false;
			resetCluster();
		}
		else if (!gracePeriod && (System.DateTime.Now - clusterTimestamp).TotalSeconds > 4)
		{
			gracePeriod = true;
			clusterTimestamp = System.DateTime.Now;
			maskCube.gameObject.SetActive(true);
			completeDisplay.GetComponent<TextMesh> ().text = "Too slow! Please try to make selection within 4 seconds!";
		}
		else if (gracePeriod && (System.DateTime.Now - clusterTimestamp).TotalSeconds > 2)
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
				resetCluster();
			}
		}
		else if (!continousMode && Input.GetKeyDown ("left")) 
		{
			if (checkMovedEnough())
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
			else
			{
				movementWarning = true;
				clusterTimestamp = System.DateTime.Now;
			}
		}
		else if (!continousMode && Input.GetKeyDown("right"))
		{
			if (checkMovedEnough())
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
			else
			{
				movementWarning = true;
				clusterTimestamp = System.DateTime.Now;
			}
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
			// continous mode no longer supported
			//fileWriter.WriteLine("{0}", (redCube1.position.z-redCube2.position.z)*1000);
			resetCluster();
		}
		else if (Input.GetKeyDown("escape"))
		{
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
