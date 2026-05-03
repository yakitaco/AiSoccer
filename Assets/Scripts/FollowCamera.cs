using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

/* フォローカメラ */
public class FollowCamera : MonoBehaviour {

	static GameObject tmpTarget = null;
	static List<GameObject> targetList = new List<GameObject>();
	static int targetNum = 0;
	public GameObject camera;
    public bool isRote = false;
    public GameObject ballObj;

    public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
    public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.
	public float rotateSpeed = 0.2f;

	public float distance = 5.0f;
	public float y_heigt;
	public float x_angle;
	
	public float rads =  0f;	//target向き
	public Vector3 OffDist =  new Vector3(0f,0f,0f);	//target向き
	
	public float pos_slerp = 0.1f;
	public float rot_slerp = 0.05f;
	public float turnSpeed = 0.25f;
	
    //private GameObject player = null;
    private Vector3 tgtPosition = new Vector3(0f,0f,0f);	//target位置
    private Vector3 tgtAngles =  new Vector3(0f,0f,0f);	//target向き

    private bool existPlayer = true;
    private PlayerMove player_ctl;

	static FollowCamera instance;

	void Awake()
	{
        instance = this;
	}

    void Start () {
    	
    }

    void LateUpdate () {
    	Vector3 newPosition;
    	
    	//ターゲット変更
        if( CrossPlatformInputManager.GetButtonDown( "Pre" ) )
        {
            moveCameraTarget(-1);
        }
        if( CrossPlatformInputManager.GetButtonDown( "Next" ) )
        {
            moveCameraTarget(1);
        }
    	
    	
    	// ターゲットが存在する場合
    	if (tmpTarget != null) {
    		tgtPosition = tmpTarget.transform.position;
    		tgtAngles   = tmpTarget.transform.rotation.eulerAngles;
    	} else if (targetList[targetNum] != null) {
    		tgtPosition = targetList[targetNum].transform.position;
    		tgtAngles   = targetList[targetNum].transform.rotation.eulerAngles;
    	} else {
    	//プレイヤーが存在しない場合
    		tgtAngles += new Vector3 (0f , turnSpeed, 0f);
    	}
    	
    	// 回転有効
    	if (isRote){

	        Quaternion  newRotation = Quaternion.Euler(0.0f, tgtAngles.y, 0.0f);//tgtAngles.x+x_angle
	        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, rot_slerp);
    	}
    	
        newPosition.y = tgtPosition.y/2 + y_heigt;
		newPosition.x = tgtPosition.x;// - Mathf.Sin(x_angle) * (distance + targetList[targetNum].transform.position.y);
		newPosition.z = tgtPosition.z;// - Mathf.Cos(x_angle) * (distance + targetList[targetNum].transform.position.y);
		if ( newPosition.x < -GameManager.FieldXlen/2 ) {
			newPosition.x = -GameManager.FieldXlen/2;
		} else if ( newPosition.x > GameManager.FieldXlen/2 ) {
			newPosition.x = GameManager.FieldXlen/2;
		}
		if ( newPosition.z < -GameManager.FieldZlen/2 ) {
			newPosition.z = -GameManager.FieldZlen/2;
		} else if ( newPosition.z > GameManager.FieldZlen/2 ) {
			newPosition.z = GameManager.FieldZlen/2;
		}
		
		
	    transform.position = Vector3.Slerp(transform.position,newPosition, pos_slerp);

        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // If the camera is orthographic...
            if (camera.GetComponent<Camera>().orthographic)
            {
                // ... change the orthographic size based on the change in distance between the touches.
                GetComponent<Camera>().orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

                // Make sure the orthographic size never drops below zero.
                GetComponent<Camera>().orthographicSize = Mathf.Max(GetComponent<Camera>().orthographicSize, 0.1f);
            }
            else
            {
                // Otherwise change the field of view based on the change in distance between the touches.
                GetComponent<Camera>().fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;

                // Clamp the field of view to make sure it's between 0 and 180.
                GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView, 0.1f, 179.9f);
            }
        }
        
		// マウス入力がある
		if ((Input.GetMouseButton(0))&&(Input.mousePosition.y < Screen.height/10*9)) {
			// 左インプット → 左回転
			if (Input.mousePosition.x < Screen.width/4) transform.eulerAngles += new Vector3 (0f, rotateSpeed, 0f);
			// 右インプット → 右回転
			if (Input.mousePosition.x > Screen.width/4*3) transform.eulerAngles -= new Vector3 (0f, rotateSpeed, 0f);
			// 上インプット → 上回転
			if ((Input.mousePosition.y > Screen.height/4*3)&&((transform.eulerAngles.x > 270f)||(transform.eulerAngles.x < 80f))) transform.eulerAngles += new Vector3 (rotateSpeed, 0f, 0f);
			// 下インプット → 下回転
			if ((Input.mousePosition.y < Screen.height/4)&&((transform.eulerAngles.x > 280f)||(transform.eulerAngles.x < 90f))) transform.eulerAngles -= new Vector3 (rotateSpeed, 0f, 0f);
			//Debug.Log(transform.eulerAngles.x);
		}
		if (targetNum > 0){
			//targetの方に少しずつ向きが変わる
			//var Yrot = transform.rotation.x;
			var heading = ballObj.transform.position - transform.position;
			heading.y = heading.y/2.0f;
        	transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation (heading), 0.05f);
			//transform.eulerAngles = new Vector3(Yrot, transform.rotation.y, transform.rotation.z);
			
			//var heading = ballObj.transform.position - transform.position;
			//heading.y = 0;
			//var look = Quaternion.LookRotation(heading);
			//this.transform.localRotation = look;
		}

    }
    
    /* カメラターゲットの一時強制変更 */
    public static void setTempCameraTarget(GameObject _target){
    	tmpTarget = _target;
    }
    
    /* カメラターゲットの一時強制解除 */
    public static void clearTempCameraTarget(){
    	tmpTarget = null;
    }
    
    /* カメラターゲットを変更 */
    public static void moveCameraTarget(int variations){
    	targetNum += variations;
	    if (targetNum < 0){
	    	targetNum += ((-targetNum)/targetList.Count+1)*targetList.Count;
	    } else if (targetNum >= targetList.Count) {
	    	targetNum -= (targetNum/targetList.Count)*targetList.Count;
	    }
	    Debug.Log("CAMERA:" + targetNum);
    }
    
    public static void addCameraTarget(GameObject _target){
    	targetList.Add(_target);
    }
    
    public static void removeCameraTarget(GameObject _target){
    	if (targetList.IndexOf(_target) <= targetNum) moveCameraTarget(-1);
    	targetList.Remove(_target);
    }
    

    
    
    
}