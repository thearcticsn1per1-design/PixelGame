using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace PixelGame.Editor
{
    /// <summary>
    /// Editor utility to create an 8-directional Animator Controller
    /// </summary>
    public class AnimatorSetup : EditorWindow
    {
        [MenuItem("Tools/Create 8-Directional Animator Controller")]
        public static void CreateAnimatorController()
        {
            // Create the animator controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath(
                "Assets/Art/Animations/PlayerAnimatorController.controller");

            if (controller == null)
            {
                Debug.LogError("Failed to create Animator Controller!");
                return;
            }

            // Add parameters
            AddParameters(controller);

            // Create a simple state machine (idle state)
            var rootStateMachine = controller.layers[0].stateMachine;
            var idleState = rootStateMachine.AddState("Idle");

            Debug.Log("Animator Controller created at: Assets/Art/Animations/PlayerAnimatorController.controller");
            Debug.Log("Parameters added. You now need to:");
            Debug.Log("1. Create animation clips for each direction");
            Debug.Log("2. Add states and transitions in the Animator window");
            Debug.Log("3. Assign the controller to your Player's Animator component");

            AssetDatabase.SaveAssets();
            Selection.activeObject = controller;
        }

        private static void AddParameters(AnimatorController controller)
        {
            // Movement state
            controller.AddParameter("isWalking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            // Direction bools for movement
            controller.AddParameter("MoveNorth", AnimatorControllerParameterType.Bool);
            controller.AddParameter("MoveNorthEast", AnimatorControllerParameterType.Bool);
            controller.AddParameter("MoveEast", AnimatorControllerParameterType.Bool);
            controller.AddParameter("MoveSouthEast", AnimatorControllerParameterType.Bool);
            controller.AddParameter("MoveSouth", AnimatorControllerParameterType.Bool);
            controller.AddParameter("MoveSouthWest", AnimatorControllerParameterType.Bool);
            controller.AddParameter("MoveWest", AnimatorControllerParameterType.Bool);
            controller.AddParameter("MoveNorthWest", AnimatorControllerParameterType.Bool);

            // Direction bools for idle facing
            controller.AddParameter("isNorth", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isNorthEast", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isEast", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isSouthEast", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isSouth", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isSouthWest", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isWest", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isNorthWest", AnimatorControllerParameterType.Bool);

            Debug.Log("Added all 18 animation parameters (isWalking, Death trigger, 8 Move directions, 8 idle directions)");
        }

        [MenuItem("Tools/Quick Fix - Assign Animator to Player")]
        public static void AssignAnimatorToPlayer()
        {
            // Find the player in the scene
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("No GameObject with 'Player' tag found in scene!");
                return;
            }

            var animator = player.GetComponent<Animator>();
            if (animator == null)
            {
                animator = player.AddComponent<Animator>();
                Debug.Log("Added Animator component to Player");
            }

            // Try to load the animator controller
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Art/Animations/PlayerAnimatorController.controller");

            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                Debug.Log("Assigned PlayerAnimatorController to Player's Animator!");
            }
            else
            {
                Debug.LogWarning("PlayerAnimatorController.controller not found. Run 'Create 8-Directional Animator Controller' first.");
            }

            // Also check if PlayerController script has the animator assigned
            var playerController = player.GetComponent<PixelGame.PlayerController>();
            if (playerController != null)
            {
                var so = new SerializedObject(playerController);
                so.FindProperty("animator").objectReferenceValue = animator;
                so.ApplyModifiedProperties();
                Debug.Log("Assigned Animator reference to PlayerController script!");
            }

            EditorUtility.SetDirty(player);
        }
    }
}
