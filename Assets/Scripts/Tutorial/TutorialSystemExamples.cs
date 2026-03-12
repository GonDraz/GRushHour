using Tutorial;
using UnityEngine;

/// <summary>
///     Example usage của Tutorial System API
///     Copy code snippets này để tích hợp tutorial vào game
/// </summary>
public class TutorialSystemExamples : MonoBehaviour
{
    // ============================================
    // EXAMPLE 2: Listen Tutorial Events
    // ============================================

    private void OnEnable()
    {
        // Subscribe to events
        TutorialManager.StepChanged += OnTutorialStepChanged;
        TutorialManager.TutorialEnded += OnTutorialEnded;
    }

    private void OnDisable()
    {
        // Unsubscribe
        TutorialManager.StepChanged -= OnTutorialStepChanged;
        TutorialManager.TutorialEnded -= OnTutorialEnded;
    }
    // ============================================
    // EXAMPLE 1: Check và Start Tutorial
    // ============================================

    /// <summary>
    ///     Kiểm tra và bắt đầu tutorial khi vào scene/page
    /// </summary>
    private void Example_CheckAndStartTutorial()
    {
        // Check xem tutorial đã hoàn thành chưa
        if (!TutorialManager.IsCompleted(TutorialType.GameplayIntro))
        {
            // Bắt đầu tutorial nếu chưa complete
            var started = TutorialManager.TryStart(TutorialType.GameplayIntro);

            if (started)
                Debug.Log("Tutorial started!");
            else
                Debug.Log("Tutorial could not start (no steps or already completed)");
        }
    }

    private void OnTutorialStepChanged(TutorialType type, TutorialStepData step)
    {
        Debug.Log($"Tutorial {type} - Step {step.Index}: {step.Message}");

        // Custom logic cho specific step
        if (step.TargetId == "power_up_button")
        {
            // Ví dụ: Play sound effect
            // AudioManager.PlaySFX("tutorial_step");
        }
    }

    private void OnTutorialEnded(TutorialType type)
    {
        Debug.Log($"Tutorial {type} completed!");

        // Custom logic khi tutorial kết thúc
        if (type == TutorialType.GameplayIntro)
        {
            // Ví dụ: Show reward popup
            // PopupManager.Show<TutorialRewardPopup>();
        }
    }

    // ============================================
    // EXAMPLE 3: Manual Tutorial Control
    // ============================================

    /// <summary>
    ///     Force next step manually (thay vì auto-complete từ button click)
    /// </summary>
    private void Example_ManualNextStep()
    {
        if (TutorialManager.IsActive) TutorialManager.NextStep();
    }

    /// <summary>
    ///     Force complete tutorial
    /// </summary>
    private void Example_ForceComplete()
    {
        TutorialManager.Complete(TutorialType.GameplayIntro);
    }

    /// <summary>
    ///     Reset tutorial progress (for testing)
    /// </summary>
    private void Example_ResetTutorial()
    {
        TutorialManager.ResetProgress(TutorialType.GameplayIntro);
    }

    // ============================================
    // EXAMPLE 4: Query Tutorial State
    // ============================================

    private void Example_QueryState()
    {
        // Check if any tutorial is currently active
        if (TutorialManager.IsActive)
        {
            Debug.Log($"Active tutorial: {TutorialManager.ActiveType}");
            Debug.Log($"Current step: {TutorialManager.ActiveStepIndex}");

            // Get current step data
            var currentStep = TutorialManager.GetCurrentStepData();
            if (currentStep != null)
            {
                Debug.Log($"Target: {currentStep.TargetId}");
                Debug.Log($"Message: {currentStep.Message}");
            }
        }

        // Check specific tutorial completion
        var gameplayCompleted = TutorialManager.IsCompleted(TutorialType.GameplayIntro);
        var powerUpCompleted = TutorialManager.IsCompleted(TutorialType.PowerUpIntro);

        Debug.Log($"Gameplay Tutorial: {(gameplayCompleted ? "Completed" : "Not Completed")}");
        Debug.Log($"PowerUp Tutorial: {(powerUpCompleted ? "Completed" : "Not Completed")}");
    }

    // ============================================
    // EXAMPLE 5: Conditional Tutorial Start
    // ============================================

    /// <summary>
    ///     Start tutorial based on game conditions
    /// </summary>
    private void Example_ConditionalStart()
    {
        // Chỉ start tutorial nếu player level thấp
        var playerLevel = 1; // Replace với actual player level

        if (playerLevel <= 3 && !TutorialManager.IsCompleted(TutorialType.PowerUpIntro))
            TutorialManager.TryStart(TutorialType.PowerUpIntro);
    }

