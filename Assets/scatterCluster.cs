using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class scatterCluster : MonoBehaviour {

	public Transform fragment;
	public Transform redCube;
	public Transform display;
	public Transform display2;
	public Transform character;
	public Material right;
	public Material wrong;
	public int fragCount;
	public float maxHeight;
	public float tunnelWidth;
	public Text text;

	private Transform[] cluster;
	private Transform redCube1, redCube2;
	private Vector3 lastPos;

	// Use this for initialization
	void Start ()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;

		cluster = new Transform[fragCount];
		lastPos = Vector3.zero;

		resetCluster ();
	}

	void resetCluster()
	{
		bool stereo, montion, density, tunneling, size;
		float rand = Random.value;
		if (rand < 1.0/13)
		{
			stereo = false;
			montion = false;
			density = false;
			tunneling = false;
			size = false;
		}
		else if (rand < 5.0/13)
		{
			stereo = Random.value > 0.5;
			montion = Random.value > 0.5;
			density = false;
			tunneling = false;
			size = true;
		}
		else if (rand < 9.0/13)
		{
			stereo = Random.value > 0.5;
			montion = Random.value > 0.5;
			density = true;
			tunneling = false;
			size = true;
		}
		else
		{
			stereo = Random.value > 0.5;
			montion = Random.value > 0.5;
			density = true;
			tunneling = true;
			size = true;
		}

		stereo = true;
		montion = true;
		density = true;
		tunneling = false;
		size = false;

		text.text = (stereo?"stereo,":"")+(montion?"montion,":"")+(density?"density,":"")+(tunneling?"tunneling,":"")+(size?"size,":"");

		OVRManager.instance.monoscopic = stereo;
		OVRCameraRig.disablePositionTracking = !montion;
		int realFragCount = density ? fragCount : fragCount / 5;

		if (redCube1 != null)
		{
			Destroy(redCube1.gameObject);
		}
		redCube1 = Instantiate (redCube) as Transform;
		redCube1.gameObject.SetActive (true);
		redCube1.position = new Vector3 (-Random.value*maxHeight/2, Random.value*maxHeight, Random.value*maxHeight);
		if (size)
		{
			redCube1.localScale *= redCube1.position.z / 10 + 1;
		}
		
		if (redCube2 != null)
		{
			Destroy(redCube2.gameObject);
		}
		redCube2 = Instantiate (redCube) as Transform;
		redCube2.gameObject.SetActive (true);
		redCube2.position = new Vector3 (Random.value*maxHeight/2, Random.value*maxHeight, Random.value*maxHeight);
		if (size)
		{
			redCube2.localScale *= redCube2.position.z / 10 + 1;
		}

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
			if (size)
			{
				cluster[i].localScale *= cluster[i].position.z / 10 + 1;
			}
			cluster[i].rotation = Random.rotation;
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

		if (Input.GetKeyDown ("left")) 
		{
			if(redCube1.position.z <= redCube2.position.z)
			{
				display.gameObject.GetComponent<Renderer>().material = right;
				display2.gameObject.GetComponent<Renderer>().material = right;
			}
			else
			{
				display.gameObject.GetComponent<Renderer>().material = wrong;
				display2.gameObject.GetComponent<Renderer>().material = wrong;
			}

			resetCluster();
		}
		else if (Input.GetKeyDown("right"))
		{
			if(redCube1.position.z >= redCube2.position.z)
			{
				display.gameObject.GetComponent<Renderer>().material = right;
				display2.gameObject.GetComponent<Renderer>().material = right;
			}
			else
			{
				display.gameObject.GetComponent<Renderer>().material = wrong;
				display2.gameObject.GetComponent<Renderer>().material = wrong;
			}

			resetCluster();
		}
	}
}
