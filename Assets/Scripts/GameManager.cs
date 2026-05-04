using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	//ゲームの状態
    public enum playStat : int
    {
        OnPlay,     //プレー中
        PreStart,   //開始前
        GoalP,      //ゴール(X軸正)
        GoalM,      //ゴール(X軸負)
        GoalLineP,  //ゴールラインを超える(X軸正)
        GoalLineM,  //ゴールラインを超える(X軸負)
        SideLineP,  //サイドラインを超える(Y軸正)
        SideLineM,  //サイドラインを超える(Y軸正)
        FoulP,      //ファール(Y軸正)
        FoulM,      //ファール(Y軸正)
        Others,     //その他
    }

public class GameManager : MonoBehaviour
{
	static playStat pStat = playStat.PreStart;

	[Header("フィールドの長辺横軸(X軸)の長さ")]
	public static float FieldXlen = 80.0f;
	[Header("フィールドの短辺縦軸(Z軸)の長さ")]
	public static float FieldZlen = 40.0f;
	public static int[] score = {0,0};
	
	[Header("ボール所持チーム番号")]
	static int ballTeamId;
	[Header("チーム情報")]
	public AiTeamCtl[] pTeam;
	
	//public static ScoreView scoreview;
	
	[Header("ゲームオン中")]
	static bool isInplayFlg = true;
	[Header("スローイン・キックフラグ")]
	static bool isKickFlg = false;
	[Header("スローイン・キックフラグ")]
	static bool isGKCatchFlg = false;
	
	static GameManager instance;
	
	void Awake()
	{
        instance = this;
	}
	
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //ボールが外に出た
    public static void ballOut(BallCtl ball , playStat outType, Vector3 outPos){
    	isInplayFlg = false;
    	pStat = outType;
    	revBallTeamId();
    	switch (outType){
    	case playStat.GoalP:
	    		score[0]++;
	    		ScoreView.changeScore();
    			setBallTeamId(1);
    			ball.setTargetPos(new Vector3(0.1f, 0.1f, 0.0f));
    			ball.setViewPos(new Vector3(0.0f, 0.1f, 0.0f));
    			instance.StartCoroutine(instance.DelayMethod(ball, 15.0f, new Vector3(0.0f, 0.1f, 0.0f)));
    			//Invoke("cameraJump", 2.0f);
    			instance.StartCoroutine(instance.cameraJump(ball.getLastTouch(), 3.0f));
    		break;
    	case playStat.GoalM:
	    		score[1]++;
	    		ScoreView.changeScore();
    			setBallTeamId(0);
    			ball.setTargetPos(new Vector3(-0.1f, 0.1f, 0.0f));
    			ball.setViewPos(new Vector3(0.0f, 0.1f, 0.0f));
    			instance.StartCoroutine(instance.DelayMethod(ball, 15.0f, new Vector3(0.0f, 0.1f, 0.0f)));
    			//Invoke("cameraJump", 2.0f);
    			instance.StartCoroutine(instance.cameraJump(ball.getLastTouch(), 3.0f));
    		break;
    	case playStat.GoalLineP:
    		if (outPos.z < 0){
    			if (ballTeamId == 0){ //コーナーキック
	    			ball.setTargetPos(new Vector3(41.0f, 0.1f, -22.0f));
	    			ball.setViewPos(new Vector3(39.9f, 0.1f, -19.9f));
	    			instance.StartCoroutine(instance.DelayMethod(ball, 10.0f, new Vector3(39.9f, 0.1f, -19.9f)));
    			} else { //ゴールキック
	    			ball.setTargetPos(new Vector3(39.0f, 0.1f, -5.0f));
	    			ball.setViewPos(new Vector3(36.0f, 0.1f, -5.0f));
	    			instance.StartCoroutine(instance.DelayMethod(ball, 7.0f, new Vector3(36.0f, 0.1f, -5.0f)));
    			}
    		} else {
    			if (ballTeamId == 0){ //コーナーキック
	    			ball.setTargetPos(new Vector3(41.0f, 0.1f, 22.0f));
	    			ball.setViewPos(new Vector3(39.9f, 0.1f, 19.9f));
	    			instance.StartCoroutine(instance.DelayMethod(ball, 10.0f, new Vector3(39.9f, 0.1f, 19.9f)));
    			} else { //ゴールキック
	    			ball.setTargetPos(new Vector3(39.0f, 0.1f, 5.0f));
	    			ball.setViewPos(new Vector3(36.0f, 0.1f, 5.0f));
	    			instance.StartCoroutine(instance.DelayMethod(ball, 7.0f, new Vector3(36.0f, 0.1f, 5.0f)));
    			}
    		}
    		break;
    	case playStat.GoalLineM:
    		if (outPos.z < 0){
    			if (ballTeamId == 0){ //ゴールキック
	    			ball.setTargetPos(new Vector3(-39.0f, 0.1f, -5.0f));
	    			ball.setViewPos(new Vector3(-36.0f, 0.1f, -5.0f));
	    			instance.StartCoroutine(instance.DelayMethod(ball, 7.0f, new Vector3(-36.0f, 0.1f, -5.0f)));
    			} else { //コーナーキック
	    			ball.setTargetPos(new Vector3(-41.0f, 0.1f, -22.0f));
	    			ball.setViewPos(new Vector3(-39.9f, 0.1f, -19.9f));
	    			instance.StartCoroutine(instance.DelayMethod(ball, 10.0f, new Vector3(-39.9f, 0.1f, -19.9f)));
    			}
    		} else {
    			if (ballTeamId == 0){ //ゴールキック
	    			ball.setTargetPos(new Vector3(-39.0f, 0.1f, 5.0f));
	    			ball.setViewPos(new Vector3(-36.0f, 0.1f, 5.0f));
	    			instance.StartCoroutine(instance.DelayMethod(ball, 7.0f, new Vector3(-36.0f, 0.1f, 5.0f)));
    			} else { //コーナーキック
	    			ball.setTargetPos(new Vector3(-41.0f, 0.1f, 22.0f));
	    			ball.setViewPos(new Vector3(-39.9f, 0.1f, 22.0f));
	    			instance.StartCoroutine(instance.DelayMethod(ball, 10.0f, new Vector3(-39.9f, 0.1f, 19.9f)));
    			}
    		}
    		break;
    	case playStat.SideLineP:
    		ball.setTargetPos(new Vector3(outPos.x, 0.1f, 22.0f));
    		ball.setViewPos(new Vector3(outPos.x, 0.1f, 20.0f));
    		instance.StartCoroutine(instance.DelayMethod(ball, 7.0f, new Vector3(outPos.x, 0.1f, 19.9f)));
    		break;
    	case playStat.SideLineM:
    		ball.setTargetPos(new Vector3(outPos.x, 0.1f, -22.0f));
    		ball.setViewPos(new Vector3(outPos.x, 0.1f, -20.0f));
    		instance.StartCoroutine(instance.DelayMethod(ball, 7.0f, new Vector3(outPos.x, 0.1f, -19.9f)));
    		break;
    	default:
    		break;
    	}
        for(int i=0 ; i < instance.pTeam.Length ; i++){
        	instance.pTeam[i].chkChgPlayer();
        }
    }
    
