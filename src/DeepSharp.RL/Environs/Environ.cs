﻿using DeepSharp.RL.Agents;

namespace DeepSharp.RL.Environs
{
    /// <summary>
    ///     环境
    ///     提供观察 并给与奖励
    /// </summary>
    public abstract class Environ<T1, T2> : ObservableObject
        where T1 : Space
        where T2 : Space
    {
        private string _name;
        private Observation? _observation;
        private List<Observation> _observationList = new();
        private Reward _reward = new(0);

        protected Environ(string name, DeviceType deviceType = DeviceType.CUDA)
        {
            _name = name;
            Device = new torch.Device(deviceType);
        }


        public string Name
        {
            internal set => SetProperty(ref _name, value);
            get => _name;
        }

        public torch.Device Device { set; get; }
        public T1? ActionSpace { protected set; get; }
        public T2? ObservationSpace { protected set; get; }
        public float Gamma { set; get; } = 0.9f;


        /// <summary>
        ///     Observation Current
        /// </summary>
        public Observation? Observation
        {
            set => SetProperty(ref _observation, value);
            get => _observation;
        }

        /// <summary>
        ///     Reward Current
        /// </summary>
        public Reward Reward
        {
            set => SetProperty(ref _reward, value);
            get => _reward;
        }

        /// <summary>
        ///     Observation Temp List
        /// </summary>
        public List<Observation> ObservationList
        {
            internal set => SetProperty(ref _observationList, value);
            get => _observationList;
        }

        public int Life => ObservationList.Count;


        /// <summary>
        ///     恢复初始
        /// </summary>
        public virtual Observation Reset()
        {
            ObservationList.Clear();
            Observation = new Observation(ObservationSpace!.Generate());
            Reward = new Reward(0);
            return Observation;
        }

        /// <summary>
        ///     执行单步
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public virtual Step Step(int epoch)
        {
            var act = Sample();
            var observation = Update(act);
            var reward = GetReward(observation);
            var complete = IsComplete(epoch);
            return new Step(act, observation, reward, complete);
        }

        /// <summary>
        ///     随机单步
        /// </summary>
        /// <returns></returns>
        public virtual Act Sample()
        {
            var sampleAction = ActionSpace!.Sample();
            return new Act(sampleAction);
        }


        /// <summary>
        ///     Update Environ Observation according  with one action from Agent
        /// </summary>
        /// <param name="act">Action from Policy</param>
        /// <returns>new observation</returns>
        public abstract Observation Update(Act act);


        /// <summary>
        ///     Cal Reward from Observation
        ///     从观察获取奖励的计算方法
        /// </summary>
        /// <param name="observation">one observation</param>
        /// <returns>one reward</returns>
        public abstract Reward GetReward(Observation observation);

        /// <summary>
        ///     Discount Reward
        ///     长期奖励折扣
        /// </summary>
        /// <param name="episode"></param>
        /// <param name="gamma"></param>
        /// <returns></returns>
        public abstract float DiscountReward(Episode episode, float gamma);

        /// <summary>
        ///     Check Environ is Complete
        ///     判断探索是否结束
        /// </summary>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public abstract bool IsComplete(int epoch);


        /// <summary>
        ///     Get Multi Episodes by one policy.
        /// </summary>
        /// <param name="policy">Agent</param>
        /// <param name="episodesSize">the size of episodes need return</param>
        /// <returns></returns>
        public virtual Episode[] GetMultiEpisodes(Agent policy, int episodesSize)
        {
            var episodes = Enumerable.Repeat(0, episodesSize)
                .Select(_ => GetEpisode(policy))
                .ToArray();

            return episodes;
        }

        /// <summary>
        ///     Get episode by one policy without reset Environ
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="maxPeriod">limit size of a episode</param>
        /// <returns></returns>
        public virtual Episode GetEpisode(Agent policy)
        {
            Reset();

            var episode = new Episode();
            var epoch = 1;
            while (IsComplete(epoch) == false)
            {
                epoch++;
                var action = policy.PredictAction(Observation!).To(Device);
                var obs = Update(action).To(Device);
                Observation = obs;
                Reward = GetReward(Observation);
                episode.Steps.Add(new Step(action, Observation, Reward));
            }

            var sumReward = episode.Steps.Sum(a => a.Reward.Value) * DiscountReward(episode, Gamma);
            episode.SumReward = new Reward(sumReward);
            return episode;
        }


        public override string ToString()
        {
            return $"{Name}\tLife:{Life}";
        }
    }
}