using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pMove : MonoBehaviour
{

    [Header("移動")]
    [TooltipAttribute("歩く速度"), SerializeField]
    float walkSpeed = 2f;
    [TooltipAttribute("歩く加速度"), SerializeField]
    float walkAccel = 2f;
    [TooltipAttribute("通常の旋回速度"), SerializeField]
    float angularSpeed = 200f;
    [TooltipAttribute("ターンする時の角度差"), SerializeField]
    float turnAngle = 45f;
    [TooltipAttribute("ターン時の旋回速度"), SerializeField]
    float turnAngularSpeed = 1000f;
    [TooltipAttribute("スピードを落とす距離。目的地がこの距離以内になったら、旋回角度に応じた減速をする"), SerializeField]
    float speedDownDistance = 0.5f;
    [TooltipAttribute("停止距離。この距離以下は移動しない"), SerializeField]
    float stopDistance = 0.01f;

    [Header("アニメーション")]
    [TooltipAttribute("移動速度とアニメーション速度の変換率"), SerializeField]
    float Speed2Anim = 1f;
    [TooltipAttribute("アニメを停止とみなす速度"), SerializeField]
    float stopSpeed = 0.01f;
    [TooltipAttribute("アニメの平均化係数"), SerializeField]
    float averageSpeed = 0.5f;

    UnityEngine.AI.NavMeshAgent agent;
    Animator anim;
    //CharacterController chrController;
    [TooltipAttribute("目的地"), SerializeField]
    Vector3 destination;
    [TooltipAttribute("停止時目的方向"), SerializeField]
    Vector3 destView;
    Rigidbody rb;
    float spd;
    /// <summary>
    /// アニメ速度を少し慣らすための値
    /// </summary>
    float lastSpeed;

    public bool IsReached
    {
        get
        {
            return DistanceXZ(destination, transform.position) <= stopDistance;
        }
    }
    
    /// <summary>
    /// XZのみの距離を返します。
    /// </summary>
    /// <param name="src">元座標</param>
    /// <param name="dst">先座標</param>
    /// <returns>高さの差を考慮しない距離</returns>
    public float DistanceXZ(Vector3 src, Vector3 dst)
    {
        src.y = dst.y;
        return Vector3.Distance(src, dst);
    }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        //chrController = GetComponent<CharacterController>();
        rb = this.GetComponent<Rigidbody> ();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity= new Vector3(0.0f, 2.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move;
        //float spd = walkSpeed * Time.deltaTime;
        //float spd;// = rb.velocity.magnitude;
        
        // 移動方向と速度を算出
        move = destination - transform.position;
        float rot = angularSpeed * Time.deltaTime;

        //　移動距離が目的地までの距離より遠い場合、角度と移動設定
        if (!IsReached)
        {
            float angle = Vector3.SignedAngle(transform.forward, move, Vector3.up);

            // 角度がturnAngleを越えていたら速度0
            if (Mathf.Abs(angle) > turnAngle)
            {
                // 最高速度を越えているのでターンのみ
                float rotmax = turnAngularSpeed * Time.deltaTime;
                rot = Mathf.Min(Mathf.Abs(angle), rotmax);
                transform.Rotate(0f, rot * Mathf.Sign(angle), 0f);
                move = Vector3.zero;
                spd -= walkAccel;
                if (spd < 1.0f) spd = 1.0f;
            }
            else
            {
                // ターンはしない
                spd += walkAccel;// * Time.deltaTime;
                if (spd > walkSpeed /* * Time.deltaTime */) spd = walkSpeed /* * Time.deltaTime */;

                // ゴール距離がスピードダウンより近い場合、角度の違いの分、前進速度を比例減速する
                if (DistanceXZ(destination, transform.position) < speedDownDistance)
                {
                    spd *= (1f - (Mathf.Abs(angle) / turnAngle));
                }

                // 1回分の移動をキャンセルする場合、回転速度は制限しない
                if (move.magnitude < spd)
                {
                    //spd = move.magnitude;
                    rot = angle;
                    transform.Rotate(0f, angle, 0f);
                }
                else
                {
                    // 移動しながらターン
                    rot = Mathf.Min(Mathf.Abs(angle), rot);
                    transform.Rotate(0f, rot * Mathf.Sign(angle), 0f);
                }

                // キャラクターの前方に移動
                move = transform.forward * spd;
            }
        }
        else
        {
            spd -= walkAccel /* * Time.deltaTime */;
            if (spd < 0f) spd = 0f;
            move = destination - transform.position;
            
            Vector3 view = destView - transform.position;
            float angle = Vector3.SignedAngle(transform.forward, view, Vector3.up);
            // 最高速度を越えているのでターンのみ
            if (Mathf.Abs(angle) > turnAngularSpeed * Time.deltaTime) {
                float rotmax = turnAngularSpeed * Time.deltaTime;
                rot = Mathf.Min(Mathf.Abs(angle), rotmax);
                transform.Rotate(0f, rot * Mathf.Sign(angle), 0f);
            }
            //move = Vector3.zero;
        }

            //chrController.Move(move);
            //Vector3 force = new Vector3 (500.0f,0.0f,1.0f);    // 力を設定
            //rb.AddForce (force);  // 力を加える
            //rb.AddForce(transform.forward, ForceMode.Impulse);
            //move = transform.forward * spd;
            move.y = rb.velocity.y;
            rb.velocity = move;//(transform.forward * 100.0f) * Time.fixedDeltaTime;
            //spd = spd / Time.deltaTime;
    }
    
    public void setDist(Vector3 dest){
    	destination = dest;
    }
    
    public void setView(Vector3 view){
    	destView = view;
    }
    
    public void setSpeed(float speed){
    	walkSpeed = speed;
    }
    
}
