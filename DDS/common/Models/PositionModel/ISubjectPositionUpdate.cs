using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.common.Models.PositionModel
{
    public interface ISubjectPositionUpdate
    {
        void RegisterPositionUpdate(IObserverPositionUpdate item);
        void UnregisterPositionUpdate(IObserverPositionUpdate item);
    }
}
