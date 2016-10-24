using System;
using System.Collections.Generic;

namespace Slave
{
    public class FrozenState : State
    {
        public FrozenState(Slave slave) : 
            base(slave)
        {
        }

        public override void Dispatch(string input)
        {
            // TO DO
            throw new NotImplementedException();
        }

        public override void Update(string toUpdate)
        {
            // TO DO
            throw new NotImplementedException();
        }
    }

    public class UnfrozenState : State
    {
        public UnfrozenState(Slave slave)
            : base(slave)
        {
        }

        public override void Dispatch(string input)
        {
            // TO DO

            // Márcio 
            // Se o input vier a null:
            // -> Tentas SlaveObj.ImportObj.Import():
            //    a) null = não há nada para processar retornas
            //    b) não null = iteras a lista retornada processas (cuidado que o process retorna string.empty quando os requisitos n sao preenchidos) e das route 
            // Caso o input n venha a null:
            // -> fazes process do input e das route
            throw new NotImplementedException();
        }

        public override void Update(string toUpdate)
        {
            // TO DO
            throw new NotImplementedException();
        }
    }

}
