using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Color Adjustments/Brightness")]
public class Brightness : MonoBehaviour {

	/// Fornece uma propriedade de shader que e definida no inspector
	/// e um material instanciado a partir do shader
    public Shader shaderDerp;
    Material m_Material;

    [Range(0f, 2f)]
    public float brightness = 1f;

    void Start() {
        // Desabilita se nao supoirta efeitos de imagem
        if (!SystemInfo.supportsImageEffects) {
            enabled = false;
            return;
        }

		// Eu desativo o efeito de imagem se o shader nao puder
		// rodar na placa de video do usuario
        if (!shaderDerp || !shaderDerp.isSupported)
            enabled = false;
    }


    Material material {
        get {
            if (m_Material == null) {
                m_Material = new Material(shaderDerp);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_Material;
        }
    }


    void OnDisable() {
        if (m_Material) {
            DestroyImmediate(m_Material);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        material.SetFloat("_Brightness", brightness);
        Graphics.Blit(source, destination, material);
    }
}
