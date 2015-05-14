using UnityEngine;
using System.Collections;

public class scatterCluster : MonoBehaviour {

	public Transform fragment;
	public Transform redCube;
	public Transform wall;
	public Material right;
	public Material wrong;
	public int fragCount;
	public float maxHeight;

	private Transform[] cluster;
	private Transform redCube1, redCube2;

	// Use this for initialization
	void Start ()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;

		cluster = new Transform[fragCount];

		resetCluster ();
	}

	void resetCluster()
	{
		for (int i = 0; i < fragCount; i++)
		{
			if (cluster[i] != null)
			{
				Destroy(cluster[i].gameObject);
			}
			cluster[i] = Instantiate(fragment) as Transform;
			cluster[i].gameObject.SetActive(true);
			cluster[i].position = new Vector3(Random.value*maxHeight-maxHeight/2, Random.value*maxHeight, Random.value*10);
			cluster[i].rotation = Random.rotation;
		}

		if (redCube1 != null)
		{
			Destroy(redCube1.gameObject);
		}
		redCube1 = Instantiate (redCube) as Transform;
		redCube1.gameObject.SetActive (true);
		redCube1.position = new Vector3 (-Random.value * 5, Random.value * 10, Random.value * 10);

		if (redCube2 != null)
		{
			Destroy(redCube2.gameObject);
		}
		redCube2 = Instantiate (redCube) as Transform;
		redCube2.position = new Vector3 (Random.value * 5, Random.value * 10, Random.value * 10);
		redCube2.gameObject.SetActive (true);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown ("left")) 
		{
			if(redCube1.position.z <= redCube2.position.z)
			{
				wall.gameObject.GetComponent<Renderer>().material = right;
			}
			else
			{
				wall.gameObject.GetComponent<Renderer>().material = wrong;
			}

			resetCluster();
		}
		else if (Input.GetKeyDown("right"))
		{
			if(redCube1.position.z >= redCube2.position.z)
			{
				wall.gameObject.GetComponent<Renderer>().material = right;
			}
			else
			{
				wall.gameObject.GetComponent<Renderer>().material = wrong;
			}

			resetCluster();
		}
	}
}
