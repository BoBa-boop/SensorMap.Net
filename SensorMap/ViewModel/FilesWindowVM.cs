using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace SensorMap.ViewModel
{
    public class FilesWindowVM:ReactiveObject
    {
        private readonly ITempImage _tempImage;
        private readonly IFileManagment _fileManagment;
        private bool isEditMode;
        private bool _hasChanges;

        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        [Reactive]public ObservableCollection<HelpfulFile> Files {  get; set; }
        [Reactive] public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                _hasChanges = value;
                this.RaiseAndSetIfChanged(ref _hasChanges, value);
            }
        }
        public FilesWindowVM(IFileManagment fileManagment,ITempImage tempImage,IEnumerable<HelpfulFile> files,object sender)
        {
            _tempImage = tempImage;
            _fileManagment = fileManagment;
            Files = new(files);
            OpenFile = new RelayCommand<HelpfulFile>((file) =>
            {
                if (file == null) return;
                if (!string.IsNullOrEmpty(file.NameFile))
                {
                    _fileManagment.OpenFileInExplorer(file.NameFile);
                }
            }, (file) =>
            {
                if (file == null) return false;

                return true;
            });
            AddFiles = new RelayCommand(() =>
            {
                fileManagment.AddHelpfulFile(tempImage, sender, true);///не обновляет источник
                HasChanges = true;
            },()=>
            {
                if (IsEditMode) return true;
                
                return false;
            });
            RemoveFiles = new RelayCommand<HelpfulFile>((file) =>
            {
                try
                {
                    Files.Remove(file);///не обновляет источник
                    HasChanges = true;
                    Growl.Success(new GrowlInfo
                    {
                        Message = "Путь к файлам удален.",
                        CancelStr = "Ignore",
                        ShowDateTime = false,
                        WaitTime = 2
                    });
                }
                catch (Exception ex)
                {
                    Growl.Error("Ошибка при удаление путей!");
                }
            }, (s) =>
            {
                if (IsEditMode) return true;

                return false;
            });
        }

        public ICommand AddFiles { get; }
        public ICommand RemoveFiles { get; }
        public ICommand OpenFile { get; }
    }
}
