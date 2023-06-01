﻿using DeepSharp.RL.Policies;

namespace DeepSharp.RL.Models
{
    /// <summary>
    ///     环境
    ///     提供观察 并给与奖励
    /// </summary>
    public abstract class Environ : ObservableObject
    {
        private string _name;
        private Observation _observation = new(torch.empty(0));
        private List<Observation> _observationList = new();
        private Reward _reward = new(0);

        protected Environ(string name)
        {
            _name = name;
        }

        public string Name
        {
            internal set => SetProperty(ref _name, value);
            get => _name;
        }

        public int ActionSpace { protected set; get; }
        public int SampleActionSpace { protected set; get; }
        public int ObservationSpace { protected set; get; }

        public Observation Observation
        {
            set => SetProperty(ref _observation, value);
            get => _observation;
        }

        public Reward Reward
        {
            set => SetProperty(ref _reward, value);
            get => _reward;
        }

        public List<Observation> ObservationList
        {
            internal set => SetProperty(ref _observationList, value);
            get => _observationList;
        }

        public int Life => ObservationList.Count;


        /// <summary>
        ///     恢复初始
        /// </summary>
        public void Reset()
        {
            ObservationList.Clear();
            Observation = new Observation(torch.zeros(ObservationSpace));
            Reward = new Reward(0);
        }


        /// <summary>
        ///     Calculate one reward from one observation
        /// </summary>
        /// <param name="observation">one observation</param>
        /// <returns>one reward</returns>
        public abstract Reward GetReward(Observation observation);

        /// <summary>
        ///     Update Environ Observation according  with one action from Agent
        /// </summary>
        /// <param name="action">Action from Policy</param>
        /// <returns>new observation</returns>
        public abstract Observation UpdateEnviron(Action action);


        /// <summary>
        ///     Get episode by one policy without reset Environ
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="maxPeriod">limit size of a episode</param>
        /// <returns></returns>
        public virtual Episode GetEpisode(IPolicy policy)
        {
            var episode = new Episode();
            var epoch = 0;
            while (StopEpoch(epoch) == false)
            {
                epoch++;
                var action = policy.PredictAction(Observation);
                var obs = Observation = UpdateEnviron(action);
                var reward = GetReward(obs);
                episode.Oars.Add(new Step {Action = action, Observation = obs, Reward = reward});
            }

            episode.SumReward = new Reward(episode.Oars.Sum(a => a.Reward.Value));
            return episode;
        }

        public abstract bool StopEpoch(int epoch);

        /// <summary>
        ///     Get Multi Episodes by one policy.
        /// </summary>
        /// <param name="policy">Agent</param>
        /// <param name="episodesSize">the size of episodes need return</param>
        /// <returns></returns>
        public virtual Episode[] GetMultiEpisodes(IPolicy policy, int episodesSize)
        {
            Reset();

            var episodes = Enumerable.Repeat(0, episodesSize)
                .Select(_ => GetEpisode(policy))
                .ToArray();

            return episodes;
        }


        public override string ToString()
        {
            return $"{Name}\tLife:{Life}";
        }
    }
}