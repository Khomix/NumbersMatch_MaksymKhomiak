using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Content.Scripts.UI
{
    public class VictoryWindowController : IDisposable
    {
        private readonly VictoryWindowView _view;
        private readonly ColorGroupTracker _tracker;

        public VictoryWindowController(VictoryWindowView view, ColorGroupTracker tracker)
        {
            _view = view;
            _tracker = tracker;

            _tracker.OnGameCompleted += HandleGameCompleted;
            _view.OnRestartClicked += HandleRestartClicked;
        }

        private void HandleGameCompleted()
        {
            // Delay window popup by 1.5s so player sees the full diagonal wave animation on the board first
            DOVirtual.DelayedCall(1.5f, () =>
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
