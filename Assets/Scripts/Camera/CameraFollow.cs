using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	// Posicao que a camera vai seguir
	public Transform target; 
	// Velocidade da camera
	public float smoothing = 5f;     
	// referencia para o gameobj do minimapa
	public GameObject minimap;

	// Deslocamento inicial do alvo
	Vector3 offset;

	void Start() {
		// Calcula o deslocamento inicial
		offset = transform.position - target.position;
	}

	void FixedUpdate () {
		// Cria uma posição que a câmera está apontando com base no deslocamento do alvo
		Vector3 targetCamPos = target.position + offset;
		
		// Interpola suavemente entre a posicao atual da camera e a posicao do alvo 
		transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
	}

	void Update () {
		if (Input.GetButtonDown("MiniMap")) {
			// Ativa o minimapa
			minimap.SetActive(!minimap.activeInHierarchy);
		}
	}
}