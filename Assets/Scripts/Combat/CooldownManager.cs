using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public class CooldownManager : MonoBehaviour
    {
        private Dictionary<string, CooldownInfo> cooldowns = new();

        private class CooldownInfo
        {
            public float Duration { get; }
            public float RemainingTime { get; private set; }
            public bool IsReady => RemainingTime <= 0;

            public CooldownInfo(float duration)
            {
                Duration = duration;
                RemainingTime = 0;
            }

            public void StartCooldown()
            {
                RemainingTime = Duration;
            }

            public void Update(float deltaTime)
            {
                if (RemainingTime > 0)
                {
                    RemainingTime -= deltaTime;
                }
            }
        }

        public void RegisterCooldown(string attackName, float duration)
        {
            if (!cooldowns.ContainsKey(attackName))
            {
                cooldowns.Add(attackName, new CooldownInfo(duration));
            }
        }

        public bool IsReady(string attackName)
        {
            return cooldowns.ContainsKey(attackName) && cooldowns[attackName].IsReady;
        }

        public void TriggerCooldown(string attackName)
        {
            if (cooldowns.ContainsKey(attackName))
            {
                cooldowns[attackName].StartCooldown();
            }
        }

        private void Update()
        {
            foreach (var cooldown in cooldowns.Values)
            {
                cooldown.Update(Time.deltaTime);
            }
        }

        public float GetRemainingTime(string attackName)
        {
            return cooldowns.ContainsKey(attackName) ? cooldowns[attackName].RemainingTime : 0f;
        }
    }
} 