using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallKick : MonoBehaviour
{
	public float KickPowMin = 10.0f;
	public float YupMin = 10.0f;
	public float KickPowMax = 50.0f;
	public float YupMax = 100.0f;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
	//void OnCollisionEnter (Collision other)
	//{
    //    if (other.gameObject.tag == "Ball")
    //    {
    //        //other.gameObject.GetComponent<Rigidbody>().AddForce (transform.forward * Random.Range(KickPowMin, KickPowMax) + new Vector3(0.0f, Random.Range(YupMin, YupMax), 0.0f));
    //        //GetComponent<Rigidbody>().velocity = Vector3.zero;
	//		//GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	//		other.gameObject.GetComponent<BallCtl>().KickToTarget(Vector3.zero, 0.0f);
    //    }
	//}
    
}
