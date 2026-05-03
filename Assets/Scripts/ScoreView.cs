using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class ScoreView : MonoBehaviour
{

	//　スコア表示用テキスト
	private static Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        scoreText = GetComponentInChildren<Text> ();
    }
    
    public static void changeScore(){
    	scoreText.text = GameManager.score[0] + " - " + GameManager.score[1];
    }
    
}
