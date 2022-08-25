using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Mirror;

public class CameraController : MonoBehaviour
{
	public float dumping = 1f;   //для скольжения камеры
	public GameObject[] players;
	public GameObject player;

	private int lastX;

	public Transform playerTransform;
	public int depth = -20;

	// Update is called once per frame
	void Update()
	{
		if (playerTransform != null)
		{
			transform.position = playerTransform.position + new Vector3(0, 3, depth);
		}
		else transform.position = new Vector3(0, 20, depth);
	}

	public void setTarget(Transform target)
	{
		playerTransform = target;
	}
}
