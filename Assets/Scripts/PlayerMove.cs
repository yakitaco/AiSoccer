using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;


public class PlayerMove : MonoBehaviour {
	public float speed = 0.5f;
	public float rotate = 0.5f;

	public float speeds =  0f;	//速度
	public float rotates =  0f;	//回転
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		speeds = CrossPlatformInputManager.GetAxisRaw("Vertical") * speed;
		rotates = CrossPlatformInputManager.GetAxisRaw("Horizontal") * rotate;
		
		GetComponent<Rigidbody>().AddForce( transform.forward *  speeds * 4.0f );
		//transform.Translate(Vector3.forward * Time.deltaTime * speeds );
		transform.eulerAngles += new Vector3 (0f, rotates, 0f);
		
		if ( transform.position.y < -10.00 ) {
			Destroy(gameObject, 1.0f);
        }
		
	}
}
