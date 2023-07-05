﻿using DeepSharp.RL.Agents;
using DeepSharp.RL.Environs;

namespace DeepSharp.RL.Trainers
{
    public abstract class TrainerCallBack
    {
        protected TrainerCallBack(RLTrainer rltrainer)
        {
            RlTrainer = rltrainer;
        }


        public RLTrainer RlTrainer { protected set; get; }

        public Agent Agent => RlTrainer.Agent;


        public abstract void OnTrainStart();
        public abstract void OnTrainEnd();
        public abstract void OnLearnStart(int epoch);
        public abstract void OnLearnEnd(int epoch, LearnOutcome outcome);
        public abstract void OnValStart(int epoch);
        public abstract void OnValEnd(int epoch, Episode[] episodes);
        public abstract void OnSaveStart();
        public abstract void OnSaveEnd();
    }
}