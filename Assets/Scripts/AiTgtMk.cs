using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiTgtMk : MonoBehaviour
{
	[Header("AIチーム用通し番号(0オリジン)")]
	public int AInum;
	public AiTeamCtl AiTeam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    	if (AiTeam != null){
        	transform.position = AiTeam.tgtPos[AInum];
        }
    }
    
	//初期設定
	public void SetInit (AiTeamCtl _AiTeam, int _Ainum)
	{
		AInum = _Ainum;
		AiTeam = _AiTeam;
	}
    
}
