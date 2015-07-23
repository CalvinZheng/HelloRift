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
	public bool forceMove;
	public bool transparent;
	public bool tunneling;
	public bool uneven;
	public int reversalCount;
	public float[] results;
	public float[] reversalResults;
	public float finalResult;
	public int currentStep;
	public string filename;

	static int reversalMax = 20;
	static float stepDownRatio = 0.7f;
	static float stepUpRatio = 2.19f;
	static int finalResultCount = 16;

	private bool lastFeedback;

	static private int sampleNumber = 10;
	static private int totalTrials = 10000;
	static private float initDistance = 0.2f;
	private int currentLevel;
	private int[] rightCount;
	private int[] wrongCount;
	private int[] disagreeCount;

	public StairCase(bool stereo, bool montion, bool density, bool PLC)
	{
		this.stereo = stereo;
		this.montion = montion;
		this.density = density;
		this.PLC = PLC;

		this.forceMove = montion;
		this.transparent = false;
		this.tunneling = false;
		this.uneven = false;

		results = new float[500];
		for (int i = 0; i < 500; i++)
		{
			results[i] = 0;
		}
		reversalCount = 0;
		finalResult = -1;
		currentStep = 0;
		reversalResults = new float[reversalMax];

		currentLevel = Random.Range (0, sampleNumber);
		rightCount = new int[sampleNumber];
		wrongCount = new int[sampleNumber];
		disagreeCount = new int[sampleNumber];
		for (int i = 0; i < sampleNumber; i++)
		{
			wrongCount[i] = 0;
			rightCount[i] = 0;
			disagreeCount[i] = 0;
		}
	}

	public float currentDistance()
	{
		return initDistance * Mathf.Pow (stepDownRatio, currentLevel);
//		return results [currentStep];
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

//		if (currentStep > 0 && lastFeedback != false)
//		{
//			reversalResults[reversalCount] = results[currentStep];
//			reversalCount++;
//		}
//
//		lastFeedback = false;

		wrongCount [currentLevel]++;

		if (currentStep >= totalTrials)
//		if (reversalCount >= reversalMax)
		{
			outputResult();
			return;
		}
		else
		{
			currentLevel = Random.Range (0, sampleNumber);
//			results[currentStep+1] = results[currentStep]*stepUpRatio;
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
		
//		if (currentStep > 0 && lastFeedback != true)
//		{
//			reversalResults[reversalCount] = results[currentStep];
//			reversalCount++;
//		}
//
//		lastFeedback = true;

		rightCount [currentLevel]++;

		if (currentStep >= totalTrials)
//		if (reversalCount >= reversalMax)
		{
			outputResult();
			return;
		}
		else
		{
			currentLevel = Random.Range (0, sampleNumber);
//			results[currentStep+1] = results[currentStep]*stepDownRatio;
		}
		
		currentStep++;
	}

	public void feedbackDisagree()
	{
		disagreeCount [currentLevel]++;
	}

	public void outputResult()
	{
		if (currentStep >= finalResultCount)
		{
			finalResult = 0;
//			for (int i = 0; i < finalResultCount; i++)
//			{
//				finalResult += Mathf.Log(reversalResults[reversalMax - i - 1], 2);
//			}
//			finalResult /= finalResultCount;
//			finalResult = Mathf.Pow(2, finalResult);
			
			StreamWriter sw = File.AppendText("output/staircase-"+filename);
			string label = "("+(stereo?"s+":"")+(montion?"m+":"")+(forceMove?"f+":"")+(!density?"b":"")+(PLC?"P":"")+(transparent?"a":"")+(tunneling?"t":"")+(uneven?"e":"")+")";
//			sw.Write(label+",");
//			for (int i = 0; i <= currentStep; i++)
//			{
//				sw.Write("{0},", results[i]);
//			}
//			sw.Write("\n");
			sw.Close();

			sw = File.AppendText("output/result-"+filename);
			sw.WriteLine(label+",{0}", finalResult);
			for (int i = 0; i < sampleNumber; i++)
			{
				sw.WriteLine("{0},{1},{2}", initDistance * Mathf.Pow (stepDownRatio, i), (float)rightCount[i]/(wrongCount[i]+rightCount[i]), (float)disagreeCount[i]/(wrongCount[i]+rightCount[i]));
			}
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
		return (float)currentStep / totalTrials;
//		return (float)reversalCount / reversalMax;
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
	public GameObject tipDisplay;
	public Material transparentMat;
	public Light lightSource;
	public bool continousMode;
	public int blockCases;
	public float timeUntilRest;
	public float unevenRate;
	public bool pseudoObserver;
	public Transform leftEye;
	public Transform rightEye;
	public Transform monoEye;

	private Transform[] cluster;
	private Transform redCube1, redCube2;
	private bool stereo, montion, density, tunneling, size, transparent, PLC;
	private string filename;
	private StairCase[] staircases;
	private StairCase currentStaircase;
	static private int staircaseCount = 2;
	private int blockCaseCount;
	private System.DateTime clusterTimestamp;
	private bool timeoutPeriod;
	private bool gracePeriod;
	private System.DateTime graceTimestamp;
	private bool restPeriod;
	private bool movementWarning;
	private bool completed;
	private byte[] positionData;
	private int positionDataCount;
	private System.DateTime startTime;
	private float leftMostX;
	private float rightMostX;
	private bool observed;
	private bool observedResult;

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
		timeoutPeriod = false;
		restPeriod = false;
		movementWarning = false;
		completed = false;
		gracePeriod = false;
		graceTimestamp = System.DateTime.Now;
		observed = false;

		staircases = new StairCase[staircaseCount];
		
//		staircases [0] = new StairCase (false, false, true, false);
//		staircases [1] = new StairCase (true, false, true, false);
//		staircases [2] = new StairCase (true, false, true, false);
//		staircases [2].forceMove = true;
//		staircases [3] = new StairCase (false, true, true, false);
		staircases [0] = new StairCase (false, false, true, false);
//		staircases [0].uneven = true;
		staircases [1] = new StairCase (true, false, true, false);
//		staircases [2] = new StairCase (false, false, true, false);
//		staircases [5] = new StairCase (true, true, false, false);
//		staircases [6] = new StairCase (true, true, true, true);
//		staircases [7] = new StairCase (true, true, true, false);
//		staircases [7].tunneling = true;
//		staircases [8] = new StairCase (true, true, true, false);
//		staircases [8].transparent = true;

		for (int i = 0; i < staircaseCount; i++)
		{
			staircases[i].results[0] = maxHeight * 0.2f;
			staircases[i].filename = filename;
		}
		currentStaircase = staircases [Random.Range (0, staircaseCount)];

		positionData = new byte[60 * 60 * 60 * 4 * 4];
		positionDataCount = 0;
		startTime = System.DateTime.Now;

		resetCluster ();
	}

	bool checkIfAvailableForSameBlock()
	{
		for (int i = 0; i < staircaseCount; i++)
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
		for (int i = 0; i < staircaseCount; i++)
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

			if (!checkIfAnyAvailable())
			{
				completed = true;
				byte[] dataToWrite = new byte[positionDataCount];
				System.Array.Copy(positionData, 0, dataToWrite, 0, positionDataCount);
				File.WriteAllBytes("output/tracking-"+filename, dataToWrite);
				return;
			}

			if (currentStaircase.finished())
			{
				while(true)
				{
					int randIndex = Random.Range(0,staircaseCount);
					if (!staircases[randIndex].finished())
					{
						currentStaircase = staircases[randIndex];
						break;
					}
				}

				restPeriod = true;

				return;
			}

//			if (blockCaseCount == 0)
//			{
//				while(true)
//				{
//					int randIndex = Random.Range(0,16);
//					if (!staircases[randIndex].finished())
//					{
//						currentStaircase = staircases[randIndex];
//						break;
//					}
//				}
//			}
//			else if (blockCaseCount < blockCases && checkIfAvailableForSameBlock())
//			{
//				while(true)
//				{
//					int randIndex = Random.Range(0,16);
//					if (!staircases[randIndex].finished()
//					    && staircases[randIndex].stereo == currentStaircase.stereo
//					    && staircases[randIndex].montion == currentStaircase.montion)
//					{
//						currentStaircase = staircases[randIndex];
//						break;
//					}
//				}
//			}
//			else
//			{
//				blockCaseCount = 0;
//				restPeriod = true;
//				return;
//			}

			stereo = currentStaircase.stereo;
			montion = currentStaircase.montion;
			density = currentStaircase.density;
			tunneling = currentStaircase.tunneling;
			size = false;
			transparent = currentStaircase.transparent;
			PLC = currentStaircase.PLC;
			currentDistance = Mathf.Min(currentStaircase.currentDistance(), maxHeight);

//			blockCaseCount++;
		}

		statusDisplay.GetComponent<TextMesh>().text = 
			(stereo?"stereo+":"")+(montion?"montion+":"")+(currentStaircase.forceMove?"force+":"")+(PLC?"PLC":"")+(transparent?"transparent":"")+(tunneling?"tunneling":"")+(currentStaircase.uneven?"uneven":"");

		OVRManager.instance.monoscopic = !stereo;
		OVRCameraRig.disablePositionTracking = !montion;
		leftMostX = character.position.x;
		rightMostX = character.position.x;
		if (currentStaircase.forceMove)
		{
			tipDisplay.GetComponent<TextMesh>().text = "This experiment requires motion";
		}
		else
		{
			tipDisplay.GetComponent<TextMesh>().text = "This experiment does not require motion";
		}

		int realFragCount = density ? fragCount : 0;

		bool leftFirst = Random.value > 0.5;

		if (redCube1 != null)
		{
			Destroy(redCube1.gameObject);
		}
		redCube1 = Instantiate (redCube) as Transform;
		redCube1.gameObject.SetActive (true);
		redCube1.position = new Vector3 (-maxHeight/4 + (Random.value-0.5f)*2*maxHeight/20*1.5f,
		                                 maxHeight/2 + (Random.value-0.5f)*2*maxHeight/20*1.5f,
		                                 maxHeight/2+(leftFirst?1:-1)*currentDistance/2);

		if (redCube2 != null)
		{
			Destroy(redCube2.gameObject);
		}
		redCube2 = Instantiate (redCube) as Transform;
		redCube2.gameObject.SetActive (true);
		redCube2.position = new Vector3 (maxHeight/4 + (Random.value-0.5f)*2*maxHeight/20*1.5f,
		                                 maxHeight/2 + (Random.value-0.5f)*2*maxHeight/20*1.5f,
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
				if (currentStaircase.uneven)
				{
					float x = Random.value*maxHeight-maxHeight/2;
					float y = Random.value*maxHeight;
					float z = 0;
					if (x < 0)
					{
						if (leftFirst == (Random.value < unevenRate))
						{
							z = Random.value*(maxHeight - redCube1.position.z)+redCube1.position.z;
						}
						else
						{
							z = Random.value*redCube1.position.z;
						}
					}
					else
					{
						if (!leftFirst == (Random.value < unevenRate))
						{
							z = Random.value*(maxHeight - redCube2.position.z)+redCube2.position.z;
						}
						else
						{
							z = Random.value*redCube2.position.z;
						}
					}
					cluster[i].position = new Vector3(x,y,z);
				}
				else
				{
					cluster[i].position = new Vector3(Random.value*maxHeight-maxHeight/2, Random.value*maxHeight, Random.value*maxHeight);
				}
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
				cluster[i].GetComponent<Renderer>().material.color = new Color(greyScale, greyScale, greyScale, transparent ? 0.4f : 1);
			}
			else
			{
				float greyScale = Random.value;
				cluster[i].GetComponent<Renderer>().material.color = new Color(greyScale, greyScale, greyScale, 1);
			}
		}

		observed = false;

		clusterTimestamp = System.DateTime.Now;
		
		Resources.UnloadUnusedAssets();
	}

	float unevenRandom()
	{
		return (Random.value * Random.value + Random.value)/2;
	}

	bool checkMovedEnough()
	{
		if (currentStaircase.forceMove == (rightMostX - leftMostX > 0.03))
			return true;
		else
			return false;
	}

	float testVisibility(Transform start, Transform end)
	{
		RaycastHit hit;
		int hitCount = 0;
		int totalCount = 0;
		for (float xStep = -0.006f; xStep <= 0.006f; xStep += 0.0001f)
		{
			for (float yStep = -0.006f; yStep <= 0.006f; yStep += 0.0001f)
			{
				Physics.Raycast(start.position, end.position-start.position + new Vector3(xStep, yStep, 0), out hit);
				if (hit.collider == end.gameObject.GetComponent<Collider> ())
				{
					hitCount++;
				}
				totalCount++;
			}
		}
		return (float)hitCount/totalCount*Mathf.Pow(12*Mathf.Sqrt(3)/(1.2f+end.position.z*2),2);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!completed)
		{
			Vector3 currentPosition = character.position;

			if (positionDataCount < 60*60*60*4*4-16)
			{
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
			}
			
			if (currentPosition.x < leftMostX)
			{
				leftMostX = currentPosition.x;
			}
			if (currentPosition.x > rightMostX)
			{
				rightMostX = currentPosition.x;
			}
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
			gracePeriod = false;
			maskCube.gameObject.SetActive(true);
			completeDisplay.GetComponent<TextMesh> ().text = "Please take a rest! You can resume anytime by pressing space key!";
		}
		else if (movementWarning && (System.DateTime.Now - clusterTimestamp).TotalSeconds < 2)
		{
			maskCube.gameObject.SetActive(true);
			completeDisplay.GetComponent<TextMesh> ().text = currentStaircase.forceMove ? "Please move left and right!" : "Please do NOT move your head!";
		}
		else if (movementWarning && (System.DateTime.Now - clusterTimestamp).TotalSeconds >= 2)
		{
			maskCube.gameObject.SetActive(false);
			movementWarning = false;
			resetCluster();
		}
		else if (!timeoutPeriod && (System.DateTime.Now - clusterTimestamp).TotalSeconds > 4)
		{
			timeoutPeriod = true;
			clusterTimestamp = System.DateTime.Now;
			maskCube.gameObject.SetActive(true);
			completeDisplay.GetComponent<TextMesh> ().text = "Too slow! Please try to make selection within 4 seconds!";
		}
		else if (timeoutPeriod && (System.DateTime.Now - clusterTimestamp).TotalSeconds > 2)
		{
			timeoutPeriod = false;
			maskCube.gameObject.SetActive(false);
			resetCluster();
		}
		else if (gracePeriod && (System.DateTime.Now - graceTimestamp).TotalSeconds > 0.5)
		{
			gracePeriod = false;
			maskCube.gameObject.SetActive(false);
		}
		
		if (Input.GetKeyDown("escape"))
		{
			Application.Quit();
		}
		else if (timeoutPeriod || gracePeriod || completed || movementWarning)
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
		else if (pseudoObserver)
		{
			if (!observed && (System.DateTime.Now - clusterTimestamp).TotalSeconds > 0.1)
			{
				float left1 = testVisibility(stereo?leftEye:monoEye, redCube1);
				float left2 = testVisibility(stereo?leftEye:monoEye, redCube2);
				float right1 = testVisibility(stereo?rightEye:monoEye, redCube1);
				float right2 = testVisibility(stereo?rightEye:monoEye, redCube2);
				float leftVisibility = left1 + right1;
				float rightVisibility = left2 + right2;

				if ((left1 >= left2) != (right1 >= right2))
				{
					currentStaircase.feedbackDisagree();
				}

				distanceDisplay.GetComponent<TextMesh> ().text = string.Format("{0}vs.{1}", leftVisibility/2, rightVisibility/2);

				observedResult = (leftVisibility > rightVisibility);

				observed = true;
			}

			if ((System.DateTime.Now - clusterTimestamp).TotalSeconds > 0.1)
			{
				if (observedResult)
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
					
//					gracePeriod = true;
//					graceTimestamp = System.DateTime.Now;
					//maskCube.gameObject.SetActive(true);
//					completeDisplay.GetComponent<TextMesh> ().text = "";
				}
				else
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
					
//					gracePeriod = true;
//					graceTimestamp = System.DateTime.Now;
					//maskCube.gameObject.SetActive(true);
//					completeDisplay.GetComponent<TextMesh> ().text = "";
				}
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
				
				gracePeriod = true;
				graceTimestamp = System.DateTime.Now;
				maskCube.gameObject.SetActive(true);
				completeDisplay.GetComponent<TextMesh> ().text = "";
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
				
				gracePeriod = true;
				graceTimestamp = System.DateTime.Now;
				maskCube.gameObject.SetActive(true);
				completeDisplay.GetComponent<TextMesh> ().text = "";
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
			//distanceDisplay.GetComponent<TextMesh> ().text = string.Format("{0}", currentStaircase.currentDistance());
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
