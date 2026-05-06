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
                    setTargetPos( new Vector3(transform.position.x, 0.1f, transform.position.z) + DisDrag(transform.position, ballrigid.linearVelocity, 0.25f, 9.80665f, 0.1f, 0.05f));
                    disd_flag = false;
                }
            } else if (ballrigid.linearVelocity.magnitude> 5.0f) {
                //0.5秒後の位置を計算
                disd_flag = true;
                setTargetPos( new Vector3(transform.position.x + ballrigid.linearVelocity.x * 0.5f, 0.1f, transform.position.z + ballrigid.linearVelocity.z * 0.5f));
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
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
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
    
    /// <summary>
    /// ボールを指定位置へ、指定した滞空時間と精度で蹴る
    /// </summary>
    /// <param name="targetPos">狙うターゲットの座標</param>
    /// <param name="time">滞空時間（0以下の場合は距離に応じて自動計算）</param>
    /// <param name="accuracy">キック精度（1.0fで完璧に狙い通り、0.0fに近づくほどブレる）</param>
    public void KickToTarget(Vector3 targetPos, float time, float accuracy = 1.0f)
    {
        Vector2 startPos2D  = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPos2D = new Vector2(targetPos.x, targetPos.z);
        float distance      = Vector2.Distance(targetPos2D, startPos2D);

        // 1. timeの自動補正（入力が0.0f以下の場合）
        float t = time;
        if (t <= 0.0f)
        {
            if (distance < 10.0f)      t = 0.5f;   // ショートパス
            else if (distance < 25.0f) t = 1.2f;   // ミドルパス
            else                       t = 2.0f;   // ロングパス・クリア
        }

        // 2. キック精度の計算とターゲット座標のブレ適用
        // 精度は 0.0f ～ 1.0f の間に制限
        accuracy = Mathf.Clamp01(accuracy);
        
        // 1.0f（完璧な精度）未満の時だけブレを発生させる
        if (accuracy < 1.0f)
        {
            // 本来予定していた水平スピード
            float intendedSpeed = distance / t;
            
            // ブレ幅の半径を計算（精度が低い、距離が遠い、スピードが速いほど大きくなる）
            // ※ 0.1f と 0.2f の係数は、ゲームのスケールに合わせて適宜微調整してください
            float errorRadius = (1.0f - accuracy) * (distance * 0.1f + intendedSpeed * 0.2f);
            
            // ターゲット座標をランダムな円内にズラす
            Vector2 randomOffset = Random.insideUnitCircle * errorRadius;
            targetPos.x += randomOffset.x;
            targetPos.z += randomOffset.y;
            
            // ターゲットがズレたので、物理計算に使う2D座標と距離を再計算
            targetPos2D = new Vector2(targetPos.x, targetPos.z);
            distance = Vector2.Distance(targetPos2D, startPos2D);
        }

        // 3. 物理的な限界の補正（低弾道すぎる場合の引き上げ）
        float maxHorizontalSpeed = 30.0f;
        if (distance / t > maxHorizontalSpeed)
        {
            t = distance / maxHorizontalSpeed; 
        }

        // 4. 異常な滞空時間の切り下げ（最大4秒に制限して宇宙開発を防ぐ）
        t = Mathf.Clamp(t, 0.1f, 4.0f);

        // 5. 初速（Velocity）の計算
        float v_xz = distance / t;
        float y0 = transform.position.y;
        float y1 = targetPos.y;
        float g  = Mathf.Abs(Physics.gravity.y);
        
        // y = v0*t - 1/2*g*t^2 から y方向の初速 v0 を逆算
        float v_y = ((y1 - y0) + 0.5f * g * t * t) / t;

        // 6. ベクトルの合成と力の適用
        Vector3 dir = (new Vector3(targetPos.x, 0.0f, targetPos.z) - new Vector3(transform.position.x, 0.0f, transform.position.z)).normalized;
        Vector3 initialVelocity = dir * v_xz;
        initialVelocity.y = v_y;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        Vector3 force = initialVelocity * rb.mass;
        rb.AddForce(force, ForceMode.Impulse);

        // ターゲットマーカーの更新（実際に飛んでいくブレた後の位置を示す）
        if (kickTarget != null) kickTarget.transform.position = new Vector3(targetPos.x, 0.2f, targetPos.z);
    }

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
