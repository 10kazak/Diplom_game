using System;
using UnityEngine;

namespace AIEnemyLibrary.Sensors
{
    /// <summary>
    /// Глобальная система шума.
    /// Любой объект может отправить событие шума,
    /// а HearingSensor у врагов может на него подписаться.
    /// </summary>
    public static class NoiseSystem
    {
        public struct NoiseEvent
        {
            public Vector3 Position;
            public float Radius;
            public GameObject Source;
            public string NoiseTag;
        }

        public static event Action<NoiseEvent> OnNoiseMade;

        public static void MakeNoise(Vector3 position, float radius, GameObject source, string noiseTag = "Default")
        {
            NoiseEvent noiseEvent = new NoiseEvent
            {
                Position = position,
                Radius = radius,
                Source = source,
                NoiseTag = noiseTag
            };

            OnNoiseMade?.Invoke(noiseEvent);
        }
    }
}