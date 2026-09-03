using System;
using DG.Tweening;
using Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Content.Scripts.UI
{
    public class VictoryWindowController : IDisposable
    {
        private readonly VictoryWindowView _view;
        private readonly ColorGroupTracker _tracker;
        private readonly GameFeelSettings _gameFeel;

        public VictoryWindowController(VictoryWindowView view, ColorGroupTracker tracker, GameFeelSettings gameFeel = null)
        {
            _view = view;
            _tracker = tracker;
            _gameFeel = gameFeel;

            _tracker.OnGameCompleted += HandleGameCompleted;
            _view.OnRestartClicked += HandleRestartClicked;
        }

        private void HandleGameCompleted()
        {
            float popupDelay = _gameFeel != null ? _gameFeel.VictoryWindowPopupDelay : 1.5f;

            DOVirtual.DelayedCall(popupDelay, () =>
            {
                _view.Show();
            });
        }

        private void HandleRestartClicked()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        public void Dispose()
        {
            _tracker.OnGameCompleted -= HandleGameCompleted;
            _view.OnRestartClicked -= HandleRestartClicked;
        }
    }
}
