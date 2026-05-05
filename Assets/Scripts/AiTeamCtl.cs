using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiTeamCtl : MonoBehaviour
{
	[Header("自選手のタグ名")]
	public string tgt_tag; //ターゲットマーカーのタグ
	[Header("敵選手のタグ名")]
	public string enm_tag; //ターゲットマーカーのタグ
	[Header("各選手のベースポジション")]
	public Vector3[] basePos;
	[Header("攻撃ポジション")]
	public Vector3[] atkPos;
	[Header("各選手の移動先")]
	public Vector3[] tgtPos;
	[Header("ボール")]
	public GameObject ball;
	[Header("自選手")]
	public GameObject[] players;
	[Header("敵選手")]
	public GameObject[] enemys;
	public GameObject obj;
	public GameObject[] unit;
	public int unitNum;
	public GameObject mk;
	[Header("目標地点")]
	public Vector3 goalPos;
	
	bool prePosFlg = false;
	[Header("交代残り数")]
	int changeCount = 5;
	[Header("一度の交代数")]
	int changeOneCount = 2;
	
	
	//背番号遠し番号
	private int pNum =1;
	
	[Header("チーム番号 (0: - -> + / 1: + -> -)")]
	[SerializeField]
    public int teamId;
	
	void Awake(){
		unitNum = Random.Range(0, unit.Length);
	
        tgtPos = new Vector3[11];
        players = new GameObject[11];
        
        for(int i=0 ; i < basePos.Length ; i++){
        	players[i] = Instantiate(obj, Tpos2Wpos(basePos[i]), Quaternion.identity);
        	players[i].SetActive (true);
        	players[i].transform.SetParent(transform);
        	if (Wpos2Tpos(players[i].transform.position).x>0)players[i].transform.position = new Vector3(0.0f,0.1f,players[i].transform.position.z);//相手陣地には行けない
        	players[i].GetComponent<AIMove>().SetInit(this, i, pNum++, tgt_tag, unit[unitNum], 1000.0f, Random.Range(12.5f, 20.0f));
        	GameObject tgtMk = Instantiate(mk, players[i].transform.position, Quaternion.identity); //デバッグ用
        	tgtMk.transform.SetParent(transform);
        	tgtMk.GetComponent<AiTgtMk>().SetInit(this, i);
        }
        
	}
	
    // Start is called before the first frame update
    void Start()
    {
        updateEnemyData();
    }

    // Update is called once per frame
    void Update()
    {
        float minSqrDis = 9999999.0f;
        int nearestPlayer = 0;
        for(int i=0 ; i < players.Length ; i++){
			float sqrDis = (ball.GetComponent<BallCtl>().targetPos - players[i].transform.position).sqrMagnitude;
			if( sqrDis < minSqrDis ) {
				minSqrDis = sqrDis;
				nearestPlayer = i;
			}
			if (i == 0){ //GKポジション
				if (GameManager.isGKCatch()){
					if (prePosFlg == true){
						tgtPos[i] = Tpos2Wpos(basePos[i] - new Vector3(0.0f,0.0f,0.0f));
						//Debug.Log("GKDist:" + Vector3.Distance( Tpos2Wpos(basePos[i] - new Vector3(0.0f,0.0f,0.0f)), players[i].transform.position));
						if ((Tpos2Wpos(basePos[i] - new Vector3(0.0f,0.0f,0.0f)) - players[i].transform.position).sqrMagnitude < 4.0f) prePosFlg = false;
					} else {
						tgtPos[i] = Tpos2Wpos(basePos[i] + new Vector3(2.0f,0.0f,0.0f));
					}
				} else {
					tgtPos[i] = Tpos2Wpos(basePos[i]) + new Vector3(ball.GetComponent<BallCtl>().targetPos.x/10f,0.0f,ball.GetComponent<BallCtl>().targetPos.z/10f);
					prePosFlg = true;
				}
				
			} else {
				tgtPos[i] = Tpos2Wpos(basePos[i] + (GameManager.isBallTeamId() == teamId? atkPos[i]: Vector3.zero)) + new Vector3(ball.GetComponent<BallCtl>().targetPos.x/2.5f,0.0f,ball.GetComponent<BallCtl>().targetPos.z/2.5f);
				if ((!GameManager.isInplay())&&((ball.GetComponent<BallCtl>().targetPos - tgtPos[i]).sqrMagnitude < 100.0f)){
					Vector3 vec = ( ball.GetComponent<BallCtl>().targetPos - tgtPos[i] ).normalized;
					tgtPos[i] = new Vector3(ball.GetComponent<BallCtl>().targetPos.x - vec.x*7.0f,0.0f,ball.GetComponent<BallCtl>().targetPos.z - vec.z * 7.0f);
				}
			}
        	if ((GameManager.getPstat()==playStat.GoalP)||(GameManager.getPstat()==playStat.GoalM)){
        		
        		if (Wpos2Tpos(tgtPos[i]).x>0)tgtPos[i] = new Vector3(0.0f,0.1f,tgtPos[i].z);//相手陣地には行けない
        	}
        }
        if ((!GameManager.isGKCatch())&&((GameManager.isInplay())||(GameManager.isBallTeamId() == teamId))){
        	if ((nearestPlayer > 0)||(Tpos2Wpos(ball.GetComponent<BallCtl>().targetPos).x < -30.0f)||(ball.transform.position.y<2.0f)){ //GKの場合はゴールエリアより手前
        		tgtPos[nearestPlayer] = ball.GetComponent<BallCtl>().targetPos;
        	}
        }
        
        if ((GameManager.isInplay())&&(Tpos2Wpos(ball.GetComponent<BallCtl>().targetPos).x < -GameManager.FieldXlen/2)&&(ball.GetComponent<BallCtl>().targetPos.x != ball.transform.position.x)){
        	//z = (z2 − z1)(X - x1)/(x2−x1) + z1  (x1!=x2) 
        
        	var calc_z = (ball.GetComponent<BallCtl>().targetPos.z - ball.transform.position.z)*(players[0].transform.position.x - ball.transform.position.x) / (ball.GetComponent<BallCtl>().targetPos.x - ball.transform.position.x ) + ball.transform.position.z;
        	tgtPos[0] = new Vector3(players[0].transform.position.x, 0.1f, calc_z);
        	
        }

    }
    
    // 最小閾値以下の選手を"一度の交代数"分交代
    public void chkChgPlayer(){
        List<int> pNum = new List<int>();  // 交代対象のリスト
        int pCount = 0;  // 交代対象の数
        if ((!GameManager.isInplay())&&(changeCount>0)){
	        for(int i=0 ; i < players.Length ; i++){
	        	if (players[i].GetComponent<AIMove>().staminaMax < 500.0f) {
	        		// 交代対象リストに追加(昇順)
	        		int j;
	        		for ( j = 0 ; j < pCount ; j++ ){
		        	    if (players[i].GetComponent<AIMove>().staminaMax < players[j].GetComponent<AIMove>().staminaMax) break;
		        	}
		        	pNum.Insert(j, i);
		        	pCount++;
	        	}
	        }
	        // "交代可能数分"または"一度の交代数"分交代
	        for(int i=0 ; i < changeOneCount && changeCount > 0 && i < pCount ; i++){
	        	// 交代処理
		        chgPlayer(pNum[i]);
		        changeCount--;
	        }
        }
    }
    
    public void chgPlayer(int pnum){
    	Debug.Log("CHG Player:" + pnum);
        players[pnum].gameObject.tag = "Untagged";
    	players[pnum].GetComponent<AIMove>().SetDestroy();
        players[pnum] = Instantiate(obj, Tpos2Wpos(new Vector3(-1.0f - changeCount, 0.0f, 0.0f)) + new Vector3(0.0f , 0.0f, -GameManager.FieldZlen/2), Quaternion.identity);
        players[pnum].SetActive (true);
        players[pnum].transform.SetParent(transform);
        players[pnum].GetComponent<AIMove>().SetInit(this, pnum, pNum++, tgt_tag, unit[unitNum], 1000.0f, Random.Range(12.5f, 20.0f));
        GameObject tgtMk = Instantiate(mk, new Vector3( 0.0f, 0.0f, 0.0f), Quaternion.identity); //デバッグ用
        tgtMk.transform.SetParent(transform);
        tgtMk.GetComponent<AiTgtMk>().SetInit(this, pnum);
        GameManager.chgTeamPlayer();
    }
   
    public int isTeamId(){
    	return teamId;
    }
    
    public void updateEnemyData(){
    	enemys  = GameObject.FindGameObjectsWithTag(enm_tag);
    }
    
    public Vector3 getWgpos(){
    	return Tpos2Wpos(goalPos);
    }
    
    public Vector3  Tpos2Wpos(Vector3 pos){
    	if (teamId == 0){
    		return pos; //そのまま返す
    	} else {
    		return new Vector3(-pos.x,pos.y,-pos.z); //XZ反転
    	}
    }
    
    public Vector3  Wpos2Tpos(Vector3 pos){
    	if (teamId == 0){
    		return pos; //そのまま返す
    	} else {
    		return new Vector3(-pos.x,pos.y,-pos.z); //XZ反転
    	}
    }
    
}
