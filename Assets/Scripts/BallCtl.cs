using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCtl : MonoBehaviour
{
	private bool jmpFlag = false;
	private bool disd_flag = false;
	[Header("落下予測地点(高さY=0.1 固定)")]
	public Vector3 targetPos;
	private Rigidbody ballrigid;
	public Vector3? viewPos;
	public GameObject kickTarget;

    private GameObject lastTouchObj;

	void Awake(){
    	FollowCamera.addCameraTarget(this.gameObject);
        ballrigid = GetComponent<Rigidbody>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
		//インプレー時
    	if (GameManager.isInplay()){
    	
	        // キックで一定高度を超えた
	        if (transform.position.y > 1.0f){
	            if (disd_flag) {
	            	//落下予測地点を計算
	            	setTargetPos( new Vector3(transform.position.x, 0.1f, transform.position.z) + DisDrag(transform.position, ballrigid.velocity, 0.25f, 9.80665f, 0.1f, 0.05f));
	            	disd_flag = false;
	            }
	        } else if (ballrigid.velocity.magnitude> 5.0f) {
	        	//0.5秒後の位置を計算
	            disd_flag = true;
	            setTargetPos( new Vector3(transform.position.x + ballrigid.velocity.x * 0.5f, 0.1f, transform.position.z + ballrigid.velocity.z * 0.5f));
	        } else {
	        	//現在の位置
	            disd_flag = true;
	            setTargetPos( new Vector3(transform.position.x, 0.1f, transform.position.z));
	        }
    		if (!GameManager.isKick()){
	    		// ゴールラインを超えた
		        if (transform.position.x<-GameManager.FieldXlen/2){
					if (!jmpFlag){
						if ((transform.position.y<2.0f)&&(transform.position.z>-2.0f)&&(transform.position.z<2.0f)){
							//GameManager.score[1]++;
							//ScoreView.changeScore();
							GameManager.ballOut(this, playStat.GoalM, transform.position);
						} else {
							GameManager.ballOut(this, playStat.GoalLineM, transform.position);
				        	
				        	//jmpFlag = true;
			        	}
		        	}
		        }
		        if (transform.position.x>GameManager.FieldXlen/2){
					if (!jmpFlag){
						if ((transform.position.y<2.0f)&&(transform.position.z>-2.0f)&&(transform.position.z<2.0f)){

							//GameManager.score[0]++;
							//ScoreView.changeScore();
							GameManager.ballOut(this, playStat.GoalP, transform.position);
						} else {
							GameManager.ballOut(this, playStat.GoalLineP, transform.position);
				        	
				        	//jmpFlag = true;
			        	}
		        	}
		        }
		        
		        
		        // サイドラインを超えた
		        if (transform.position.z<-GameManager.FieldZlen/2){
					if (!jmpFlag){
						GameManager.ballOut(this, playStat.SideLineM, transform.position);
			        	//Invoke("jmpPos", 2.0f);
			        	//jmpFlag = true;
		        	}
		        }
		        if (transform.position.z>GameManager.FieldZlen/2){
					if (!jmpFlag){
						GameManager.ballOut(this, playStat.SideLineP, transform.position);
			        	//Invoke("jmpPos", 2.0f);
			        	//jmpFlag = true;
		        	}
		        }
	        }
	        
	        // 落下(ありえない)
	        if (transform.position.y<-5.0f){
				if (!jmpFlag){
		        	Invoke("jmpPos", 2.0f);
		        	jmpFlag = true;
	        	}
	        }
	        
        }

    }
    
    //ランダムな地点に飛ばす(デバッグ用)
    void jmpPos()
    {
	    jmpFlag = false;
	    GetComponent<Rigidbody>().velocity = Vector3.zero;
	    GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    	transform.position = new Vector3(Random.Range(-GameManager.FieldXlen/2, GameManager.FieldXlen/2),10.0f,Random.Range(-GameManager.FieldZlen/2, GameManager.FieldZlen/2));
    	Debug.Log("BALL JUMP:" + transform.position);
    }
    
    //ターゲットマーカーの位置設定
    public void setTargetPos( Vector3 pos){
    	targetPos = pos;
    }
    
    //空中に飛んだ場合の落下予測地点を計算
    Vector3 DisDrag(Vector3 pos, Vector3 vel, float drag, float gravity, float mass, float step)
    {
    	float time = 0;
        float r =  Mathf.Clamp01(1 - drag * Time.fixedDeltaTime);
        float n = 0.0f;
        while (pos.y >= 0.7f)
        {
            //Debug.Log ("[CAL]pos" + pos + ",vel" + vel);
            n = time / Time.fixedDeltaTime;
            pos.y = vel.y * Time.fixedDeltaTime * r * (1 - Mathf.Pow(r, n)) / (1 - r) + 1.0f;
            pos.y = pos.y - r * gravity * Time.fixedDeltaTime * Time.fixedDeltaTime / (1 - r) * (n - r * (1 - Mathf.Pow(r, n)) / (1 - r));
            time += step;
        }
        pos.x = vel.x * Time.fixedDeltaTime * r * (1 - Mathf.Pow(r, n)) / (1 - r);
        pos.z = vel.z * Time.fixedDeltaTime * r * (1 - Mathf.Pow(r, n)) / (1 - r);
        //Debug.Log ("[END]pos" + pos + ",vel" + vel);
        return new Vector3(pos.x, 0.0f, pos.z);
    }
    
    //ボールを指定位置へ飛ばす
	public void ShootByTime( Vector3 pos , float time )
	{
	    Vector2 startPos    = new Vector2( transform.position.x, transform.position.z );
	    Vector2 targetPos   = new Vector2( pos.x, pos.z );
	    float distance      = Vector2.Distance( targetPos, startPos );
	    
	    if (distance < 10.0f){
		    // とりあえず適当に60度でかっ飛ばすとするよ！
		    ShootFixedAngle( pos, 15.0f );
	    } else if (distance < 25.0f){
	    	ShootFixedAngle( pos, 30.0f );
	    } else {
	    	ShootFixedAngle( pos, 45.0f );
	    };
	    
	    if (kickTarget != null) kickTarget.transform.position = new Vector3( pos.x, 0.2f, pos.z);
	    
	}

	private void ShootFixedAngle( Vector3 i_targetPosition, float i_angle )
	{
	    float speedVec  = ComputeVectorFromAngle( i_targetPosition, i_angle );
	    if( speedVec <= 0.0f )
	    {
	        // その位置に着地させることは不可能のようだ！
	        Debug.LogWarning( "!!" );
	        return;
	    }

	    Vector3     vec = ConvertVectorToVector3( speedVec, i_angle, i_targetPosition );
	    
	    // 速さベクトルのままAddForce()を渡してはいけないぞ。力(速さ×重さ)に変換するんだ
	    GetComponent<Rigidbody>().velocity = Vector3.zero;
	    Vector3 force   = vec * GetComponent<Rigidbody>().mass;
	    GetComponent<Rigidbody>().AddForce( force, ForceMode.Impulse );
	}

	private float ComputeVectorFromAngle( Vector3 i_targetPosition, float i_angle )
	{
	    // xz平面の距離を計算。
	    Vector2 startPos    = new Vector2( transform.position.x, transform.position.z );
	    Vector2 targetPos   = new Vector2( i_targetPosition.x, i_targetPosition.z );
	    float distance      = Vector2.Distance( targetPos, startPos );

	    float x     = distance;
	    float g     = Physics.gravity.y;
	    float y0    = transform.position.y;
	    float y     = i_targetPosition.y;

	    // Mathf.Cos()、Mathf.Tan()に渡す値の単位はラジアンだ。角度のまま渡してはいけないぞ！
	    float rad   = i_angle * Mathf.Deg2Rad;

	    float cos   = Mathf.Cos( rad );
	    float tan   = Mathf.Tan( rad );

	    float v0Square  = g * x * x / ( 2 * cos * cos * ( y - y0 - x * tan ) );
	    
	    // 負数を平方根計算すると虚数になってしまう。
	    // 虚数はfloatでは表現できない。
	    // こういう場合はこれ以上の計算は打ち切ろう。
	    if( v0Square <= 0.0f )
	    {
	        return 0.0f;
	    }

	    float v0    = Mathf.Sqrt( v0Square );
	    return v0;
	}

	private Vector3 ConvertVectorToVector3( float i_v0, float i_angle, Vector3 i_targetPosition )
	{
	    Vector3     startPos    = transform.position;
	    Vector3     targetPos   = i_targetPosition;
	    startPos.y  = 0.0f;
	    targetPos.y = 0.0f;

	    Vector3     dir     = ( targetPos - startPos ).normalized;
	    Quaternion yawRot   = Quaternion.FromToRotation( Vector3.right, dir );
	    Vector3     vec     = i_v0 * Vector3.right;
	    
	    vec     = yawRot * Quaternion.AngleAxis( i_angle, Vector3.forward ) * vec;

	    return vec;
	}

	//private void InstantiateShootObject( Vector3 i_shootVector )
	//{
	//    if( m_shootObject == null )
	//    {
	//        throw new System.NullReferenceException( "m_shootObject" );
	//    }
    //
	//    //if( m_shootPoint == null )
	//    //{
	//    //    throw new System.NullReferenceException( "m_shootPoint" );
	//    //}
    //
	//    var obj         = Instantiate<GameObject>( m_shootObject, m_shootPoint.position, Quaternion.identity );
	//    var rigidbody   = obj.AddComponent<Rigidbody>();
    //
	//    // 速さベクトルのままAddForce()を渡してはいけないぞ。力(速さ×重さ)に変換するんだ
	//    Vector3 force   = i_shootVector * rigidbody.mass;
    //
	//    rigidbody.AddForce( force, ForceMode.Impulse );
	//}
	
	public void setLastTouch(GameObject obj){
		lastTouchObj = obj;
	}
	
	public GameObject getLastTouch(){
		return lastTouchObj;
	}
	
	// 表示ポジション設定(Nullでボールを表示先に設定)
	public void setViewPos(Vector3? pos){
		viewPos = pos;
	}
	
	// 表示ポジション取得
	public Vector3 getViewPos(){
		if (viewPos == null) return transform.position;
		return (Vector3)viewPos;
	}
	
}
