using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public enum StepState { Pending, Running, Success, Failed }

    public record CheckResult(bool Ok, string? Message = null);

    public interface IDbCheck
    {
        string Title { get; }
        Task<CheckResult> RunAsync(string path);
    }
    public class CheckStepVM : ReactiveObject
    {
        private StepState _state;
        private string? _message;

        public CheckStepVM(IDbCheck check) => Check = check;

        public IDbCheck Check { get; }
        public string Title => Check.Title;

        public StepState State
        {
            get => _state;
            set => this.RaiseAndSetIfChanged(ref _state, value);
        }

        public string? Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }
    }
}
