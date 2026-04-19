using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public struct TutorialStep
    {
        [TextArea] public string instructionText;
        public TutorialAction requiredAction;
        public float delayAfterCompletion;
        public Damageable targetEnemy; // For KillEnemy action
    }

    public enum TutorialAction
    {
        None,
        Move,
        Jump,
        Attack,
        KillEnemy
    }

    public List<TutorialStep> steps;
    public TMP_Text instructionTextUI;
    public GameObject tutorialPanel; 

    private int currentStepIndex = 0;
    private bool isWaitingForAction = false;
    private PlayerController playerController;
    private Damageable currentTargetEnemy;

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found! TutorialManager needs a PlayerController in the scene.");
            return;
        }

        // Subscribe to events
        playerController.Moved += OnPlayerMoved;
        playerController.Jumped += OnPlayerJumped;
        playerController.Attacked += OnPlayerAttacked;

        if (steps.Count > 0)
        {
            ShowStep(currentStepIndex);
        }
        else
        {
            if(instructionTextUI) instructionTextUI.text = "No tutorial steps defined.";
        }
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.Moved -= OnPlayerMoved;
            playerController.Jumped -= OnPlayerJumped;
            playerController.Attacked -= OnPlayerAttacked;
        }
        
        // Clean up enemy listener if active
        if (currentTargetEnemy != null)
        {
            currentTargetEnemy.damageableDeath.RemoveListener(OnEnemyKilled);
        }
    }

    private void ShowStep(int index)
    {
        if (index >= steps.Count)
        {
            CompletedTutorial();
            return;
        }

        TutorialStep step = steps[index];
        if(instructionTextUI) instructionTextUI.text = step.instructionText;
        if(tutorialPanel) tutorialPanel.SetActive(true);
        
        isWaitingForAction = true;
        
        if (step.requiredAction == TutorialAction.None)
        {
            StartCoroutine(AutoAdvance(Mathf.Max(step.delayAfterCompletion, 2f)));
        }
        else if (step.requiredAction == TutorialAction.KillEnemy)
        {
            if (step.targetEnemy != null)
            {
                if (!step.targetEnemy.IsAlive)
                {
                    // Already dead
                    CompleteStep();
                }
                else
                {
                    currentTargetEnemy = step.targetEnemy;
                    currentTargetEnemy.damageableDeath.AddListener(OnEnemyKilled);
                }
            }
            else
            {
                Debug.LogWarning($"Step {index} requires KillEnemy but no TargetEnemy is assigned!");
                // Auto skip if broken
                CompleteStep();
            }
        }
    }

    private IEnumerator AutoAdvance(float delay)
    {
        // Prevent double completion if user manages to trigger something else? 
        // Although None requires no action so it shouldn't be an issue
        yield return new WaitForSeconds(delay);
        CompleteStep();
    }

    private void CompleteStep()
    {
        if (!isWaitingForAction) return;

        // Cleanup listener if we just finished a KillEnemy step
        if (currentTargetEnemy != null)
        {
            currentTargetEnemy.damageableDeath.RemoveListener(OnEnemyKilled);
            currentTargetEnemy = null;
        }

        isWaitingForAction = false;
        TutorialStep step = steps[currentStepIndex];
        
        StartCoroutine(WaitAndNext(step.delayAfterCompletion));
    }

    private IEnumerator WaitAndNext(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentStepIndex++;
        
        if (currentStepIndex < steps.Count)
        {
            ShowStep(currentStepIndex);
        }
        else
        {
            CompletedTutorial();
        }
    }
    
    private void CompletedTutorial()
    {
        if(instructionTextUI) instructionTextUI.text = "Tutorial Complete!";
        StartCoroutine(FinishAndLoadNextLevel(3f));
    }

    private IEnumerator FinishAndLoadNextLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        if(tutorialPanel) tutorialPanel.SetActive(false);
        
        // Load Next Level (Map 1)
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        
        if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels or build settings not configured.");
            // Return to Main Menu if no next level
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    // Event Handlers
    private void OnPlayerMoved()
    {
        CheckAction(TutorialAction.Move);
    }

    private void OnPlayerJumped()
    {
        CheckAction(TutorialAction.Jump);
    }

    private void OnPlayerAttacked()
    {
        CheckAction(TutorialAction.Attack);
    }
    
    private void OnEnemyKilled()
    {
        CheckAction(TutorialAction.KillEnemy);
    }

    private void CheckAction(TutorialAction action)
    {
        if (!isWaitingForAction) return;
        if (currentStepIndex >= steps.Count) return;

        if (steps[currentStepIndex].requiredAction == action)
        {
            CompleteStep();
        }
    }
}
