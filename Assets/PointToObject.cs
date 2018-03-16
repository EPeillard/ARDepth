using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointToObject : MonoBehaviour {

    public string name;
	public GameObject objectToPointTo;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!objectToPointTo)
        {
            objectToPointTo = GameObject.Find(name);
        }

        if(objectToPointTo)
        {
            foreach (Light l in this.transform.GetComponentsInChildren<Light>())
            {
                l.gameObject.transform.LookAt(objectToPointTo.transform);
            }
        }
	}
}
