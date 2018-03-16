using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour {

	public float speed = 0.5f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.Translate (gameObject.transform.parent.transform.TransformDirection(Vector3.forward*Input.GetAxis ("Mouse ScrollWheel")*speed),Space.World);
	}
}
