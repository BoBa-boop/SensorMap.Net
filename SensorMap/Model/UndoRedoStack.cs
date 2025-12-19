using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using System.Windows;

namespace SensorMap.Model
{
    public class UndoRedoStack:ReactiveObject
    {
        private Stack<ICommandSensors> _undo;
        private Stack<ICommandSensors> _redo;
        private int _undoCount;
        private int _redoCount;

        [Reactive]
        public int UndoCount
        {
            get => _undoCount; 
            set=> this.RaiseAndSetIfChanged(ref _undoCount, value);
        }
        [Reactive]
        public int RedoCount
        {
            get => _redoCount;
            set=> this.RaiseAndSetIfChanged(ref _redoCount, value);
        }
        public UndoRedoStack() 
        {
            Reset();
        }

        private void Reset()
        {
            _undo = new Stack<ICommandSensors>();
            _redo = new Stack<ICommandSensors>();
        }

        public void Do(ICommandSensors command)
        {
            command.Do();
            _undo.Push(command);
            _redo.Clear();

            UndoCount = _undo.Count;
            RedoCount = _redo.Count;
        }
        public void Undo()
        {
            if (_undo.Count > 0)
            {
                ICommandSensors command = _undo.Pop();
                command.Undo();
                _redo.Push(command);

                UndoCount = _undo.Count;
                RedoCount = _redo.Count;
            }
        }
        public void Redo() 
        {
            if( _redo.Count > 0)
            {
                ICommandSensors command = _redo.Pop();
                command.Do();
                _undo.Push(command);

                UndoCount = _undo.Count;
                RedoCount = _redo.Count;
            }
        }
    }
}
