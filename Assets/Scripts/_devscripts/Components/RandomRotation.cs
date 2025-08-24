using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class RandomRotation : MonoBehaviour
{
	public float Rotation;

	// Update is called once per frame
	void Awake ()
	{
		Rotation = Random.Range(0, 360);
		var sp = GetComponent<SpriteRenderer>();
		
		transform.Rotate(Vector3.forward, Rotation);
	}
}
