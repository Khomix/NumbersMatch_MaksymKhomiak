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
    }
}