using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAt : MonoBehaviour {
    public Transform neckBone;
    public GameObject watchTarget;
    public float _plusRotationY;//Inspector上で-90
    public float _plusRotationZ;//Inspector上で-90
    public float _maxAngle = 60;
    public float _smoothingFactor = 1;

    private Quaternion previousRotation;
    
    protected virtual void LateUpdate()
    {
        if (watchTarget != null)
        {
            // 制限なしの回転を求め...
            var rotation = Quaternion.LookRotation(watchTarget.transform.position - neckBone.position);

            // その回転角を_maxAngleまでに制限し...
            rotation = Quaternion.RotateTowards(transform.rotation, rotation, _maxAngle);

            // 軸合わせを行い...
            rotation *= Quaternion.Euler(0, _plusRotationY, _plusRotationZ);

            // Slerpで前回の回転とブレンドし、previousRotationにセットし...
            previousRotation = Quaternion.Slerp(previousRotation, rotation, Time.deltaTime * _smoothingFactor);

            // その回転を首のrotationとする
            neckBone.transform.rotation = previousRotation;

            // 首の回転がアニメーションにより制御されている場合、それによって回転が毎回更新されてしまうはずなので
            // 下記のようにやったのでは、なめらかに回ってくれないかもしれません
            //neckBone.transform.rotation = Quaternion.Lerp(neckBone.transform.rotation, rotation, Time.deltaTime);
        }
    }
    
}
