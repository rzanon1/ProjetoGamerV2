using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour {
	
	public float speed = 600.0f;
	public float life = 3;
	public ParticleSystem normalTrailParticles;
	public ParticleSystem bounceTrailParticles;
	public ParticleSystem pierceTrailParticles;
	public ParticleSystem ImpactParticles;
	public int damage = 20;
	public bool piercing = false;
	public bool bounce = false;
	public Color bulletColor;
	public AudioClip bounceSound;
	public AudioClip hitSound;

	Vector3 velocity;
    Vector3 force;
	Vector3 newPos;
	Vector3 oldPos;
	Vector3 direction;
	bool hasHit = false;
	RaycastHit lastHit;
	// Reference to the audio source.
	AudioSource bulletAudio;  
	float timer;

	void Awake() {
		bulletAudio = GetComponent<AudioSource> ();
	}

	void Start() {
		newPos = transform.position;
		oldPos = newPos;

		// Set our particle colors.
		var main = normalTrailParticles.main;
		main.startColor = bulletColor;
		main = bounceTrailParticles.main;
		main.startColor = bulletColor;
		main = pierceTrailParticles.main;
		main.startColor = bulletColor;
		main = ImpactParticles.main;
		main.startColor = bulletColor;

		normalTrailParticles.gameObject.SetActive(true);
		if (bounce) {
			bounceTrailParticles.gameObject.SetActive(true);
			normalTrailParticles.gameObject.SetActive(false);
			life = 1;
			speed = 20;
		}
		if (piercing) {
			pierceTrailParticles.gameObject.SetActive(true);
			normalTrailParticles.gameObject.SetActive(false);
			speed = 40;
		}
	}

	void Update() {
		if (hasHit) {
			return;
		}
			
		timer += Time.deltaTime;

		// Destruir depois de um tempo caso a bala nao atinja nada

		if (timer >= life) {
			Dissipate();
		}

        velocity = transform.forward;
		velocity.y = 0;
		velocity = velocity.normalized * speed;

		// Assumir que move completamente
		newPos += velocity * Time.deltaTime;
	
		// Verificar se nao acerta algo no caminho
		direction = newPos - oldPos;
		float distance = direction.magnitude;

		if (distance > 0) {
            RaycastHit[] hits = Physics.RaycastAll(oldPos, direction, distance);

		    // Find the first valid hit
		    for (int i = 0; i < hits.Length; i++) {
		        RaycastHit hit = hits[i];

				if (ShouldIgnoreHit(hit)) {
					continue;
				}

				// notify hit
				OnHit(hit);

				lastHit = hit;

				if (hasHit) {
					newPos = hit.point;
					break;
				}
		    }
		}

		oldPos = transform.position;
		transform.position = newPos;
	}

	/**
 	* Evita atingir o mesmo inimigo duas vezes com o mesmo raycast
 	* quando tem Piercing shot. O tiro ainda pode ricochetear
 	* em uma parede, voltar e atingir o inimigo novamente se tivermos
 	* tiros tanto perfurantes quanto ricocheteantes.
 */
	bool ShouldIgnoreHit (RaycastHit hit) {
		if (lastHit.point == hit.point || lastHit.collider == hit.collider)
			return true;
		
		return false;
	}

	/**
 	* Determinar o que fazer quando atinge algo.
 	*/

	void OnHit(RaycastHit hit) {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        if (hit.transform.tag == "Environment") {
			newPos = hit.point;
			ImpactParticles.transform.position = hit.point;
			ImpactParticles.transform.rotation = rotation;
			ImpactParticles.Play();
			if (bounce) {
				Vector3 reflect = Vector3.Reflect(direction, hit.normal);
				transform.forward = reflect;
				bulletAudio.clip = bounceSound;
				bulletAudio.pitch = Random.Range(0.8f, 1.2f);
				bulletAudio.Play();
			}
			else {
				hasHit = true;
				bulletAudio.clip = hitSound;
				bulletAudio.volume = 0.5f;
				bulletAudio.pitch = Random.Range(1.2f, 1.3f);
				bulletAudio.Play();
				DelayedDestroy();
			}
        }

        if (hit.transform.tag == "Enemy") {
			ImpactParticles.transform.position = hit.point;
			ImpactParticles.transform.rotation = rotation;
			ImpactParticles.Play();

			// Tentar e achar o script da vida do inimiga no gameobj de acerto
			EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
			
			// Se o componente da vida do inimigo existe
			if (enemyHealth != null) {
				// Ele deve tomar o dano
				enemyHealth.TakeDamage(damage, hit.point);
			}
			if (!piercing) {
            	hasHit = true;
				DelayedDestroy();
			}
			bulletAudio.clip = hitSound;
			bulletAudio.volume = 0.5f;
			bulletAudio.pitch = Random.Range(1.2f, 1.3f);
			bulletAudio.Play();
        }
	}

	// Metodo para destruir o objeto do jogo, mas que
	// primeiro desanexa o efeito de particula e o deixa
	// por um segundo. Chamado se a bala terminar sua vida
	// no ar para criar o efeito de desaparecer gradualmente
	// ao inves de sumir imediatamente.

	void Dissipate() {
		var normalTrailParticlesEmission = normalTrailParticles.emission.enabled;
		normalTrailParticlesEmission = false;
		normalTrailParticles.transform.parent = null;
		var normalTrailParticlesMain = normalTrailParticles.main;
		Destroy(normalTrailParticles.gameObject, normalTrailParticlesMain.duration);

		if (bounce) {
			var bounceParticlesEmission = bounceTrailParticles.emission.enabled;
			bounceParticlesEmission = false;
			bounceTrailParticles.transform.parent = null;
			var bounceTrailParticlesMain = bounceTrailParticles.main;
			Destroy(bounceTrailParticles.gameObject, bounceTrailParticlesMain.duration);
		}
		if (piercing) {
			var pierceTrailParticlesEmission = pierceTrailParticles.emission.enabled;
			pierceTrailParticlesEmission = false;
			pierceTrailParticles.transform.parent = null;
			var pierceTrailParticlesMain = pierceTrailParticles.main;
			Destroy(pierceTrailParticles.gameObject, pierceTrailParticlesMain.duration);
		}

		Destroy(gameObject);
	}

	void DelayedDestroy() {
		normalTrailParticles.gameObject.SetActive(false);
		if (bounce) {
			bounceTrailParticles.gameObject.SetActive(false);
		}
		if (piercing) {
			pierceTrailParticles.gameObject.SetActive(false);
		}
		Destroy(gameObject, hitSound.length);
	}
}