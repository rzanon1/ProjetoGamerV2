using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {

	// Pontuacao do jogador
	int score;       
	
	// Referencia o componente de Text
	public Text number;

	void Awake() {
		// Reseta o score.
		score = 0;
	}

	void Update() {
		// Ajusta o texto do score mostrado, no formato "Score: Valor"
		number.text = score.ToString();
	}

	public void AddScore(int toAdd) {
		score += toAdd;
		number.GetComponent<Animation>().Stop();
		number.GetComponent<Animation>().Play();
	}

	public int GetScore() {
		return score;
	}
}