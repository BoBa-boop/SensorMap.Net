using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.IdentityModel.Tokens;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Behaviors;
using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MechSensorsVM:ReactiveObject
    {
        private Mechanism _mechanism;
        private SensorAssignments _sensor;
        private ITempImage _imageControl;
        private bool isEditMode;
        private bool _hasChanges;
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
                this.RaiseAndSetIfChanged(ref _sensor, value);
            }
        }
        [Reactive]
        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                 _hasChanges = value;
                this.RaiseAndSetIfChanged(ref _hasChanges, value);
            }
        }
        [Reactive] public ObservableCollection<Sensor> SensorList{get;set;}
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        public MechSensorsVM(ITempImage imageControl,Mechanism currentMech,ObservableCollection<Sensor> sensorsList)
        {
            Mechanism = (Mechanism)currentMech.Clone();
            SensorList = new (sensorsList);
            if(Mechanism!=null && Mechanism.SensorsAssig!=null)
                Mechanism.SensorsAssig.RemoveMany(Mechanism.SensorsAssig.Where(x => x.ToDelete == true).ToList());
            _imageControl = imageControl;
            AddImage = new RelayCommand<SensorAssignments>((sens) =>
            {
                byte[] tempImage = _imageControl.OpenImageDialog();
                if (!tempImage.IsNullOrEmpty())
                {
                    SelectedSensor.Image = tempImage;
                    SelectedSensor.IsModified = true;
                    HasChanges = true;
                }
                
            });
            ShowPreviewImage = new RelayCommand<object>((image) =>
            {
                if (image is byte[] img && img != null)
                {
                    var browser = new CustomImageBrowser(_imageControl.CreateImageFromBytes(img)) { Title = "Просмотр" };
                    browser.ShowDialog();
                }
            });
            
        }

        public ICommand AddImage { get; set; }
        public ICommand ShowPreviewImage { get; set; }
    }
}
