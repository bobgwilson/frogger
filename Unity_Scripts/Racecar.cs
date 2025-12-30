using UnityEngine;

/// <summary>
/// Racecar that speeds up after 6 laps and plays a sound effect for each lap after that.
/// </summary>
public class Racecar : MovingObject
{
    private int laps;
    private const int LapsBeforeSpeedup = 6;
    private const int FastSpeed = 120;

    /// <summary>
    /// Increments lap count and triggers speed/sound changes when lap threshold is reached.
    /// </summary>
    protected override void FinishedLap()
    {
        laps++;
        if (laps < LapsBeforeSpeedup) return;
        speed = FastSpeed;
        SoundManager.Instance.PlaySound(SoundManager.Instance.raceCar);
    }
}
