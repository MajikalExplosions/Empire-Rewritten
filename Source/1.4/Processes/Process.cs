using UnityEngine;
using Verse;

namespace Empire_Rewritten.Processes
{
    /// <summary>
    ///    An event that triggers at some future point. This class provides information about progress and is saveable.
    /// </summary>
    public abstract class Process : IExposable
    {
        private string _label;
        public string Label
        {
            set => _label = value;
            get => _label;
        }

        private string _tooltip;
        public string Tooltip
        {
            set => _tooltip = value;
            get => _tooltip;
        }

        protected string _iconPath;
        /// <summary>
        ///     An <see cref="Texture2D"/> that is displayed whenever the <see cref="Process"/> is visualized somewhere
        /// </summary>
        public Texture2D Icon => ContentFinder<Texture2D>.Get(_iconPath);

        private ProcessState _state;
        public ProcessState State
        {
            private set => _state = value;
            get => _state;
        }

        private int _progress;
        public int Progress
        {
            private set => _progress = value;
            get => _progress;
        }
        /// <summary>
        ///     A percentage (0f to 1f) that shows how far the <see cref="Process"/> has advanced
        /// </summary>
        public float ProgressPct => _progress / (float)_duration;

        private int _duration;
        public int Duration
        {
            protected set => _duration = value;
            get => _duration;
        }



        /// <summary>
        ///     To be used during Saving/Loading only!
        /// </summary>
        public Process()
        {
            EmpireWorldComp.Current.AddUpdateCall(new WorldCompAction((_) => OnComplete(), Tick, _ShouldDiscard));
        }

        /// <summary>
        ///     Creates a new <see cref="Process"/>
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toolTip"></param>
        /// <param name="duration"></param>
        /// <param name="iconPath"></param>
        public Process(string label, string tooltip, int duration, string iconPath)
        {
            this._label = label;
            this._tooltip = tooltip;
            this._iconPath = iconPath;

            _progress = 0;
            _duration = duration;
            _state = ProcessState.Suspended;

            Initialize();
        }

        /// <summary>
        ///     Marks the <see cref="Process"/> for deletion, effectively cancelling it
        /// </summary>
        public virtual void Cancel()
        {
            _state = ProcessState.Canceled;
        }

        /// <summary>
        ///     Initializes the <see cref="Process"/>, by adding it to the <see cref="UpdateController"/>
        /// </summary>
        private void Initialize()
        {
            if (_state == ProcessState.Running) return;

            EmpireWorldComp.Current.AddUpdateCall(new WorldCompAction((_) => OnComplete(), Tick, _ShouldDiscard));

            _state = ProcessState.Running;
        }

        /// <summary>
        ///     Internal function for the <see cref="UpdateController"/> that informs it that this <see cref="Process"/> can be discarded of
        /// </summary>
        /// <returns>true if the <see cref="Process"/> has finished, or if it was canceled</returns>
        private bool _ShouldDiscard() => ProgressPct >= 1f || _state == ProcessState.Canceled;

        /// <summary>
        ///     Function to be implemented which runs when the <see cref="Process"/> has finished
        /// </summary>
        protected abstract void OnComplete();

        /// <summary>
        ///     Function that does the "work". Triggers the <see cref="Run"/> function when true
        /// </summary>
        /// <returns>true, when the <see cref="Progess"/> reaches completion, false when suspended or otherwise</returns>
        private bool Tick()
        {
            if (_state == ProcessState.Suspended) return false;
            _progress++;
            if (ProgressPct >= 1f) return true;

            return false;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref _label, "label");
            Scribe_Values.Look(ref _tooltip, "tooltip");
            Scribe_Values.Look(ref _iconPath, "iconPath");
            Scribe_Values.Look(ref _state, "state");
            Scribe_Values.Look(ref _progress, "progress");
            Scribe_Values.Look(ref _duration, "duration");
        }

        public enum ProcessState
        {
            Suspended,
            Running,
            Completed,
            Canceled
        }
    }
}
