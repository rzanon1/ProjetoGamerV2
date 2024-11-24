using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HighScoreManager : MonoBehaviour
{
	[SerializeField] private Transform scoreContainer; // Content do Scroll View
	[SerializeField] private GameObject scorePrefab;   // Prefab dos itens de score

	private List<ScoreEntry> highScores;

	private void Start()
	{
		// Carregar e exibir os high scores ao iniciar
		LoadHighScores();
		DisplayHighScores();
	}

	private void LoadHighScores()
	{
		// Carregar os scores salvos do PlayerPrefs
		highScores = LoadSavedHighScores();
	}

	private void SaveHighScore(string playerName, int score)
	{
		// Carregar os scores existentes
		List<ScoreEntry> savedScores = LoadSavedHighScores();

		// Verificar se o jogador já está na lista
		bool playerExists = savedScores.Exists(entry => entry.playerName == playerName);

		if (playerExists)
		{
			// Atualizar o score do jogador se o novo score for maior
			foreach (var entry in savedScores)
			{
				if (entry.playerName == playerName && entry.score < score)
				{
					entry.score = score;
					break;
				}
			}
		}
		else
		{
			// Adicionar o novo jogador
			savedScores.Add(new ScoreEntry { playerName = playerName, score = score });
		}

		// Ordenar os scores em ordem decrescente
		savedScores.Sort((a, b) => b.score.CompareTo(a.score));

		// Manter apenas os top 10 scores
		if (savedScores.Count > 10)
			savedScores.RemoveRange(10, savedScores.Count - 10);

		// Salvar no PlayerPrefs
		string json = JsonUtility.ToJson(new ScoreList(savedScores));
		PlayerPrefs.SetString("HighScores", json);
		PlayerPrefs.Save();
	}

	public void AddNewScore(string playerName, int score)
	{
		// Adicionar e salvar o novo score
		SaveHighScore(playerName, score);

		// Atualizar a lista de scores na memória
		LoadHighScores();

		// Atualizar a exibição na tela
		DisplayHighScores();
	}

	private void DisplayHighScores()
	{
		// Limpar scores antigos do container
		foreach (Transform child in scoreContainer)
		{
			Destroy(child.gameObject);
		}

		// Adicionar os novos scores no container
		foreach (ScoreEntry entry in highScores)
		{
			CreateScoreItem(entry.playerName, entry.score);
		}
	}

	private void CreateScoreItem(string playerName, int score)
	{
		// Criar um novo item de score
		GameObject scoreItem = Instantiate(scorePrefab, scoreContainer);
		scoreItem.transform.Find("NameText").GetComponent<Text>().text = playerName;
		scoreItem.transform.Find("ScoreText").GetComponent<Text>().text = score.ToString();
	}

	private List<ScoreEntry> LoadSavedHighScores()
	{
		string json = PlayerPrefs.GetString("HighScores", "");
		if (string.IsNullOrEmpty(json))
			return new List<ScoreEntry>();

		// Desserializar os scores
		ScoreList scoreList = JsonUtility.FromJson<ScoreList>(json);
		return scoreList.scores;
	}

	[System.Serializable]
	public class ScoreEntry
	{
		public string playerName;
		public int score;
	}

	[System.Serializable]
	private class ScoreList
	{
		public List<ScoreEntry> scores;

		public ScoreList(List<ScoreEntry> scores)
		{
			this.scores = scores;
		}
	}
}
