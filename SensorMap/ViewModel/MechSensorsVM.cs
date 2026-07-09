using CommunityToolkit.Mvvm.Input;
using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MechSensorsVM : ReactiveObject
    {
        private Mechanism _mechanism;
        private MapObject _selectedMapObject;
        private ITempImage _imageControl;
        private bool isEditMode;
        private bool _hasChanges;

        [Reactive] public Mechanism Mechanism
        {
            get { return _mechanism; }
            set { _mechanism = value; this.RaiseAndSetIfChanged(ref _mechanism, value); }
        }

        /// <summary>
        /// Выбранный объект на карте (датчик или устройство)
        /// </summary>
        [Reactive]
        public MapObject SelectedMapObject
        {
            get { return _selectedMapObject; }
            set
            {
                if (_selectedMapObject != null)
                {
                    var changes = Observable.Merge(
                        this.WhenAnyValue(x => x._selectedMapObject.Description)
                    );

                    changes.Subscribe(_ =>
                    {
                        if (_selectedMapObject != null && !_selectedMapObject.IsModified)
                        {
                            _selectedMapObject.IsModified = true;
                            HasChanges = true;
                        }
                    });
                }
                this.RaiseAndSetIfChanged(ref _selectedMapObject, value);
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

        [Reactive] public ObservableCollection<Sensor> SensorList { get; set; }
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }

        public MechSensorsVM(ITempImage imageControl, Mechanism currentMech, ObservableCollection<Sensor> sensorsList)
        {
            Mechanism = (Mechanism)currentMech.Clone();
            SensorList = new(sensorsList);
            if (Mechanism != null && Mechanism.MapObjects != null)
                Mechanism.MapObjects.RemoveMany(Mechanism.MapObjects.Where(x => x.ToDelete == true).ToList());
            _imageControl = imageControl;
            AddImage = new RelayCommand<MapObject>((mapObj) =>
            {
                byte[] tempImage = _imageControl.OpenImageDialog();
                if (!string.IsNullOrEmpty(Convert.ToBase64String(tempImage ?? Array.Empty<byte>())))
                {
                    SelectedMapObject.Image = tempImage;
                    SelectedMapObject.IsModified = true;
                    HasChanges = true;
                }
            });
            ShowPreviewImage = new RelayCommand<object>((image) =>
            {
                if (image is byte[] img && img != null)
                {
                    var browser = new Behaviors.CustomImageBrowser(_imageControl.CreateImageFromBytes(img)) { Title = "Просмотр" };
                    browser.ShowDialog();
                }
            });
        }

        public ICommand AddImage { get; set; }
        public ICommand ShowPreviewImage { get; set; }
    }
}
