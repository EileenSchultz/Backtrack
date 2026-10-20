using System;
using System.Collections.Generic;

namespace BacktrackVisualizer
{
    public class StepInfo
    {
        public string Action { get; set; } // "Choose", "Undo", "BaseCase"
        public List<int> Current { get; set; }
        public bool[] Used { get; set; }
        public int Value { get; set; }
    }

    public class PermutationsBacktrack
    {
        public IEnumerable<StepInfo> GenerateSteps(int[] nums)
        {
            if (nums == null || nums.Length == 0) yield break;

            var current = new List<int>();
            var used = new bool[nums.Length];

            foreach (var step in Backtrack(nums, current, used))
            {
                yield return step;
            }
        }

        private IEnumerable<StepInfo> Backtrack(int[] nums, List<int> current, bool[] used)
        {
            if (current.Count == nums.Length)
            {
                yield return new StepInfo
                {
                    Action = "BaseCase",
                    Current = new List<int>(current),
                    Used = (bool[])used.Clone()
                };
                yield break;
            }

            for (int i = 0; i < nums.Length; i++)
            {
                if (used[i]) continue;

                used[i] = true;
                current.Add(nums[i]);

                yield return new StepInfo
                {
                    Action = "Choose",
                    Current = new List<int>(current),
                    Used = (bool[])used.Clone(),
                    Value = nums[i]
                };

                // Рекурсивно получаем следующие шаги
                foreach (var step in Backtrack(nums, current, used))
                {
                    yield return step;
                }

                current.RemoveAt(current.Count - 1);
                used[i] = false;

                yield return new StepInfo
                {
                    Action = "Undo",
                    Current = new List<int>(current),
                    Used = (bool[])used.Clone(),
                    Value = nums[i]
                };
            }
        }
    }
}