    // ============================================
    // EXAMPLE 7: Skip Tutorial (Optional Feature)
    // ============================================

    /// <summary>
    ///     Cho phép player skip tutorial (nếu cần)
    /// </summary>
    private void Example_SkipTutorial()
    {
        if (TutorialManager.IsActive)
        {
            // Complete ngay lập tức
            TutorialManager.Complete(TutorialManager.ActiveType);

            Debug.Log("Tutorial skipped!");
        }
    }

    // ============================================
    // EXAMPLE 9: Multi-Step Tutorial Flow
    // ============================================

    /// <summary>
    ///     Example: Tutorial với nhiều bước liên tiếp
    ///     Không cần code - chỉ setup trong ScriptableObjects!
    /// </summary>
    private void Example_MultiStepTutorial()
    {
        /*
         * Tạo sequence với nhiều steps:
         *
         * GameplayIntro_Sequence:
         *   Step 1: "play_button" - "Nhấn để bắt đầu"
         *   Step 2: "aim_indicator" - "Kéo để ngắm"
         *   Step 3: "shoot_button" - "Thả ra để bắn"
         *   Step 4: "power_up_button" - "Dùng power-up"
         *
         * Hệ thống tự động:
         * - Hiển thị step 1
         * - Khi click button → Next step 2
         * - Khi click → Next step 3
         * - ... cho đến hết
         */

        // Code chỉ cần:
        TutorialManager.TryStart(TutorialType.GameplayIntro);
        // Hệ thống lo phần còn lại!
    }

    // ============================================
    // EXAMPLE 10: Analytics Integration (Optional)
    // ============================================

    /// <summary>
    ///     Track tutorial progress với analytics
    /// </summary>
    private void Example_AnalyticsIntegration()
    {
        TutorialManager.StepChanged += (type, step) =>
        {
            // Track step completion
            // AnalyticsManager.LogEvent("tutorial_step_complete", new Dictionary<string, object> {
            //     { "tutorial_type", type.ToString() },
            //     { "step_index", step.Index },
            //     { "step_id", step.Id }
            // });
        };

        TutorialManager.TutorialEnded += type =>
        {
            // Track tutorial completion
            // AnalyticsManager.LogEvent("tutorial_complete", new Dictionary<string, object> {
            //     { "tutorial_type", type.ToString() }
            // });
        };
    }

    // ============================================
    // EXAMPLE 6: Integration với Page/Screen System
    // ============================================

    /// <summary>
    ///     Typical integration pattern trong HomePage, LevelSelectionPage, etc.
    /// </summary>
    public class HomePageTutorialIntegration
    {
        public void OnPageAppear()
        {
            // Delay một chút để UI render xong
            // Invoke("StartTutorialIfNeeded", 0.5f);
        }

        private void StartTutorialIfNeeded()
        {
            // Priority: Start tutorial nào trước
            if (!TutorialManager.IsCompleted(TutorialType.GameplayIntro))
                TutorialManager.TryStart(TutorialType.GameplayIntro);
            else if (!TutorialManager.IsCompleted(TutorialType.PowerUpIntro))
                TutorialManager.TryStart(TutorialType.PowerUpIntro);
        }
    }

    // ============================================
    // EXAMPLE 8: Debug Tutorial in Editor
    // ============================================

#if UNITY_EDITOR
    [ContextMenu("Debug: Show Tutorial Status")]
    private void Debug_ShowStatus()
    {
        Debug.Log("=== Tutorial Debug Info ===");
        Debug.Log($"GameplayIntro: {(TutorialManager.IsCompleted(TutorialType.GameplayIntro) ? "✓" : "✗")}");
        Debug.Log($"PowerUpIntro: {(TutorialManager.IsCompleted(TutorialType.PowerUpIntro) ? "✓" : "✗")}");

        if (TutorialManager.IsActive)
            Debug.Log($"Active: {TutorialManager.ActiveType} (Step {TutorialManager.ActiveStepIndex})");
    }

    [ContextMenu("Debug: Reset All Tutorials")]
    private void Debug_ResetAll()
    {
        TutorialManager.ResetProgress(TutorialType.GameplayIntro);
        TutorialManager.ResetProgress(TutorialType.PowerUpIntro);
        Debug.Log("All tutorials reset!");
    }

    [ContextMenu("Debug: Start Gameplay Tutorial")]
    private void Debug_StartGameplay()
    {
        TutorialManager.ResetProgress(TutorialType.GameplayIntro);
        TutorialManager.TryStart(TutorialType.GameplayIntro);
    }
#endif
}