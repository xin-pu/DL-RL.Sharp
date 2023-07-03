﻿using DeepSharp.RL.Agents;
using DeepSharp.RL.Environs;

namespace TorchSharpTest.RLTest.ValueBasedTest
{
    public class QLearningTest : AbstractTest
    {
        public QLearningTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }


        [Fact]
        public void KArmedBanditMain()
        {
            /// Step 1 Create a 4-Armed Bandit
            var kArmedBandit = new KArmedBandit(new[] {0.4, 0.85, 0.75, 0.75}) {Gamma = 0.95f};

            /// Step 2 Create AgentCrossEntropy with 0.7f percentElite as default
            var agent = new QLearning(kArmedBandit);
            Print(kArmedBandit);

            var i = 0;
            var testEpisode = 20;
            var bestReward = 0f;
            while (true)
            {
                i++;
                kArmedBandit.Reset();
                agent.Learn();


                var episode = agent.RunEpisodes(testEpisode);
                var reward = episode.Average(a => a.SumReward.Value);

                bestReward = new[] {bestReward, reward}.Max();
                Print($"{agent} Play:{i:D3}\t {reward}");
                if (bestReward >= 17.5)
                    break;
            }

            var e = agent.RunEpisode();
            var act = e.Steps.Select(a => a.Action);
            Print(string.Join("\r\n", act));
        }


        [Fact]
        public void FrozenlakeMain()
        {
            /// Step 1 Create a 4-Armed Bandit
            var frozenlake = new Frozenlake(new[] {1f, 0.2f, 0.2f}) {Gamma = 0.95f};

            /// Step 2 Create AgentCrossEntropy with 0.7f percentElite as default
            var agent = new QLearning(frozenlake);
            Print(frozenlake);

            var i = 0;
            var testEpisode = 20;
            var bestReward = 0f;

            while (true)
            {
                i++;
                frozenlake.Reset();
                agent.Learn();

                if (i % 100 == 0)
                {
                    var episode = agent.RunEpisodes(testEpisode);
                    var reward = episode.Average(a => a.SumReward.Value);

                    bestReward = new[] {bestReward, reward}.Max();
                    Print($"{agent} Play:{i:D3}\t {reward}");
                    if (bestReward > 0.7)
                        break;
                }
            }

            frozenlake.ChangeToRough();
            var e = agent.RunEpisode();
            var act = e.Steps.Select(a => a.Action);
            Print(string.Join("\r\n", act));
        }
    }
}