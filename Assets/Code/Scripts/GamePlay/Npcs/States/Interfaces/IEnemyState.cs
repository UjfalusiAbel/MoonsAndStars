using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoonsAndStars.Assets.Code.Scripts.GamePlay.Npcs.States.Interfaces
{
    public interface IEnemyState
    {
        public void Enter();
        public void Update();
        public void Exit();
    }
}