    //delay秒後にカメラターゲット変更
    IEnumerator cameraJump(GameObject target, float delay){
    	yield return new WaitForSeconds(delay);
    	FollowCamera.setTempCameraTarget(target);
    }
    
    public static bool isInplay(){
    	return isInplayFlg;
    }
    
    public static bool isKick(){
    	return isKickFlg;
    }
    
    public static void doKick(){
    	isKickFlg = false;
    	//isGKCatchFlg = false;
    }
    
    public static bool isGKCatch(){
    	return isGKCatchFlg;
    }
    
    public static void doGKCatch(bool flag){
    	isGKCatchFlg = flag;
    }
    
    //ボールをdelay秒後に指定位置に置く
    IEnumerator DelayMethod(BallCtl ball, float delay, Vector3 pos) {
        yield return new WaitForSeconds(delay);
        ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
	    ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    	ball.transform.position = pos;
    	isInplayFlg = true;
		isKickFlg = true;
		pStat = playStat.OnPlay;
		FollowCamera.clearTempCameraTarget();
		ball.setViewPos(null);
	}
	
    public static playStat getPstat(){
    	return pStat;
    }
    
    public static void chgTeamPlayer(){
        for(int i=0 ; i < instance.pTeam.Length ; i++){
        	instance.pTeam[i].updateEnemyData();
        }
    }
    
    public static int isBallTeamId(){
    	return ballTeamId;
    }
    
    public static void setBallTeamId(int _ballTeamId){
    	ballTeamId = _ballTeamId;
    }
    
    static void revBallTeamId(){
    	if (ballTeamId == 0){
    		ballTeamId = 1;
    	} else {
    		ballTeamId = 0;
    	}
    }

}
