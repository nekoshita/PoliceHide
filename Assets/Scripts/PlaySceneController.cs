﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlaySceneController : MonoBehaviour {
	bool pause = true;
	public GameObject gameover_image;
	public GameObject joy_stick;
	public GameObject pause_image;
	public Text score_text;
	public Text finish_score_text;
	[SerializeField] int score = 0;
	int point = 0;
	// Use this for initialization
	void Start () {
		Time.timeScale = 1;
		//SceneManager.LoadScene("Stage", LoadSceneMode.Additive);
		point = PlayerPrefs.GetInt("point", 0);
	}
	
	// Update is called once per frame
	void Update () {
		score_text.text = "Score : " + score.ToString();
	}

	public void PauseButton(){
		if (pause) {
			pause = false;
			pause_image.SetActive (true);
			joy_stick.SetActive (false);
			Time.timeScale = 0;
		} else {
			Time.timeScale = 1;
			pause = true;
			pause_image.SetActive (false);
			joy_stick.SetActive (true);
		}
	}

	public void GameOver(){
		gameover_image.SetActive (true);
		finish_score_text.text = "Score : " + score;
		PlayerPrefs.SetInt("point", point + score);
		if (DataManager.instance.Score < score) {
			finish_score_text.text = "Score : " + score;
			DataManager.instance.Score = score;
		}
	}
	public void Retry(){	
		SceneManager.LoadScene ("Play");
	}
	public void Home(){
		Time.timeScale = 1;
		SceneManager.LoadScene ("Title");
	}
	public void Animation(){
		Time.timeScale = 1;
		SceneManager.LoadScene ("AniamtionSelect");
	}
	public void CheckScore(int id,int count){
		switch (id) {
		case 1:
			score += 100 * count;
			break;
		case 2:
			score += 1000 * count;
			break;
		case 3:
			score += 10000 * count;
			break;
		case 4:
			score += 100000 * count;
			break;
		case 5:
			score += 500000 * count;
			break;
		}
	}
}
