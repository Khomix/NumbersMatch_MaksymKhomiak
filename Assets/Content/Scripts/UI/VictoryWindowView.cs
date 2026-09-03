using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Content.Scripts.UI
{
    public class VictoryWindowView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _restartButton;
        [SerializeField] private float _fadeDuration = 0.5f;

        public event Action OnRestartClicked;

        private void Awake()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);

                _restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, _fadeDuration).OnComplete(() =>
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });
        }

        public void Hide()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, _fadeDuration).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        private void OnDestroy()
        {
            _restartButton.onClick.RemoveAllListeners();
        }
    }
}
