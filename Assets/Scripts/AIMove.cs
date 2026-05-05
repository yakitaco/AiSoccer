using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AM1.Nav;

public class AIMove : MonoBehaviour {
	public Transform target;
	//UnityEngine.AI.NavMeshAgent agent;
	private NavController sendObject;
	[Header("AIチーム用通し番号(0オリジン)")]
	public int AInum;
	[Header("背番号")]
	[SerializeField]
	int Number;
	public AiTeamCtl AiTeam;
	[Header("ボール")]
	public GameObject ball;
	
	[Header("スタミナ")]
	[SerializeField]
	float stamina = 1000.0f;
	[Header("スタミナ初期")]
	[SerializeField]
	float staminaOrg = 1000.0f;
	[Header("スタミナ最大")]
	[SerializeField]
	public float staminaMax = 1000.0f;
	[Header("疲労度(10.0f～20.0f)")]
	[SerializeField]
	public float fatiguex = 20.0f;
	
	[SerializeField]
	float dist = 0.0f;
	
	[SerializeField]
	bool isBallCatch = false; //GKがキャッチ状態
	
	[SerializeField]
	private Animator animator;
	
	[SerializeField]
	private lookAt lookat;
	
	pMove pM;
    Rigidbody rb;
	
	private Text   numView;
	private Slider stmSlider;
	private Slider stmMaxSlider;
	
	[SerializeField]
	bool forceKick = false; //強制キック
	[SerializeField]
	int keepTouch = 0;		//保持中
	
	private IEnumerator GkCoroutine; //GKキャッチ後のリリース用コルーチン
	
	// Use this for initialization
	void Start () {
		//agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		sendObject = GetComponent<NavController>();
		pM = GetComponent<pMove>();
		stmSlider = transform.Find("objUI").transform.Find("stmSlider").GetComponent<Slider>();
		stmMaxSlider = transform.Find("objUI").transform.Find("stmMaxSlider").GetComponent<Slider>();
		numView = transform.Find("objUI").transform.Find("numberView").GetComponent<Text>();
		stmSlider.maxValue = staminaOrg;
		stmMaxSlider.maxValue = staminaOrg;
		numView.text = ""+Number;
		FollowCamera.addCameraTarget(this.gameObject);
		rb = this.GetComponent<Rigidbody> ();
	}

