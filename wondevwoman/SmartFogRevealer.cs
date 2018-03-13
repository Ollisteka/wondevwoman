using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG.WondevWoman
{
    public class SmartFogRevealer : IFogRevealer
    {
        //множество позиций врага
        //для каждой возможной - поставить врагов в p
        //применить prevAct к prevState
        //для каждого хода врага - применить его, и сравнить с текущей картой
        //не совпавшие - выкинуть
        //из оставшихся - залогировать кол-во, выбрать 1
        public void ConsiderStateBeforeMove(State state, Countdown countdown)
        {
            throw new NotImplementedException();
        }

        public void RegisterAction(IGameAction action)
        {
            throw new NotImplementedException();
        }
    }
}
