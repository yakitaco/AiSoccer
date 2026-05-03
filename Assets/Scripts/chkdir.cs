using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chkdir : MonoBehaviour
{
	public Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    	var diff = target - transform.position;
    	//var angle = diff / diff.magnitude - transform.eulerAngles;
    	var axis = Vector3.Cross( transform.forward, diff );
        var angle = Vector3.Angle( transform.forward, diff ) * (axis.y < 0 ? -1 : 1) ;
        Debug.LogWarning( "dir:" + angle);
        //var angle =diff;
        //Debug.LogWarning( "dir:" + angle);
    }
}
