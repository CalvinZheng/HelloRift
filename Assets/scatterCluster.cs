using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class Staircase
{
	public bool stereo;
	public bool montion;
	public bool density;
	public bool PLC;
	public bool forceMove;
	public bool transparent;
	public bool tunneling;
	public bool uneven;
	public bool widen;
	public bool hollow;
	public bool strip;
	public bool size;
	public bool randomSize;
	public int reversalCount;
	public float[] results;
	public float[] reversalResults;
	public float finalResult;
	public int currentStep;
	public string filename;

	static int reversalMax = 12;
	static float stepDownRatio = 0.8f;
	static float stepUpRatio = 2.19f;
	static int finalResultCount = 10;

	private bool lastFeedback;

	//If you want the experiments to run at specified distances for multiple time, instead of staircase method, set samplingMode to true. We usually run idealObserver using this mode.
	static bool samplingMode = false;
	static private int sampleNumber = 20;
	static private int totalTrials = 100000;
	static private float initDistance = 0.2f;
	private int currentLevel;
	private int[] rightCount;
	private int[] wrongCount;
	private int[] disagreeCount;

	public Staircase(bool stereo, bool montion, bool density, bool hollow, bool uneven, bool widen, bool strip)
	{
		this.stereo = stereo;
		this.montion = montion;
		this.density = density;
		this.PLC = false;

		this.forceMove = montion;
		this.transparent = false;
		this.tunneling = false;
		this.uneven = uneven;
		this.widen = widen;
		this.hollow = hollow;
		this.strip = strip;
		if (strip)
		{
			this.widen = false;
		}
		this.size = false;
		this.randomSize = false;

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
		if (samplingMode)
			return initDistance * Mathf.Pow (stepDownRatio, currentLevel);
		else
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

		if (samplingMode)
		{
			wrongCount [currentLevel]++;

			if (currentStep >= totalTrials)
			{
				outputResult();
				return;
			}
			else
			{
				currentLevel = Random.Range (0, sampleNumber);
			}
		}
		else
		{
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
				results[currentStep+1] = results[currentStep]*stepUpRatio;
			}
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
		
		if (samplingMode)
		{
			rightCount [currentLevel]++;

			if (currentStep >= totalTrials)
			{
				outputResult();
				return;
			}
			else
			{
				currentLevel = Random.Range (0, sampleNumber);
			}
		}
		else
		{
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
		}
		
		currentStep++;
	}

	public void feedbackDisagree()
	{
		disagreeCount [currentLevel]++;
	}

	public string conditionLabel()
	{
		return "("+(strip?"bar+":"")+(stereo?"stereo+":"")+(montion?"motion+":"")+(!density?"base+":"")+(size?"size+":"")+(randomSize?"Rsize+":"")+(uneven?"50/50+":"")+(hollow?"hollow+":"")+")";
	}

	public void outputResult()
	{
		if (currentStep >= finalResultCount)
		{
			finalResult = 0;
			if (!samplingMode)
			{
				for (int i = 0; i < finalResultCount; i++)
				{
					finalResult += Mathf.Log(reversalResults[reversalMax - i - 1], 2);
				}
				finalResult /= finalResultCount;
				finalResult = Mathf.Pow(2, finalResult);
			}
			
			StreamWriter sw = File.AppendText("output/staircase-"+filename);
			string label = conditionLabel();
			if (!samplingMode)
			{
				sw.Write(label+",");
				for (int i = 0; i <= currentStep; i++)
				{
					sw.Write("{0},", results[i]);
				}
				sw.Write("\n");
			}
			sw.Close();

			sw = File.AppendText("output/result-"+filename);
			sw.WriteLine(label+",{0}", finalResult);
			if (samplingMode)
			{
				for (int i = 0; i < sampleNumber; i++)
				{
					sw.WriteLine("{0},{1},{2}", initDistance * Mathf.Pow (stepDownRatio, i), (float)rightCount[i]/(wrongCount[i]+rightCount[i]), (float)disagreeCount[i]/(wrongCount[i]+rightCount[i]));
				}
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
		if (samplingMode)
		{
			return (float)currentStep / totalTrials;
		}
		else
		{
			return (float)reversalCount / reversalMax;
		}
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
	public Transform shields;
	public Transform shield1;
	public Transform shield2;

	private Transform[] cluster;
	private Transform redCube1, redCube2;
	private bool stereo, montion, density, tunneling, size, transparent, PLC;
	private string filename;
	private Staircase[] staircases;
	private Staircase currentStaircase;
	private int staircaseCount;
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
	private Vector3 redCube1Scale;
	private Vector3 redCube2Scale;
	private float[] clusterScales;

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

	//Wanna use other experiment conditions? change the count here and follow it with staircase objects of your choosing.
		staircaseCount = 26;
		staircases = new Staircase[staircaseCount];

        //*EXAMPLE* new StairCase(istereo, imontion, dense, hollow, uneven, widen, strip);

        staircases[0] = new Staircase(true, true, false, false, false, true, false);
        staircases[0].size = false;

        for (int i = 0; i < 14; i++)
        {
            bool hollow = i % 4 == 1 || i % 4 == 2;
            bool uneven = i % 4 == 1 || i % 4 == 3;
            bool isize = false;
            bool istereo = i / 4 == 0 || i / 4 == 2;
            bool imotion = i / 4 == 1 || i / 4 == 2;
            staircases[i + 1] = new Staircase(istereo, imotion, true, hollow, uneven, true, false);
            staircases[i + 1].size = isize;
        }

        for (int i = 0; i < 11; i++)
        {
            bool hollow = i % 3 == 1;
            bool uneven = i % 3 == 1 || i % 3 == 2;
            bool istereo = i / 3 == 0 || i / 3 == 2;
            bool imotion = i / 3 == 1 || i / 3 == 2;
            staircases[i + 15] = new Staircase(istereo, imotion, true, hollow, uneven, false, true);
            staircases[i + 15].size = false;
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

		redCube1Scale = Vector3.zero;
		redCube2Scale = Vector3.zero;
		clusterScales = new float[fragCount];

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

//			if (checkIfAvailableForSameBlock())
//			{
//				while(true)
//				{
//					int randIndex = Random.Range(0,staircaseCount);
//					if (!staircases[randIndex].finished() && staircases[randIndex].stereo == currentStaircase.stereo && staircases[randIndex].montion == currentStaircase.montion)
//					{
//						currentStaircase = staircases[randIndex];
//						break;
//					}
//				}
//			}
//			else
//			{
//				while(true)
//				{
//					int randIndex = Random.Range(0,staircaseCount);
//					if (!staircases[randIndex].finished())
//					{
//						currentStaircase = staircases[randIndex];
//						break;
//					}
//				}
//
//				restPeriod = true;
//
//				return;
//			}

			stereo = currentStaircase.stereo;
			montion = currentStaircase.montion;
			density = currentStaircase.density;
			tunneling = currentStaircase.tunneling;
			size = currentStaircase.size;
			transparent = currentStaircase.transparent;
			PLC = currentStaircase.PLC;
			currentDistance = Mathf.Min(currentStaircase.currentDistance(), maxHeight+0.02f);
		}

		statusDisplay.GetComponent<TextMesh>().text = currentStaircase.conditionLabel();

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

		bool rightFirst = Random.value > 0.5;

		if (redCube1 != null)
		{
			Destroy(redCube1.gameObject);
		}
		if (redCube2 != null)
		{
			Destroy(redCube2.gameObject);
		}
		if (currentStaircase.strip)
		{
			redCube1 = Instantiate (redCube) as Transform;
			redCube1.gameObject.SetActive (true);
			redCube1.localScale += new Vector3 (maxHeight*2, 0, 0);
			redCube1.position = new Vector3 (0,
			                                 maxHeight/3*2,
			                                 maxHeight/2+(rightFirst?1:-1)*currentDistance/2);
			
			redCube2 = Instantiate (redCube) as Transform;
			redCube2.gameObject.SetActive (true);
			redCube2.localScale += new Vector3 (maxHeight*2, 0, 0);
			redCube2.position = new Vector3 (0,
			                                 maxHeight/3,
			                                 maxHeight/2+(!rightFirst?1:-1)*currentDistance/2);

			shields.gameObject.SetActive(true);
		}
		else
		{
			bool leftBig = (Random.value > 0.5f);

			redCube1 = Instantiate (redCube) as Transform;
			redCube1.gameObject.SetActive (true);
			redCube1.localScale += new Vector3 (currentStaircase.widen ? redCube.localScale.x : 0, 0, 0);
			if (currentStaircase.randomSize)
				redCube1.localScale *= (leftBig ? 1.2f : 0.8f);
			redCube1.position = new Vector3 (-maxHeight/4 - (currentStaircase.widen ? redCube.localScale.x/2 : 0) + (Random.value-0.5f)*2*maxHeight/20,
			                                 maxHeight/2 + (Random.value-0.5f)*2*maxHeight/20*1.5f,
			                                 maxHeight/2+(rightFirst?1:-1)*currentDistance/2);

			redCube2 = Instantiate (redCube) as Transform;
			redCube2.gameObject.SetActive (true);
			redCube2.localScale += new Vector3 (currentStaircase.widen ? redCube.localScale.x : 0, 0, 0);
			if (currentStaircase.randomSize)
				redCube2.localScale *= (!leftBig ? 1.2f : 0.8f);
			redCube2.position = new Vector3 (maxHeight/4 + (currentStaircase.widen ? redCube.localScale.x/2 : 0) + (Random.value-0.5f)*2*maxHeight/20,
			                                 maxHeight/2 + (Random.value-0.5f)*2*maxHeight/20*1.5f,
			                                 maxHeight/2+(!rightFirst?1:-1)*currentDistance/2);

			shields.gameObject.SetActive(false);
		}

		redCube1Scale = redCube1.localScale;
		redCube2Scale = redCube2.localScale;

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
					float usingUnevenRate = unevenRate;
					if (currentStaircase.hollow)
					{
						usingUnevenRate = (maxHeight-currentDistance)/2/maxHeight;
					}
					bool leftOrUpSide = (currentStaircase.strip ? (y > maxHeight/2) : (x < 0));

					if (leftOrUpSide)
					{
						float frontLine = (currentStaircase.hollow ? Mathf.Min(redCube1.position.z, redCube2.position.z) : redCube1.position.z);
						float backLine = (currentStaircase.hollow ? Mathf.Max(redCube1.position.z, redCube2.position.z) : redCube1.position.z);
						if (rightFirst == (Random.value < usingUnevenRate))
						{
							z = Random.value*(maxHeight - backLine)+backLine;
                            //cluster[i].gameObject.SetActive(false);
						}
						else
						{
							z = Random.value*frontLine;
                           //cluster[i].gameObject.SetActive(false);
                        }
					}
					else
					{
						float frontLine = (currentStaircase.hollow ? Mathf.Min(redCube1.position.z, redCube2.position.z) : redCube2.position.z);
						float backLine = (currentStaircase.hollow ? Mathf.Max(redCube1.position.z, redCube2.position.z) : redCube2.position.z);
						if (!rightFirst == (Random.value < usingUnevenRate))
						{
							z = Random.value*(maxHeight - backLine)+backLine;
                            //cluster[i].gameObject.SetActive(false);
                        }
						else
						{
							z = Random.value*frontLine;
                            //cluster[i].gameObject.SetActive(false);
                        }
					}
					cluster[i].position = new Vector3(x,y,z);
				}
				else if (currentStaircase.hollow)
				{
					if (Random.value > 0.5f)
					{
						cluster[i].position = new Vector3(Random.value*maxHeight-maxHeight/2, Random.value*maxHeight, Random.value*Mathf.Min (redCube1.position.z, redCube2.position.z));
					}
					else
					{
						cluster[i].position = new Vector3(Random.value*maxHeight-maxHeight/2, Random.value*maxHeight, 
						                                  Random.Range(Mathf.Max (redCube1.position.z, redCube2.position.z), maxHeight));
					}
				}
				else
				{
					cluster[i].position = new Vector3(Random.value*maxHeight-maxHeight/2, Random.value*maxHeight, Random.value*maxHeight);
				}

				Bounds clusterBounds = cluster[i].GetComponent<Collider>().bounds;
				clusterBounds.extents = new Vector3(1,1,1)*fragment.localScale.x*Mathf.Sqrt(2)/2;

				if (redCube1.GetComponent<Collider>().bounds.Intersects(clusterBounds))
				{
					if (cluster[i].position.z > redCube1.position.z)
					{
						cluster[i].position += new Vector3(0, 0, fragment.localScale.x*Mathf.Sqrt(2)/2);
					}
					else
					{
						cluster[i].position -= new Vector3(0, 0, fragment.localScale.x*Mathf.Sqrt(2)/2);
					}
				}
				if (redCube2.GetComponent<Collider>().bounds.Intersects(clusterBounds))
				{
					if (cluster[i].position.z > redCube2.position.z)
					{
						cluster[i].position += new Vector3(0, 0, fragment.localScale.x*Mathf.Sqrt(2)/2);
					}
					else
					{
						cluster[i].position -= new Vector3(0, 0, fragment.localScale.x*Mathf.Sqrt(2)/2);
					}
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
			clusterScales[i] = Random.Range(0.8f, 1.2f);
			cluster[i].localScale *= clusterScales[i];
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
		float minX = 999, maxX = -999, minY = 999, maxY = -999;
        float accurancy = 0.0005f;
		for (float xStep = -end.localScale.x/2+ accurancy; xStep < end.localScale.x/2- accurancy; xStep += accurancy)
		{
			for (float yStep = -end.localScale.y/2+ accurancy; yStep < end.localScale.y/2- accurancy; yStep += accurancy)
			{
				Physics.Raycast(start.position, end.position-start.position + new Vector3(xStep, yStep, 0), out hit);
				if (hit.collider != shield1.gameObject.GetComponent<Collider> ()
				    && hit.collider != shield2.gameObject.GetComponent<Collider> () )
				{
					totalCount++;
				}
				if (hit.collider == end.gameObject.GetComponent<Collider> ())
				{
					//Debug.DrawLine(start.position, end.position + new Vector3(xStep, yStep, 0), Color.red, 3, false);
					hitCount++;
					if (xStep < minX)
						minX = xStep;
					if (xStep > maxX)
						maxX = xStep;
					if (yStep < minY)
						minY = yStep;
					if (yStep > maxY)
						maxY = yStep;
				}
			}
		}
		float result = (float)hitCount / totalCount;
		if (currentStaircase.size)
		{
			result *= Mathf.Pow(12*Mathf.Sqrt(3)/(1.2f+end.position.z*2),2);
		}

		if (currentStaircase.size && (currentStaircase.uneven || currentStaircase.hollow) && !(currentStaircase.uneven && currentStaircase.hollow))
		{
			result = Mathf.Max (maxX - minX, maxY - minY);
			result /= end.position.z - start.position.z;
		}
		return result;
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
		
		if (!size)
		{
			redCube1.localScale = redCube1Scale * (character.position.z - redCube1.position.z) / (character.position.z - maxHeight/2);
			redCube2.localScale = redCube2Scale * (character.position.z - redCube2.position.z) / (character.position.z - maxHeight/2);
//			for (int i = 0; i < fragCount; i++)
//			{
//				if (cluster[i] != null)
//				{
//					cluster[i].localScale = fragment.localScale * clusterScales[i] * (character.position.z - cluster[i].position.z) / (character.position.z - maxHeight/2);
//				}
//			}
		}
		else
		{
			redCube1.localScale = redCube1Scale;
			redCube2.localScale = redCube2Scale;
			for (int i = 0; i < fragCount; i++)
			{
				if (cluster[i] != null)
				{
					cluster[i].localScale = fragment.localScale * clusterScales[i];
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
			if (Input.GetKeyDown ("space") || pseudoObserver)
			{
				restPeriod = false;
				maskCube.gameObject.SetActive(false);
				resetCluster();
			}
		}
		else if (pseudoObserver)
		{
			if (!observed && (System.DateTime.Now - clusterTimestamp).TotalSeconds > 0.01)
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

                if (leftVisibility == rightVisibility)
                    observedResult = (Random.value > 0.5 ? true : false);

				observed = true;
			}

			if ((System.DateTime.Now - clusterTimestamp).TotalSeconds > 0.01)
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
		else if (!continousMode && ((!currentStaircase.strip && Input.GetKeyDown ("left")) || (currentStaircase.strip && Input.GetKeyDown ("up")))) 
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
		else if (!continousMode && ((!currentStaircase.strip && Input.GetKeyDown("right")) || (currentStaircase.strip && Input.GetKeyDown ("down"))))
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
			foreach (Staircase aStaircase in staircases)
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
