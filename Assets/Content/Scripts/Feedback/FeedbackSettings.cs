using UnityEngine;
using System.Collections.Generic;

public enum FeedbackType
{
    WrongPlacement,
    GroupComplete,
    PieceLand,
    ButtonClick
}

[System.Serializable]
public struct FeedbackData
{
    public FeedbackType Type;
    public AudioClip AudioClips;
    public bool UseCameraShake;
    public float ShakeDuration;
    public float ShakeAmplitude;
}

[CreateAssetMenu(fileName = "FeedbackSettings", menuName = "Settings/FeedbackSettings")]
public class FeedbackSettings : ScriptableObject
{
    public List<FeedbackData> Feedbacks = new List<FeedbackData>();

    public bool TryGetFeedback(FeedbackType type, out FeedbackData data)
    {
        foreach (var fb in Feedbacks)
        {
            if (fb.Type == type)
            {
                data = fb;
                return true;
            }
        }

        data = default;
        return false;
    }
}