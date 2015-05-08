using UnityEngine;
using System.Collections;

public class scatterCluster : MonoBehaviour {

	public Transform fragment;
	public Transform redCube;
	public int fragCount;

	private Transform[] cluster;

	// Use this for initialization
	void Start ()
	{
		Random.seed = (int)System.DateTime.Now.Ticks;

		cluster = new Transform[fragCount];
		for (int i = 0; i < fragCount; i++)
		{
			cluster[i] = Instantiate(fragment) as Transform;
			cluster[i].position = new Vector3(Random.value*10-5, Random.value*10, Random.value*10);
			cluster[i].rotation = Random.rotation;
		}

		Transform redCube1 = Instantiate (redCube) as Transform;
		redCube1.position = new Vector3 (-Random.value * 5, Random.value * 10, Random.value * 10);
		Transform redCube2 = Instantiate (redCube) as Transform;
		redCube2.position = new Vector3 (Random.value * 5, Random.value * 10, Random.value * 10);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
