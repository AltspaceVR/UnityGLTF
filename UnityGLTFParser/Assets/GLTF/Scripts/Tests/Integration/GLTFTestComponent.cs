﻿using UnityEngine;
using System.Collections;
using GLTF;

public class GLTFTestComponent : MonoBehaviour {
	public string Url;
	public bool Multithreaded = true;

	public Shader GLTFStandard;


	IEnumerator Start()
	{
		var loader = new GLTFLoader(
				Url,
				gameObject.transform
			);
		loader.SetShaderForMaterialType(GLTFLoader.MaterialType.PbrMetallicRoughness, GLTFStandard);
		loader.Multithreaded = Multithreaded;
		yield return loader.Load();
		IntegrationTest.Pass();
	}
}