	void Update () {
		if (AiTeam != null){
			//agent.SetDestination(AiTeam.tgtPos[AInum]);
			//sendObject.SetDestination(AiTeam.tgtPos[AInum]);
			pM.setDist(AiTeam.tgtPos[AInum]);
			pM.setView(ball.GetComponent<BallCtl>().getViewPos());

			if (rb.linearVelocity.magnitude > 3.0f){
				stamina -= rb.linearVelocity.magnitude - 3.0f;
				staminaMax -= (rb.linearVelocity.magnitude - 3.0f)/ fatiguex;
			} else if ((stamina < staminaMax)&&(rb.linearVelocity.magnitude < 2.0f)) {
				stamina += 2.5f - rb.linearVelocity.magnitude;
			}

			float sqrDistToTarget = (ball.GetComponent<BallCtl>().targetPos - AiTeam.tgtPos[AInum]).sqrMagnitude;
			if ((sqrDistToTarget > 400.0f)&&(stamina < 300.0f)&&(stamina/staminaMax < 0.9f)) {
				pM.setSpeed(0.0f);
			} else if (sqrDistToTarget > 900.0f){
				pM.setSpeed(1.0f);
			} else if ((sqrDistToTarget > 225.0f)||(stamina < 0.0f)) {
				pM.setSpeed(2.0f);
			} else {
				pM.setSpeed(4.0f);
			}
			dist = Mathf.Sqrt(sqrDistToTarget);
			
			stmSlider.value = stamina;
			stmMaxSlider.value = staminaMax;
		
        	animator.SetFloat("Speed",rb.linearVelocity.magnitude / 2.0f + 0.5f);
		}
		
		// GKがキャッチ状態
		if (isBallCatch){
			keepTouch = 0;
			ball.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
	    	ball.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			ball.gameObject.transform.position = transform.position + transform.forward * 0.25f + new Vector3(0.0f, 0.3f, 0.0f);
			if ((AiTeam.Wpos2Tpos(ball.gameObject.transform.position).x > -30.0f)||(AiTeam.Wpos2Tpos(ball.gameObject.transform.position).x < -40.0f)){
				//StopCoroutine(GkCoroutine);
				isBallCatch = false;
				forceKick = true;
				GameManager.doGKCatch(false);
				GkCoroutine = null;
				ball.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + transform.forward * 1.0f, 0.0f);
			}
		}
		
	}

	//初期設定
	public void SetInit (AiTeamCtl _AiTeam, int _Ainum,int _Number, string name, GameObject unit, float _stamina, float _fatiguex)
	{
		AInum = _Ainum;
		AiTeam = _AiTeam;
		Number = _Number;
		stamina = _stamina;
		staminaOrg = _stamina;
		staminaMax = _stamina;
		fatiguex = _fatiguex;
		gameObject.tag = name;
		gameObject.name = name + "_" + _Number;
		GameObject _unit = Instantiate(unit, transform.position, Quaternion.identity);
		_unit.transform.SetParent(transform);
		_unit.transform.localScale = new Vector3(0.05f, 0.06f, 0.05f);
		animator=_unit.GetComponent<Animator>();
		GetComponent<lookAt>().watchTarget = ball;
		GetComponent<lookAt>().neckBone = _unit.transform.Find("Armature").transform.Find("Root_jnt").transform.Find("Torso_jnt").transform.Find("Head_jnt");
		//_unit.transform.Find("Armature").transform.Find("Root_jnt").transform.Find("Torso_jnt").transform.Find("Head_jnt").GetComponent<lookAt>().watchTarget = ball;
	}
	
	//終了設定
	public void SetDestroy()
	{
		FollowCamera.removeCameraTarget(this.gameObject);
    	Destroy(this.gameObject,0.5f);
	}
	
	void OnCollisionEnter (Collision other)
	{
	//Debug.Log("OnCollisionEnter:" + Number);
	
	    if (other.gameObject.tag == "Ball") {
	    	keepTouch = 0;
	    	if (GameManager.isKick()){ //キックフラグON
	    		var tgtObj = chkOptPass(AiTeam.players, 10.0f, 50.0f);
		        if (tgtObj != null){
	    			other.gameObject.GetComponent<BallCtl>().ShootByTime(tgtObj.transform.position + tgtObj.transform.forward * tgtObj.GetComponent<Rigidbody>().linearVelocity.magnitude * 5.0f, 0.0f);
					Debug.Log("BYBYBY:" + AiTeam.getWgpos());
	    		} else {
	    			other.gameObject.GetComponent<BallCtl>().ShootByTime(AiTeam.getWgpos(), 0.0f);
	    			Debug.Log("GAGAGA:" + AiTeam.getWgpos());
	    		}
	    		transform.position -= transform.forward * 0.1f; // 少し後ろにずれないと正確に蹴れない？？？
	    		forceKick = false;
	    		GameManager.doKick();
	    	} else if (forceKick == true){
	    		var dir = getRelativeDir(AiTeam.getWgpos());
		        if ((dir > -90.0f)&&(dir < 90.0f)){
		    		var tgtObj = chkOptPass(AiTeam.players, 10.0f, 50.0f);
			        if (tgtObj != null){
		    			other.gameObject.GetComponent<BallCtl>().ShootByTime(tgtObj.transform.position + tgtObj.transform.forward * tgtObj.GetComponent<Rigidbody>().linearVelocity.magnitude * 5.0f, 0.0f);
		    		} else {
		    			other.gameObject.GetComponent<BallCtl>().ShootByTime(AiTeam.getWgpos(), 0.0f);
		    		}
		    		forceKick = false;
		        } else {
			        if (dir < -60.0f){
			        	dir = -60.0f;
			        	other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 2.0f, 0.0f);
			        } else if (dir > 60.0f){
			        	dir = 60.0f;
			        	other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 2.0f, 0.0f);
			        } else {
		        		other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 3.0f, 0.0f);
		        	}
		        }
	    	} else if (( AInum == 0)&&( AiTeam.teamId != GameManager.isBallTeamId() )&&(AiTeam.Wpos2Tpos(other.gameObject.transform.position).x < -30.0f)) {  //GKの場合
	    		//ボールキャッチ
	    		isBallCatch = true;
	    	    other.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
	    		other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	    		GameManager.doGKCatch(true);
	    		GkCoroutine = doGKkick(7.0f);
	    		StartCoroutine(GkCoroutine);
	    	} else {
		        var dir = getRelativeDir(AiTeam.getWgpos());
		        if (( AiTeam.Wpos2Tpos(other.gameObject.transform.position).x > 25.0f )&&(dir > -45.0f)&&(dir < 45.0f)){
		        	//Debug.Log("Shoot:" + AiTeam.getWgpos());
		        	other.gameObject.GetComponent<BallCtl>().ShootByTime(AiTeam.getWgpos() + AiTeam.Wpos2Tpos(new Vector3(Random.Range(-3.0f, 5.0f), Random.Range(-2.0f, 2.0f), Random.Range(-5.0f, 5.0f))), 0.0f);
		    	} else if ((checkCloseEnemy(transform.position) < 49.0f)||((AiTeam.Wpos2Tpos(other.gameObject.transform.position).x < -20.0f)&&(checkCloseEnemy(transform.position) < 100.0f))) {

		        	var tgtObj = chkOptPass(AiTeam.players, 5.0f, 30.0f); //味方を探す
		        	if (tgtObj == null){
		        	    if (AiTeam.Wpos2Tpos(other.gameObject.transform.position).x < 0.0f){ // 自陣の場合はクリア優先
		        	    	StartCoroutine("stepStop");
					    	if (dir < -90.0f){
					    		other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, -90.0f, 0.0f)) * transform.forward * 50.0f, 0.0f);
					    	} else if (dir > 90.0f){
					    		other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f,  90.0f, 0.0f)) * transform.forward * 50.0f, 0.0f);
					    	} else {
					    		other.gameObject.GetComponent<BallCtl>().ShootByTime(AiTeam.getWgpos(), 0.0f);
					    	}
					    	//Debug.Log("Clear:" + AiTeam.getWgpos());
				    	} else { // 敵陣の場合はドリブル突破を図る
				        	if (dir < -60.0f){
				        		dir = -60.0f;
				        		other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 2.0f, 0.0f);
				        	} else if (dir > 60.0f){
				        		dir = 60.0f;
				        		other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 2.0f, 0.0f);
				        	} else {
				        		dir = Random.Range(-60.0f, 60.0f);
			        			other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 3.0f, 0.0f);
			        		}
			        		//Debug.Log("Dribble:" + AiTeam.getWgpos());
				    	}
		        	} else {
		        		StartCoroutine("stepStop");
		        		other.gameObject.GetComponent<BallCtl>().ShootByTime(tgtObj.transform.position + tgtObj.transform.forward * tgtObj.GetComponent<Rigidbody>().linearVelocity.magnitude * 5.0f, 0.0f);
		        	}
	        	} else {
		        	//if (other.gameObject.GetComponent<Rigidbody>().velocity.magnitude < 5.0f ){
			        	if (dir < -60.0f){
			        		dir = -60.0f;
			        		other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 2.0f, 0.0f);
			        	} else if (dir > 60.0f){
			        		dir = 60.0f;
			        		other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 2.0f, 0.0f);
			        	} else {
		        			other.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + Quaternion.Euler(new Vector3(0.0f, dir, 0.0f)) * transform.forward * 2.0f, 0.0f);
		        		}
		        	//}
	        	}
	        	if (Random.Range(0, 5)==1) forceKick = true;
        	}
        	if (GameManager.isInplay()){ //プレー中
        		GameManager.setBallTeamId(AiTeam.isTeamId());
        		other.gameObject.GetComponent<BallCtl>().setLastTouch(this.gameObject);
        	}
		}
	}
	
	void OnCollisionStay (Collision other)
	{
	//Debug.Log("OnCollisionStay:" + Number);
	
	    if (other.gameObject.tag == "Ball") {
			keepTouch++;
			if (keepTouch > 20){ //強制聞く
	    		var tgtObj = chkOptPass(AiTeam.players, 10.0f, 50.0f);
		        if (tgtObj != null){
	    			other.gameObject.GetComponent<BallCtl>().ShootByTime(tgtObj.transform.position + tgtObj.transform.forward * 3.0f, 0.0f);
	    		} else {
	    			other.gameObject.GetComponent<BallCtl>().ShootByTime(AiTeam.getWgpos(), 0.0f);
	    		}
	    		forceKick = false;
	    		GameManager.doKick();
	    	}
		} else {
			if (Random.Range(0, 5)==1) transform.position -= transform.forward * 0.01f;
			if (Random.Range(0, 5)==1) transform.position += transform.right * 0.01f;
			if (Random.Range(0, 5)==1) transform.position -= transform.right * 0.01f;
		}
		
	}
	
	//最適なパス出し先を選択
	GameObject chkOptPass(GameObject[] list, float minDist, float maxDist){
		GameObject tgtObj = null;
        float sqrMinDist = minDist * minDist;
        float sqrMaxDist = maxDist * maxDist;

		foreach(var obj in list){
			if ((this != obj)&&((AiTeam.Wpos2Tpos(this.gameObject.transform.position).x < 0.0f)||(AiTeam.Wpos2Tpos(obj.gameObject.transform.position).x > 0.0f))){
				var dir = getRelativeDir(obj.transform.position);
		        if ((dir > -90.0f)&&(dir < 90.0f)){
		        	float sqrDis = (transform.position - obj.transform.position).sqrMagnitude;
		        	if ((sqrDis > sqrMinDist)&&(sqrDis < sqrMaxDist)&&(checkCloseEnemy(obj.transform.position)> 25.0f)){
						tgtObj = obj;
						//Debug.Log("AAA:" + obj.transform.position + "/" + transform.position);
					}
				}
			}
		}
		return tgtObj;
	}
		
	public IEnumerator stepStop () {

		//if (agent.hasPath) {
	    //    GetComponent<Rigidbody>().velocity = Vector3.zero;
		//	GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		//	agent.isStopped = true;
	    //    GetComponent<Rigidbody>().velocity = Vector3.zero;
		//	GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		//	yield return new WaitForSeconds(0.5f);
		//	agent.isStopped = false;
		//}
		yield return new WaitForSeconds(0.5f);
	}
	
	float checkCloseEnemy(Vector3 own){
        float minDis = 99999.0f;
        for(int i=0 ; i < AiTeam.enemys.Length ; i++){
			float dis = Vector3.Distance( own, AiTeam.enemys[i].transform.position);
			if( dis < minDis  ) {
				var dir = getRelativeDir(AiTeam.enemys[i].transform.position);
		        if ((dir > -90.0f)&&(dir < 90.0f)){
					minDis = dis;
				}
			}
        }
		
		return minDis;
	}
	
	public IEnumerator doGKkick(float delay){
		yield return new WaitForSeconds(delay);
		if (isBallCatch){
		    isBallCatch = false;
		    GameManager.doGKCatch(false);
		    ball.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		    ball.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		    ball.gameObject.GetComponent<BallCtl>().ShootByTime(transform.position + transform.forward * 1.0f, 0.0f);
		    forceKick = true;
	    }
	}
	
	float getRelativeDir(Vector3 target){
    	var diff = target - transform.position;
    	var axis = Vector3.Cross( transform.forward, diff );
        var angle = Vector3.Angle( transform.forward, diff ) * (axis.y < 0 ? -1 : 1) ;
		return angle;
	}
		
	public void jump(){
		;
	}
		
}
