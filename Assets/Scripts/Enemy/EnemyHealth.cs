using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour {

	public int startingHealth = 100;  
	[HideInInspector]
	public int currentHealth;  
	// Velocidade em que o inimigo entra no chao quando morto
	public float sinkSpeed = 2.5f;   
	// Quantidade de score que se adiciona para o player quando o inimigo morre
	public int scoreValue = 20; 
	// O som que toca quando o inimigo morre
	public AudioClip deathClip;    
	// O som que toca quando o inimigo queima
	public AudioClip burnClip;  
	// O sistema de particulas que aparece quando o inimigo queima
	public ParticleSystem deathParticles;  
	// A barra de vida que aparece sobre a cabeca
	public Slider healthBarSlider;
	// Aparece dois olhos quando o inimigo é derrotado
	public GameObject eye;
	
	// A instancia do slider da barra de vida para esse inimigo
	Slider sliderInstance;
	// Verificacao da morte do inimigo
	bool isDead;
	// Verificacao se o inimigo esta queimando
	bool isBurning = false;
	// A rim color do shader, muda para simular um efeito vermelho de hit
	Color rimColor;
    // Mudando a rim power para produzir um efeito melhor
    float rimPower;
	// Esse é o valor de cutoff do shader de dissolução. Modificamos esse valor para controlar a dissolução do corpo quando ele é queimado.
    
    float cutoffValue = 0f;
	// Componentes e scripts que precisam ser referenciados
	Animator anim;            
	AudioSource enemyAudio;        
	CapsuleCollider capsuleCollider;   
	SkinnedMeshRenderer myRenderer;
	GameObject enemyHealthbarManager;
	WaveManager waveManager;
	ScoreManager scoreManager;
	PickupManager pickupManager;

	void Awake() {
		anim = GetComponent<Animator>();
		enemyAudio = GetComponent<AudioSource>();
		capsuleCollider = GetComponent<CapsuleCollider>();
		myRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		enemyHealthbarManager = GameObject.Find("EnemyHealthbarsCanvas");
		waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
		scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
		pickupManager = GameObject.Find("PickupManager").GetComponent<PickupManager>();
	}

	void Start() {
		currentHealth = startingHealth;

		// Instantiate our health bar GUI slider.
		sliderInstance = Instantiate(healthBarSlider, gameObject.transform.position, Quaternion.identity) as Slider;
		sliderInstance.gameObject.transform.SetParent(enemyHealthbarManager.transform, false);
		sliderInstance.GetComponent<Healthbar>().enemy = gameObject;
		sliderInstance.gameObject.SetActive(false);


		rimColor = myRenderer.materials[0].GetColor("_RimColor");
        rimPower = myRenderer.materials[0].GetFloat("_RimPower");
    }

	void Update() {
		// Se estiver queimando, atualiza o valor de cutoff dos materiais
		// para que eles se dissolvam gradualmente ao longo do tempo.
		if (isBurning) {
			cutoffValue = Mathf.Lerp(cutoffValue, 1f, 1.3f * Time.deltaTime);
			myRenderer.materials[0].SetFloat("_Cutoff", cutoffValue);
			myRenderer.materials[1].SetFloat("_Cutoff", 1);
		}
	}

	public void TakeDamage(int amount, Vector3 hitPoint) {
        StopCoroutine("Ishit");
        StartCoroutine("Ishit");

		// Se o inimigo está morto nao precisa tomar mais dano, entao sai da funcao
		if (isDead)
			return;

		GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * -300, hitPoint);
		
		// Reduz a saúde atual pela quantidade de dano sofrido
		currentHealth -= amount;

		// Ajusta o valor da barra de vida para a vida atual
		if (currentHealth <= startingHealth) {
			sliderInstance.gameObject.SetActive(true);
		}
		int sliderValue = (int) Mathf.Round(((float)currentHealth / (float)startingHealth) * 100);
		sliderInstance.value = sliderValue;
		
		// Se o valor da vida atual é menos ou igual a 0 o inimigo esta morto
		if (currentHealth <= 0) {
			Death();
		}
	}

	IEnumerator Ishit() {
		Color newColor = new Color(10, 0, 0, 0);
        float newPower = 0.5f;

		myRenderer.materials[0].SetColor("_RimColor", newColor);
        myRenderer.materials[0].SetFloat("_RimPower", newPower);

        float time = 0.25f;
		float elapsedTime = 0;
		while (elapsedTime < time) {
			newColor = Color.Lerp(newColor, rimColor, elapsedTime / time);
			myRenderer.materials[0].SetColor("_RimColor", newColor);
            newPower = Mathf.Lerp(newPower, rimPower, elapsedTime / time);
            myRenderer.materials[0].SetFloat("_RimPower", newPower);
            elapsedTime += Time.deltaTime;
			yield return null;
		}
        myRenderer.materials[0].SetColor("_RimColor", rimColor);
        myRenderer.materials[0].SetFloat("_RimPower", rimPower);
    }

	void Death() {
		isDead = true;

		// Diz para o animador que o inimigo esta morto
		anim.SetTrigger("Dead");
		
		// Muda o clipe de audio para a fonte do audio de morte e toca
		enemyAudio.clip = deathClip;
		enemyAudio.Play();

		// Acha e desabilita o agente do Nav Mesh
		if (GetComponent<UnityEngine.AI.NavMeshAgent>()) {
			GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
		}
		
		// Acha o componente rigidbody e o faz kinematic
		GetComponent<Rigidbody>().isKinematic = true;

		// Adiciona a pontuacao pelo valor do inimigo
		scoreManager.AddScore(scoreValue);

		waveManager.enemiesAlive--;

		// Transforma o collider em um trigger para que os tiros passem por ele
		capsuleCollider.isTrigger = true;

		// Remove esse objeto
		StartCoroutine(StartSinking());
		waveManager.playEnemyTextAnimation ();

		Destroy(sliderInstance.gameObject);
	}

	IEnumerator StartSinking() {
		yield return new WaitForSeconds(2);

		isBurning = true;


		deathParticles.Play();

		enemyAudio.clip = burnClip;
		enemyAudio.Play();

		// Spawna os dois olhos
		for (int i = 0; i < 2; i++) {
			GameObject instantiatedEye = Instantiate(eye, transform.position + new Vector3(0, 0.3f, 0), transform.rotation) as GameObject;
			instantiatedEye.GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3 (Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f)));
		}

		SpawnPickup();


		Destroy(gameObject, 3f);
	}

	/** 
	 * Chance de spawnar um power up na morte
	 */
	void SpawnPickup() {
		// Spawna um pouco acima do chao
		Vector3 spawnPosition = transform.position + new Vector3(0, 0.3f, 0);

		// Spawna o power up de tiro extra apos um certo score
		if (scoreManager.GetScore () >= pickupManager.scoreNeededForExtraBullet) {
			Instantiate (pickupManager.bulletPickup, spawnPosition, transform.rotation);

			// Aumenta o score necessario para spawnar um power up
			pickupManager.scoreNeededForExtraBullet += pickupManager.extraScoreNeededAfterEachPickup;
		} else {
			// Gera aleatoriamente um dos 3 powerups.
			// Está configurado para gerar um powerup 20% das vezes.
			// E os powerups sao selecionados da seguinte forma:
			// - 30% das vezes sera um powerup de ricochete (bounce pickup)
			// - 20% das vezes sera um powerup de perfuracao (pierce pickup)
			// - 50% das vezes sera um powerup de saúde (health pickup)

			float rand = Random.value;
			if (rand <= 0.2f) {
				// Bounce.
				if (rand <= 0.06f) {
					Instantiate (pickupManager.bouncePickup, spawnPosition, transform.rotation);
				}
			// Pierce.
			else if (rand > 0.06f && rand <= 0.1f) {
					Instantiate (pickupManager.piercePickup, spawnPosition, transform.rotation);
				}
			// Health.
			else {
					Instantiate (pickupManager.healthPickup, spawnPosition, transform.rotation);
				}
			}
		}
	}
}
