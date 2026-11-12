using System.Collections;
using UnityEngine;

public class TavernUpgrade : MonoBehaviour
{
    private static TavernUpgrade _activeUpgrade;

    [SerializeField] private Transform _tavernOverviewPoint;
    [SerializeField] private GameObject _tavernOverviewPanel;

    private Coroutine _openingRoutine;
    private PlayerMovement _playerMovement;
    private bool _playerMovementWasEnabled;
    private GameObject _playerUi;
    private bool _playerUiWasActive;
    private bool _isOpen;

    public void OpenTavernUpgrade()
    {
        if (_activeUpgrade != null || _isOpen || _openingRoutine != null)
            return;

        _activeUpgrade = this;
        LockPlayerAndHideUI();
        _openingRoutine = StartCoroutine(OpenTavernUpgradeCoroutine());
    }

    private IEnumerator OpenTavernUpgradeCoroutine()
    {
        CameraFocusManager.Instance.SetFocus(_tavernOverviewPoint, 32f);

        yield return new WaitUntil(() =>
            CameraFocusManager.Instance.IsAtTarget());
        
        _tavernOverviewPanel.SetActive(true);
        _isOpen = true;
        _openingRoutine = null;
    }

    public void CloseTavernUpgrade()
    {
        // UI button events may point to another TavernUpgrade component in the scene.
        // Always close through the instance that opened the upgrade flow and owns the lock.
        var owner = _activeUpgrade != null ? _activeUpgrade : this;
        owner.ClosePanelAndRestorePlayer();

        if (DialogueManager.Instance != null
            && DialogueManager.DialogueInProgress
            && DialogueManager.Instance.NPCSelected != null)
        {
            DialogueManager.Instance.ContinueDialogue();
        }
    }

    private void ClosePanelAndRestorePlayer()
    {
        if (_tavernOverviewPanel != null)
            _tavernOverviewPanel.SetActive(false);

        if (_openingRoutine != null)
        {
            StopCoroutine(_openingRoutine);
            _openingRoutine = null;
        }

        FinishClosingPanel();
    }

    private void FinishClosingPanel()
    {
        RestorePlayerAndUI();
        _isOpen = false;
        if (_activeUpgrade == this)
            _activeUpgrade = null;

        if (CameraFocusManager.Instance != null)
            CameraFocusManager.Instance.FocusPlayer();
    }

    private void LockPlayerAndHideUI()
    {
        var player = Player.Instance;
        _playerMovement = player != null
            ? player.GetComponent<PlayerMovement>()
            : FindFirstObjectByType<PlayerMovement>();

        if (_playerMovement != null)
        {
            _playerMovementWasEnabled = _playerMovement.enabled;
            _playerMovement.enabled = false;
        }

        if (player != null && player.TryGetComponent(out PlayerAnimations playerAnimations))
            playerAnimations.SetIsMovingAnimation(false);

        var dialogueManager = DialogueManager.Instance;
        _playerUi = dialogueManager != null ? dialogueManager.PlayerUI : null;
        if (_playerUi != null)
        {
            _playerUiWasActive = _playerUi.activeSelf;
            _playerUi.SetActive(false);
        }
    }

    private void RestorePlayerAndUI()
    {
        if (_playerMovement != null)
            _playerMovement.enabled = _playerMovementWasEnabled;

        if (_playerUi != null)
            _playerUi.SetActive(_playerUiWasActive);

        _playerMovement = null;
        _playerUi = null;
    }
}
