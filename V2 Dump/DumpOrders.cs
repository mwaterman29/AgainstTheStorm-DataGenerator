using System;
using System.Collections.Generic;
using BubbleStormTweaks;
using Eremite.Model;
using Eremite.Services;
using System.Linq;

namespace ATSDumpV2
{

    public class DumpOrders
    {
        public static int orderIndex = 0;
        public static int orderStepSize = 5;

        public static bool Step(List<Order> orders)
        {
            var allOrders = Plugin.GameSettings.orders;
            int thisStepMax = Math.Min(allOrders.Length, orderIndex + orderStepSize);

            for (; orderIndex < thisStepMax; orderIndex++)
            {
                var order = allOrders[orderIndex];
                var orderObj = new Order
                {
                    Name = order.Name,
                    LogicSets = new List<OrderLogicSet>(),
                    Rewards = new List<string>(),
                    ExcludedBiomes = new List<string>(),
                    ReputationReward = $"{order.reputationReward.DisplayName} {order.reputationReward.GetAmountText()}"
                };

                foreach (var orderLogicSet in order.logicsSets)
                {
                    var logicSet = new OrderLogicSet
                    {
                        Difficulty = orderLogicSet.difficulty.ToString(),
                        Logics = new List<string>(),
                        Rewards = new List<string>()
                    };

                    foreach (var logic in orderLogicSet.logics)
                    {
                        if (logic.Timed)
                            logicSet.Logics.Add(logic.Description);
                        else
                            logicSet.Logics.Add($"{logic.DisplayName} {logic.GetAmountText()}");
                    }

                    if (orderLogicSet.rewards != null)
                    {
                        foreach (var reward in orderLogicSet.rewards)
                        {
                            logicSet.Rewards.Add(reward?.Name ?? "<???>");
                        }
                    }
                    else
                    {
                        foreach (var reward in order.rewards)
                        {
                            logicSet.Rewards.Add($"{reward.DisplayName} {reward.GetAmountText()}");
                        }
                    }

                    orderObj.LogicSets.Add(logicSet);
                }

                if (order.excludeOnBiomes?.Length > 0)
                {
                    foreach (var exclude in order.excludeOnBiomes)
                    {
                        orderObj.ExcludedBiomes.Add(exclude.Name);
                    }
                }

                orders.Add(orderObj);
            }

            return orderIndex == allOrders.Length;
        }
    }
}