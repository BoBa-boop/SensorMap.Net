using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.ViewModel
{
    public class MechSensorsVM:ReactiveObject
    {
        private Mechanism _mechanism;
        private SensorAssignments _sensor;
        private readonly IAppDbContextFactory _appDbContextFactory;

       
        [Reactive]public Mechanism Mechanism
        {
            get { return _mechanism; }
            set { _mechanism = value; this.RaiseAndSetIfChanged(ref _mechanism, value); }
        }
        
        [Reactive]public SensorAssignments SelectedSensor
        {
            get { return _sensor; }
            set 
            {
                _sensor = value;
                this.RaiseAndSetIfChanged(ref _sensor, value);
            }
        }

        public MechSensorsVM(IAppDbContextFactory appDbContextFactory,Mechanism currentMech)
        {
            _appDbContextFactory = appDbContextFactory;
            Mechanism = currentMech;
        }
    }
}
