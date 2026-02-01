using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using System.Windows;

namespace SensorMap.Model
{
    public class UndoRedoStack:ReactiveObject
    {
        private Stack<IUndoRedoCommand> _undo = new Stack<IUndoRedoCommand>();
        private Stack<IUndoRedoCommand> _redo = new Stack<IUndoRedoCommand>();
        private bool _canUndo;
        private bool _canRedo;

        [Reactive]public bool CanUndo
        {
            get { return _undo.Count > 0; }
            set
            {
                this.RaiseAndSetIfChanged(ref _canUndo, value);
            }
        }
        [Reactive]public bool CanRedo
        {
            get { return _redo.Count > 0; }
            set
            {
                this.RaiseAndSetIfChanged(ref _canRedo, value);
            }
        }
        public UndoRedoStack() 
        {
            Reset();
        }

        private void Reset()
        {
            _undo.Clear();
            _redo.Clear();

            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }

        public void Do(IUndoRedoCommand command)
        {
            command.Do();
            _undo.Push(command);
            _redo.Clear();

            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }
        public void Undo()
        {
            if (!CanUndo) return;

            IUndoRedoCommand command = _undo.Pop();
            command.Undo();
            _redo.Push(command);

            this.RaisePropertyChanged(nameof(CanUndo)); 
            this.RaisePropertyChanged(nameof(CanRedo));
        }
        public void Redo() 
        {
            if (!CanRedo) return;
            IUndoRedoCommand command = _redo.Pop();
            command.Do();
            _undo.Push(command);

            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }
    }
}
