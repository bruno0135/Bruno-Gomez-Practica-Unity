using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BrunoGomez
{
    [InitializeOnLoad]
    public class AutoSetupPlayer
    {
        static AutoSetupPlayer()
        {
            EditorApplication.update += RunOnce;
        }

        private static void RunOnce()
        {
            EditorApplication.update -= RunOnce;
            Setup();
        }

        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            // Try to find the player
            GameObject player = GameObject.Find("sky_protectiv_suit_rig_UNITY");
            if (player == null) player = GameObject.FindWithTag("Player");
            
            if (player == null) return;

            bool changed = false;

            // 0. Tag the player
            if (!player.CompareTag("Player"))
            {
                player.tag = "Player";
                changed = true;
            }

            // 1. CharacterController
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc == null) 
            {
                cc = player.AddComponent<CharacterController>();
                cc.center = new Vector3(0, 0.93f, 0);
                cc.height = 1.8f;
                cc.radius = 0.28f;
                cc.skinWidth = 0.08f;
                cc.minMoveDistance = 0.001f;
                changed = true;
            }

            // 2. SimplePlayerController
            SimplePlayerController spc = player.GetComponent<SimplePlayerController>();
            if (spc == null) 
            {
                spc = player.AddComponent<SimplePlayerController>();
                // Adjusting defaults for head-level view
                spc.cameraOffset = new Vector3(0, 1.7f, 0.2f); // Slightly forward and at head height
                spc.cameraDistance = 0.5f; // Very close to the head
                spc.lookSensitivity = 2.0f;
                changed = true;
            }

            // 3. Animator Controller
            Animator anim = player.GetComponent<Animator>();
            if (anim != null && anim.runtimeAnimatorController == null)
            {
                string controllerPath = "Assets/UnityTechnologies/SpaceRobotKyle/Animations/StarterAssetsThirdPerson.controller";
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller != null) 
                {
                    anim.runtimeAnimatorController = controller;
                    changed = true;
                }
            }

            // 4. Materials
            SkinnedMeshRenderer[] renderers = player.GetComponentsInChildren<SkinnedMeshRenderer>();
            string[] matPaths = {
                "Assets/Sky_Protective_suit/Materials/M_Protectiv_suit.mat",
                "Assets/UnityTechnologies/SpaceRobotKyle/Materials/Kyle_Material.mat"
            };
            
            Material suitMat = null;
            foreach (var path in matPaths)
            {
                suitMat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (suitMat != null) break;
            }

            if (suitMat != null)
            {
                // Force URP/Lit if pink or if we detect URP
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null) suitMat.shader = urpShader;

                foreach (var renderer in renderers)
                {
                    renderer.sharedMaterial = suitMat;
                    changed = true;
                }
            }

            // 5. MotionSpeed initialization
            if (anim != null)
            {
                anim.SetFloat("MotionSpeed", 1.0f);
            }

            // 6. Doors Setup
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            int doorCount = 0;
            foreach (GameObject go in allObjects)
            {
                string lowerName = go.name.ToLower();
                
                // Specifically targeting the Sci-Fi Kit door naming convention
                bool isScifiDoor = lowerName.StartsWith("door_left") || lowerName.StartsWith("door_right") || lowerName.StartsWith("door_ver");
                bool isGenericDoor = (lowerName.Contains("door") || lowerName.Contains("gate")) && 
                                     !lowerName.Contains("panel") && 
                                     !lowerName.Contains("frame") &&
                                     !lowerName.Contains("the_doors");

                if ((isScifiDoor || isGenericDoor) && go.GetComponent<AutomaticDoor>() == null)
                {
                    // For Sci-Fi kit doors, they might be individual objects. 
                    // We attach the script and it will handle moving itself or its panels.
                    go.AddComponent<AutomaticDoor>();
                    
                    // CRITICAL: Disable static for the object and all its mesh children
                    go.isStatic = false;
                    foreach (MeshRenderer mr in go.GetComponentsInChildren<MeshRenderer>()) mr.gameObject.isStatic = false;
                    foreach (Transform t in go.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = false;
                    
                    doorCount++;
                    changed = true;
                }
            }
            if (doorCount > 0) Debug.Log("AutomaticDoor: Configured " + doorCount + " interactive doors. Press 'E' to open!");

            // 7. Camera
            GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCam == null)
            {
                mainCam = new GameObject("Main Camera");
                mainCam.tag = "MainCamera";
                mainCam.AddComponent<Camera>();
                mainCam.AddComponent<AudioListener>();
                changed = true;
            }

            if (spc != null && spc.cameraTransform == null)
            {
                spc.cameraTransform = mainCam.transform;
                changed = true;
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(player.scene);
                Debug.Log("<b>[Antigravity]</b>: He configurado automáticamente tu personaje, la cámara y las animaciones.");
            }
        }
    }
}
#endif
