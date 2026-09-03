using DG.Tweening;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "GameFeel", menuName = "Settings/GameFeel")]
    public class GameFeelSettings : ScriptableObject
    {
        [Header("Drag Settings")]
        public float DragZDepth = 55f;
    
        [Tooltip("Offset applied to the piece relative to the cursor position.")]
        public Vector3 DragOffset = new Vector3(0, 1.5f, 0); 
    
        [Tooltip("Speed of the smooth movement towards the target offset.")]
        public float DragSmoothSpeed = 15f; 

        [Header("Drop Success Animation")]
        public float MoveToPositionDuration = 0.5f;
        public Ease MoveToPositionEase = Ease.OutBack;

        [Header("Drop Fail Animation (Return)")]
        public float MoveBackDuration = 0.5f;
        public Ease MoveBackEase = Ease.OutBack;

        [Header("Magnetic Snap & Assist Settings")]
        [Tooltip("Strength of magnetic snap towards matching target slot (0 = none, 1 = instant).")]
        public float MagneticSnapStrength = 0.6f;

        [Tooltip("Strength of magnetic repulsion pushing away from wrong target slot.")]
        public float MagneticRepulsionStrength = 0.2f;

        [Header("Hover Highlight Colors")]
        public Color MatchHighlightColor = Color.green;
        public Color MismatchHighlightColor = Color.red;
        public Color NeutralHighlightColor = new Color(0.85f, 0.85f, 0.85f, 1f);

        [Header("Group Reveal Settings")]
        public float GroupRevealDelayScale = 0.4f;
        public float RevealDissolveDuration = 0.6f;
        public Ease RevealDissolveEase = Ease.OutQuad;

        [Header("Victory Wave Animation")]
        public float VictoryWaveDuration = 1.0f;
        public float VictoryJumpPower = 0.6f;
        public float VictoryJumpDuration = 0.4f;
        public Ease VictoryJumpEase = Ease.OutQuad;

        [Header("Victory UI Window")]
        public float VictoryWindowPopupDelay = 1.5f;
        public float VictoryWindowFadeDuration = 0.5f;
    }
}