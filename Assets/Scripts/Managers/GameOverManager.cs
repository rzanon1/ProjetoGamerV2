using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
	public PlayerHealth playerHealth; // Referência à saúde do jogador
	public float restartDelay = 5f;   // Tempo antes de reiniciar o jogo

	Animator anim;                    // Referência ao componente Animator
	float restartTimer;               // Temporizador para reiniciar o jogo
	ScoreManager scoreManager;        // Referência ao ScoreManager

	void Awake()
	{
		// Configuração do Animator
		anim = GameObject.Find("HUDCanvas").GetComponent<Animator>();

		// Encontrar o ScoreManager na cena
		scoreManager = FindObjectOfType<ScoreManager>();
	}

	void Update()
	{
		// Se a saúde do jogador acabar...
		if (playerHealth != null && playerHealth.currentHealth <= 0)
		{
			// Salvar o high score
			SaveHighScore();

			// Disparar animação de Game Over
			anim.SetTrigger("GameOver");

			// Incrementar o temporizador
			restartTimer += Time.deltaTime;

			// Reiniciar o nível se o tempo do temporizador atingir o atraso
			if (restartTimer >= restartDelay)
			{
				SceneManager.LoadScene("Level 01");
			}
		}
	}

	private void SaveHighScore()
	{
		// Obter a pontuação do ScoreManager
		int playerScore = scoreManager != null ? scoreManager.GetScore() : 0;
		string playerName = "Player"; // Nome fixo ou dinâmico se necessário

		// Encontrar o HighScoreManager
		HighScoreManager highScoreManager = FindObjectOfType<HighScoreManager>();

		if (highScoreManager != null)
		{
			// Adicionar a nova pontuação
			highScoreManager.AddNewScore(playerName, playerScore);
			Debug.Log("HighScore atualizado: " + playerName + " - " + playerScore);
		}
		else
		{
			Debug.LogWarning("HighScoreManager não encontrado na cena!");
		}
	}
}
