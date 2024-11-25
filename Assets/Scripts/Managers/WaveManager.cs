using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour {
	
	public PlayerHealth playerHealth;   
	// A distância do Camera View Frustum em que os inimigos serão gerados,
	// garantindo que não sejam visíveis ao serem gerados.

	public float bufferDistance = 200;
	// Tempo em segundos a cada wave
	public float timeBetweenWaves = 5f;
    // Tempo entre cada spawn em uma wave
    public float spawnTime = 3f;
    // Wave que inicia
	public int startingWave = 1;
    // Dificuldade que inicia
	public int startingDifficulty = 1;
	public Text number;
	public Text numberEnemies; 

	[HideInInspector]
	public int enemiesAlive = 0;

	// Representa uma onda com múltiplas entradas.
	[System.Serializable]
	public class Wave {
		// Lista de entradas (diferentes tipos de inimigos nesta onda).
		public Entry[] entries;

		// Representa uma entrada individual dentro da onda.
		[System.Serializable]
		public class Entry {
			// O tipo de inimigo a ser gerado (prefab).
			public GameObject enemy;
			// Quantidade de inimigos deste tipo a ser gerada.
			public int count;
			// Contador que rastreia quantos já foram gerados (não serializado).
			[System.NonSerialized]
			public int spawned;
		}
	}

	// Todas as ondas configuradas para o jogo.
	public Wave[] waves;

    // Juntando as variaveis para que funcione
    Vector3 spawnPosition = Vector3.zero;
	int waveNumber;
	float timer; 
	Wave currentWave;
	int spawnedThisWave = 0;
	int totalToSpawnForWave;
	bool shouldSpawn = false;
	int difficulty;
	int enemiesInLastFrame;
	float timeSinceNoEnemiesKilled;

	void Start() {
		// Permitir iniciar em uma wave maior e na dificuldade que desejar
		waveNumber = startingWave > 0 ? startingWave - 1 : 0;
		difficulty = startingDifficulty;

		// Inicia a proxima wave (que sera a primeira no inicio do jogo).
		StartCoroutine("StartNextWave");
	}
	
	void Update() {
		// Isso fica em false enquanto arruma a proxima wave
		if (!shouldSpawn) {
			return;
        }

		// Inicia a próxima onda quando:
		// Todos os inimigos foram gerados e eliminados pelo jogador, ou
		// Nenhum inimigo foi morto nos últimos 20 segundos,
		// assumindo que não há mais inimigos, e a próxima onda é iniciada
		if (spawnedThisWave == totalToSpawnForWave && (enemiesAlive == 0 || (enemiesAlive == enemiesInLastFrame && timeSinceNoEnemiesKilled > 20))) {
			StartCoroutine("StartNextWave");
			return;
		}
			
		// Adiciona o tempo desde a última chamada de Update ao timer
		timer += Time.deltaTime;

        // Se o timer excede o tempo entre spawns, checa se precisa spawnar um inimigo, spawnando depois
		if (timer >= spawnTime) {
			// Gera um inimigo de cada entrada nesta onda
			// A dificuldade multiplica o número de inimigos gerados para cada "loop",
			// ou seja, a cada execução completa de todas as ondas
			foreach (Wave.Entry entry in currentWave.entries) {
				if (entry.spawned < (entry.count * difficulty)) {
					Spawn(entry);
				}
			}
		}

		numberEnemies.text = enemiesAlive.ToString();

		// Atualiza o tempo desde que o ultimo inimigo foi morto
		if (enemiesInLastFrame == enemiesAlive) {
			timeSinceNoEnemiesKilled += Time.deltaTime;
		} else {
			enemiesInLastFrame = enemiesAlive;
			timeSinceNoEnemiesKilled = 0;
		}
	}

	public void playEnemyTextAnimation(){
		numberEnemies.GetComponent<Animation>().Play();
	}

	// Ajeita a nova wave e seus requerimentos
	IEnumerator StartNextWave() {
		shouldSpawn = false;

		yield return new WaitForSeconds(timeBetweenWaves);

		if (waveNumber < waves.Length) {
			currentWave = waves[waveNumber];
		} else {
			difficulty++;

			// Redefinindo a variável "spawned" na última onda definida
			foreach (Wave.Entry entry in waves [waves.Length - 1].entries) {
				entry.spawned = 0;
			}

			currentWave = waves [waves.Length - 1];
		}

		// A dificuldade multiplica o número de inimigos gerados para cada "loop",
		// ou seja, a cada execução completa de todas as ondas
        totalToSpawnForWave = 0;
		foreach (Wave.Entry entry in currentWave.entries) {
			totalToSpawnForWave += (entry.count * difficulty);
		}

		spawnedThisWave = 0;
		shouldSpawn = true;

		waveNumber++;

		number.text = waveNumber.ToString ();//actualWaveNumber == 0 ? ((waveNumber + ((difficulty - 1) * waves.Length)).ToString()) : actualWaveNumber.ToString();
		number.GetComponent<Animation>().Play();
	}

	/**
 	* Gera inimigos
 	* 
 	 Este metodo e chamado em intervalos regulares, mas devido a todas as condicoes
 	 que podem impedir a geracao de um inimigo, pode haver muitos intervalos entre
 	 cada geracao real, fazendo com que os inimigos sejam gerados de forma muito
 	 irregular. Isso apenas faz com que pareça mais aleatorio.
 	*/
	void Spawn(Wave.Entry entry) {
		// Reseta o timer.
		timer = 0f;
		
		// Se a vida do player chega a 0, para de spawnar
		if (playerHealth.currentHealth <= 0f) {
			return;
		}
		
		// Encontra uma posicao aleatoria aproximadamente no nível
		Vector3 randomPosition = Random.insideUnitSphere * 35;
		randomPosition.y = 0;

		// Encontra a posicao mais proxima na malha de navegacao para nossa posicao aleatoria
		// Se nao for possivel encontrar uma posicao valida, retorna e tenta novamente
		UnityEngine.AI.NavMeshHit hit;
		if (!UnityEngine.AI.NavMesh.SamplePosition(randomPosition, out hit, 5, 1)) {
			return;
		}
		
		// Tem uma posicao valida de spawn no nav mesh
		spawnPosition = hit.position;
		
		// Verifica se essa posicao e visivel na tela, se for, retorna e tenta novamente
		Vector3 screenPos = Camera.main.WorldToScreenPoint(spawnPosition);
		if ((screenPos.x > -bufferDistance && screenPos.x < (Screen.width + bufferDistance)) && 
		    (screenPos.y > -bufferDistance && screenPos.y < (Screen.height + bufferDistance))) 
		{
			return;
		}

		// Passando por todas as checagens, spawna o inimigo
		GameObject enemy =  Instantiate(entry.enemy, spawnPosition, Quaternion.identity) as GameObject;
		// Multiplica a vida e o valor do score pela dificuldade atual
		enemy.GetComponent<EnemyHealth>().startingHealth *= difficulty;
		enemy.GetComponent<EnemyHealth>().scoreValue *= difficulty;
		
		entry.spawned++;
		spawnedThisWave++;
		enemiesAlive++;
		numberEnemies.text = enemiesAlive.ToString();
	}
}
