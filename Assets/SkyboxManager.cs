using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager : MonoBehaviour {
   [SerializeField] private Material[] Skyboxes;

   void ChangeSkybox(Material mat) {
      RenderSettings.skybox = mat;
   }
}
