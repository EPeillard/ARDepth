using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

public class Calibration : MonoBehaviour {

    public GameObject spatialMapping;

    GameObject cam;

	// Use this for initialization
	void Start () {
        cam = GameObject.Find("Main Camera");
        //spatialMapping.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            spatialMapping.SetActive(true);
        }
        if(Input.GetMouseButtonUp(0) && Input.GetMouseButton(1) || Input.GetMouseButtonUp(1) && Input.GetMouseButton(0) || Input.GetMouseButtonUp(1) && Input.GetMouseButtonUp(0))
        {
            var hit = new RaycastHit();
            if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit))
            {
                var pos = cam.transform.position;
                pos.y = hit.point.y;
                gameObject.transform.position = pos;

                /*var fwrdSpace = transform.TransformDirection(Vector3.forward);
                var fwrdCam = hit.point - cam.transform.position;
                fwrdCam.y = fwrdSpace.y = 0;
                var rot = Vector3.Angle(fwrdSpace,fwrdCam);*/
                gameObject.transform.LookAt(hit.point);

                var p = gameObject.GetComponentInChildren<Experiment>().gameObject.transform.position;
                p.y = cam.transform.position.y;
                gameObject.GetComponentInChildren<Experiment>().gameObject.transform.position = p;
            }
            spatialMapping.SetActive(false);
        }
	}
}
