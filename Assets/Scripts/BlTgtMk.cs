using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlTgtMk : MonoBehaviour
{
	public BallCtl ball;
	
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
    	transform.position = ball.targetPos;
    }
    
}
