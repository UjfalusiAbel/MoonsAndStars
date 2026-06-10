using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States.Interfaces;
using UnityEngine;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States
{
    public abstract class EnemyState : IEnemyState
    {
        protected GameObject _owner;
        protected EnemyAI _enemyAI;
        public EnemyState(GameObject owner, EnemyAI enemyAI)
        {
            _owner = owner;
            _enemyAI = enemyAI;
        }
        public abstract void Enter();

        public abstract void Exit();

        public abstract void Update();
    }
}