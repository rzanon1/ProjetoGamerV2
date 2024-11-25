using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour {

	// Tempo entre os ataques em segundos
	public float timeBetweenAttacks = 1f;  
	// Dano do ataque
	public int attackDamage = 10;               
	   
	// Referencia ao gameobj do player
	GameObject player;       
	// Referencia a vida do player
	PlayerHealth playerHealth;     
	// Referencia a vida do inimigo
	EnemyHealth enemyHealth; 
	// Verificar se o player está na dentro do trigger do collider e pode ser atacado
	bool playerInRange;   
	// Timer para o próximo ataque
	float timer;                               
	
	void Awake() {
		// Arrumando as referencias
		player = GameObject.FindGameObjectWithTag("Player");
		playerHealth = player.GetComponent<PlayerHealth>();
		enemyHealth = GetComponent<EnemyHealth>();
	}

	void OnTriggerEnter(Collider other) {
		// Verificando se o colisor que está na distancia é o do player
		if (other.gameObject == player) {
			playerInRange = true;
			// Um pequeno tempo de reacao
			timer = 0.2f;
		}
	}
	
	
	void OnTriggerExit(Collider other) {
		// Verificando se o colisor do player nao esta mais no range
		if (other.gameObject == player) { 
			playerInRange = false;
		}
	}
	
	
	void Update() {
		// Adiciona o tempo desde que o ultimo update foi chamado para o timer
		timer += Time.deltaTime;
		
		// Se o tempo excede o tempo entre ataques o player está dentro do range, o inimigo esta vivo e o player, ataque realizado
		if (timer >= timeBetweenAttacks && playerInRange && enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0) {
			Attack();
		}
	}
	
	
	void Attack() {
		// Reseta o timer.
		timer = 0f;
		
		// Aplica o dano no player
		playerHealth.TakeDamage(attackDamage);
	}
}