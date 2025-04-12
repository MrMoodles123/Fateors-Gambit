using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialTextObject; // Assign UI text in Inspector
    private TextMeshProUGUI tutorialText;
    private bool movementUnlocked = false;
    private bool canRemoveBarrier = false; // Only allow removal after movement
    private bool jumpUnlocked = false;

    private InputAction moveAction;
    private InputAction jumpAction;

    void Start()
    {
        tutorialText = tutorialTextObject.GetComponent<TextMeshProUGUI>();

        moveAction = new InputAction("Move", binding: "<Keyboard>/w");
        moveAction.AddBinding("<Keyboard>/a");
        moveAction.AddBinding("<Keyboard>/s");
        moveAction.AddBinding("<Keyboard>/d");

        jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");

        moveAction.Enable();
        moveAction.Disable(); // Disable movement initially
        jumpAction.Disable(); // Disable jumping initially

        StartCoroutine(RunTutorial());
    }

    IEnumerator RunTutorial()
    {
        // Step 1: Wait for WASD input
        tutorialText.text = "Move with WASD!";
        yield return new WaitUntil(() => IsMovementPressed());

        movementUnlocked = true;
        moveAction.Enable();

        // Step 2: Walk through barrier
        tutorialText.text = "Walk through the barrier to remove it!";

        // Step 3: Enable jumping tutorial
        yield return new WaitForSeconds(1f);
        tutorialText.text = "Jump over the obstacle by pressing SPACE!";
        jumpAction.Enable();
        yield return new WaitUntil(() => IsJumpPressed());
        Debug.Log("Jumped!");
        jumpUnlocked = true;
        tutorialText.text = "Great! You've learned how to jump!";
    }

    private bool IsMovementPressed()
    {
        return Keyboard.current.wKey.isPressed ||
               Keyboard.current.aKey.isPressed ||
               Keyboard.current.sKey.isPressed ||
               Keyboard.current.dKey.isPressed;
    }

    private bool IsJumpPressed()
    {
        return Keyboard.current.spaceKey.wasPressedThisFrame;
    }
}
