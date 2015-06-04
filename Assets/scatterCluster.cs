using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class scatterCluster : MonoBehaviour {

	public Transform fragment;
	public Transform redCube;
	public Transform character;
	public int fragCount;
	public float maxHeight;
	public float tunnelWidth;
	public GameObject statusDisplay;
	public GameObject feedbackDisplay;
	public Material transparentMat;
	public Light lightSource;

	private Transform[] cluster;
	private Transform redCube1, redCube2;
	private Vector3 lastPos;
	private bool stereo, montion, density, tunneling, size, transparent, PLC;

	// Use this for initialization
	void Start ()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;

		cluster = new Transform[fragCount];
		lastPos = Vector3.zero;
		
		stereo = true;
		montion = true;
		density = true;
		tunneling = true;
		size = true;
		transparent = false;
		PLC = false;

		resetCluster ();
	}

	void resetCluster()
	{
//		float rand = Random.value;
//		if (rand < 1.0/13)
//		{
//			stereo = false;
//			montion = false;
//			density = false;
//			tunneling = false;
//			size = false;
//		}
//		else if (rand < 5.0/13)
//		{
//			stereo = Random.value > 0.5;
//			montion = Random.value > 0.5;
//			density = false;
//			tunneling = false;
//			size = true;
//		}
//		else if (rand < 9.0/13)
//		{
//			stereo = Random.value > 0.5;
//			montion = Random.value > 0.5;
//			density = true;
//			tunneling = false;
//			size = true;
//		}
//		else
//		{
//			stereo = Random.value > 0.5;
//			montion = Random.value > 0.5;
//			density = true;
//			tunneling = true;
//			size = true;
//		}

		statusDisplay.GetComponent<TextMesh>().text = 
			(stereo?"stereo,":"")+(montion?"montion,":"")+(density?"density,":"")+(tunneling?"tunneling,":"")+(size?"size,":"")+(transparent?"transparent,":"")+(PLC?"PLC":"");

		//OVRManager.instance.monoscopic = stereo;
		//OVRCameraRig.disablePositionTracking = !montion;
		if (PLC)
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
		int realFragCount = density ? fragCount : fragCount / 5;

		if (redCube1 != null)
		{
			Destroy(redCube1.gameObject);
		}
		redCube1 = Instantiate (redCube) as Transform;
		redCube1.gameObject.SetActive (true);
		redCube1.position = new Vector3 (-Random.value*maxHeight/2, Random.value*maxHeight, Random.value*maxHeight);

		if (redCube2 != null)
		{
			Destroy(redCube2.gameObject);
		}
		redCube2 = Instantiate (redCube) as Transform;
		redCube2.gameObject.SetActive (true);
		redCube2.position = new Vector3 (Random.value*maxHeight/2, Random.value*maxHeight, Random.value*maxHeight);

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
				float alpha = transparent ? 0.4f : 1;
				if (transparent)
					cluster[i].GetComponent<Renderer>().material = transparentMat;
				cluster[i].GetComponent<Renderer>().material.color = new Color(greyScale, greyScale, greyScale, 0.4F);
			}
		}
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

		if (Input.GetKeyDown ("left")) 
		{
			if(redCube1.position.z <= redCube2.position.z)
			{
				feedbackDisplay.GetComponent<TextMesh>().text = "Correct!";
			}
			else
			{
				feedbackDisplay.GetComponent<TextMesh>().text = "Wrong!";
			}

			resetCluster();
		}
		else if (Input.GetKeyDown("right"))
		{
			if(redCube1.position.z >= redCube2.position.z)
			{
				feedbackDisplay.GetComponent<TextMesh>().text = "Correct!";
			}
			else
			{
				feedbackDisplay.GetComponent<TextMesh>().text = "Wrong!";
			}

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
	}

	void copyMaterial(GameObject anObject)
	{
		anObject.GetComponent<Renderer>().material = new Material (anObject.GetComponent<Renderer>().material);
	}
}
