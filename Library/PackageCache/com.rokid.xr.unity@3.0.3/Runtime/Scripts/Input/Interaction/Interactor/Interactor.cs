using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Interactor provides a base template for any kind of interaction.
    /// Interactions can be wholly defined by three things: the concrete Interactor,
    /// the concrete Interactable, and the logic governing their coordination.
    ///
    /// Subclasses are responsible for implementing that coordination logic via template
    /// methods that operate on the concrete interactor and interactable classes.

    /// Interactor 为任何类型的交互提供了一个基本模板。
    /// 交互可以完全由三件事定义：具体的 Interactor、具体的 Interactable 以及控制它们协调的逻辑。
    /// 子类负责通过在具体交互器和可交互类上运行的模板方法来实现协调逻辑。
    /// </summary>
    public abstract class Interactor<TInteractor, TInteractable> : MonoBehaviour, IInteractor
                                    where TInteractor : Interactor<TInteractor, TInteractable>
                                    where TInteractable : Interactable<TInteractor, TInteractable>
    {
        [HideInInspector, SerializeField, Interface(typeof(IActiveState)), Optional]
        private MonoBehaviour _activeState;
        private IActiveState ActiveState = null;

        [SerializeField, Interface(typeof(IGameObjectFilter)), Optional]
        private List<MonoBehaviour> _interactableFilters = new List<MonoBehaviour>();
        private List<IGameObjectFilter> InteractableFilters = null;

        private Func<TInteractable> _computeCandidateOverride;
        private bool _clearComputeCandidateOverrideOnSelect = false;
        private Func<bool> _computeShouldSelectOverride;
        private bool _clearComputeShouldSelectOverrideOnSelect = false;
        private Func<bool> _computeShouldUnselectOverride;
        private bool _clearComputeShouldUnselectOverrideOnUnselect;

        protected virtual void DoPreprocess() { }
        protected virtual void DoNormalUpdate() { }
        protected virtual void DoHoverUpdate() { }
        protected virtual void DoSelectUpdate() { }

        public virtual bool ShouldHover
        {
            get
            {
                if (State != InteractorState.Normal)
                {
                    return false;
                }

                return HasCandidate || ComputeShouldSelect();
            }
        }

        public virtual bool ShouldUnhover
        {
            get
            {
                if (State != InteractorState.Hover)
                {
                    return false;
                }

                return _interactable != _candidate || _candidate == null;
            }
        }

        public bool ShouldSelect
        {
            set { }
            get
            {
                if (State != InteractorState.Hover)
                {
                    return false;
                }

                if (_computeShouldSelectOverride != null)
                {
                    return _computeShouldSelectOverride.Invoke();
                }

                return _candidate == _interactable && ComputeShouldSelect();
            }
        }

        public bool ShouldUnselect
        {
            get
            {
                if (State != InteractorState.Select)
                {
                    return false;
                }

                if (_computeShouldUnselectOverride != null)
                {
                    return _computeShouldUnselectOverride.Invoke();
                }

                return ComputeShouldUnselect();
            }
        }

        protected virtual bool ComputeShouldSelect()
        {
            return QueuedSelect;
        }

        protected virtual bool ComputeShouldUnselect()
        {
            return QueuedUnselect;
        }
        [SerializeField]
        private InteractorState _state = InteractorState.Normal;
        public event Action<InteractorStateChangeArgs> WhenStateChanged = delegate { };
        public event Action WhenPreprocessed = delegate { };
        public event Action WhenProcessed = delegate { };
        public event Action WhenPostprocessed = delegate { };

        private ISelector _selector = null;

        [SerializeField]
        private int _maxIterationsPerFrame = 3;
        public int MaxIterationsPerFrame
        {
            get
            {
                return _maxIterationsPerFrame;
            }
            set
            {
                _maxIterationsPerFrame = value;
            }
        }

        protected ISelector Selector
        {
            get
            {
                return _selector;
            }

            set
            {
                if (value != _selector)
                {
                    if (_selector != null)
                    {
                        _selector.WhenSelected -= HandleSelected;
                        _selector.WhenUnselected -= HandleUnselected;
                    }
                }

                _selector = value;
                if (_selector != null)
                {
                    _selector.WhenSelected += HandleSelected;
                    _selector.WhenUnselected += HandleUnselected;
                }
            }
        }

        private Queue<bool> _selectorQueue = new Queue<bool>();
        private Queue<bool> _unSelectorQueue = new Queue<bool>();
        private bool QueuedSelect => _selectorQueue.Count > 0 && _selectorQueue.Peek();
        private bool QueuedUnselect => _unSelectorQueue.Count > 0 && _unSelectorQueue.Peek();

        /// <summary>
        /// 这里处理交互器状态的变化
        /// </summary>
        /// <value></value>
        public InteractorState State
        {
            get
            {
                return _state;
            }
            protected set
            {
                if (_state == value)
                {
                    return;
                }
                InteractorState previousState = _state;
                _state = value;

                WhenStateChanged(new InteractorStateChangeArgs(previousState, _state));
            }
        }

        protected TInteractable _candidate;
        protected TInteractable _interactable;
        protected TInteractable _selectedInteractable;

        public virtual object CandidateProperties
        {
            get
            {
                return null;
            }
        }

        public TInteractable Candidate => _candidate;
        public TInteractable Interactable => _interactable;
        public TInteractable SelectedInteractable => _selectedInteractable;

        public bool HasCandidate => _candidate != null;
        public bool HasInteractable => _interactable != null;
        public bool HasSelectedInteractable => _selectedInteractable != null;

        private MultiAction<TInteractable> _whenInteractableSet = new MultiAction<TInteractable>();
        private MultiAction<TInteractable> _whenInteractableUnset = new MultiAction<TInteractable>();
        private MultiAction<TInteractable> _whenInteractableSelected = new MultiAction<TInteractable>();
        private MultiAction<TInteractable> _whenInteractableUnselected = new MultiAction<TInteractable>();
        public MAction<TInteractable> WhenInteractableSet => _whenInteractableSet;
        public MAction<TInteractable> WhenInteractableUnset => _whenInteractableUnset;
        public MAction<TInteractable> WhenInteractableSelected => _whenInteractableSelected;
        public MAction<TInteractable> WhenInteractableUnselected => _whenInteractableUnselected;

        protected virtual void InteractableSet(TInteractable interactable)
        {
            _whenInteractableSet.Invoke(interactable);
        }

        protected virtual void InteractableUnset(TInteractable interactable)
        {
            _whenInteractableUnset.Invoke(interactable);
        }

        protected virtual void InteractableSelected(TInteractable interactable)
        {
            _whenInteractableSelected.Invoke(interactable);
        }

        protected virtual void InteractableUnselected(TInteractable interactable)
        {
            _whenInteractableUnselected.Invoke(interactable);
        }

        protected virtual void DoPostprocess() { }

        private UniqueIdentifier _identifier;
        public int Identifier => _identifier.ID;

        [HideInInspector, SerializeField, Optional]
        private UnityEngine.Object _data = null;
        public object Data { get; protected set; } = null;


        protected virtual void Awake()
        {
            _identifier = UniqueIdentifier.Generate();
            ActiveState = _activeState as IActiveState;
            InteractableFilters =
                _interactableFilters.ConvertAll(mono => mono as IGameObjectFilter);
        }

        protected virtual void Start()
        {
            foreach (IGameObjectFilter filter in InteractableFilters)
            {
                Assert.IsNotNull(filter);
            }

            if (Data == null)
            {
                _data = this;
                Data = _data;
            }
        }

        protected virtual void OnEnable()
        {
            _selectorQueue.Clear();
            _unSelectorQueue.Clear();
            if (_selector != null)
            {
                _selector.WhenSelected += HandleSelected;
                _selector.WhenUnselected += HandleUnselected;
            }
        }

        protected virtual void OnDisable()
        {
            if (_selector != null)
            {
                _selector.WhenSelected -= HandleSelected;
                _selector.WhenUnselected -= HandleUnselected;
            }
            Disable();
        }

        protected virtual void OnDestroy()
        {
            UniqueIdentifier.Release(_identifier);
        }

        public virtual void SetComputeCandidateOverride(Func<TInteractable> computeCandidate,
            bool shouldClearOverrideOnSelect = true)
        {
            _computeCandidateOverride = computeCandidate;
            _clearComputeCandidateOverrideOnSelect = shouldClearOverrideOnSelect;
        }

        public virtual void ClearComputeCandidateOverride()
        {
            _computeCandidateOverride = null;
            _clearComputeCandidateOverrideOnSelect = false;
        }

        public virtual void SetComputeShouldSelectOverride(Func<bool> computeShouldSelect,
            bool clearOverrideOnSelect = true)
        {
            _computeShouldSelectOverride = computeShouldSelect;
            _clearComputeShouldSelectOverrideOnSelect = clearOverrideOnSelect;
        }

        public virtual void ClearComputeShouldSelectOverride()
        {
            _computeShouldSelectOverride = null;
            _clearComputeShouldSelectOverrideOnSelect = false;
        }

        public virtual void SetComputeShouldUnselectOverride(Func<bool> computeShouldUnselect,
            bool clearOverrideOnUnselect = true)
        {
            _computeShouldUnselectOverride = computeShouldUnselect;
            _clearComputeShouldUnselectOverrideOnUnselect = clearOverrideOnUnselect;
        }

        public virtual void ClearComputeShouldUnselectOverride()
        {
            _computeShouldUnselectOverride = null;
            _clearComputeShouldUnselectOverrideOnUnselect = false;
        }

        public void Preprocess()
        {
            DoPreprocess();
            if (!UpdateActiveState())
            {
                Disable();
            }
            WhenPreprocessed();
        }

        public void Process()
        {
            switch (State)
            {
                case InteractorState.Normal:
                    DoNormalUpdate();
                    break;
                case InteractorState.Hover:
                    DoHoverUpdate();
                    break;
                case InteractorState.Select:
                    DoSelectUpdate();
                    break;
            }
            WhenProcessed();
        }

        public void Postprocess()
        {
            _selectorQueue.Clear();
            _unSelectorQueue.Clear();
            DoPostprocess();
            WhenPostprocessed();
        }

        /// <summary>
        /// 处理候选者,获取相应的候选者
        /// </summary>
        public virtual void ProcessCandidate()
        {
            _candidate = null;
            if (!UpdateActiveState())
            {
                return;
            }

            if (_computeCandidateOverride != null)
            {
                _candidate = _computeCandidateOverride.Invoke();
            }
            else
            {
                _candidate = ComputeCandidate();
            }
        }

        public void InteractableChangesUpdate()
        {
            if (_selectedInteractable != null &&
                !_selectedInteractable.HasSelectingInteractor(this as TInteractor))
            {
                UnselectInteractable();
            }

            if (_interactable != null &&
                !_interactable.HasInteractor(this as TInteractor))
            {
                UnsetInteractable();
            }
        }

        public void Hover()
        {
            if (State != InteractorState.Normal)
            {
                return;
            }

            SetInteractable(_candidate);
            State = InteractorState.Hover;
        }

        public void Unhover()
        {
            if (State != InteractorState.Hover)
            {
                return;
            }

            UnsetInteractable();
            State = InteractorState.Normal;
        }


        public virtual void Select()
        {
            if (State != InteractorState.Hover)
            {
                return;
            }

            if (_clearComputeCandidateOverrideOnSelect)
            {
                ClearComputeCandidateOverride();
            }

            if (_clearComputeShouldSelectOverrideOnSelect)
            {
                ClearComputeShouldSelectOverride();
            }

            while (QueuedSelect)
            {
                _selectorQueue.Dequeue();
            }

            if (Interactable != null)
            {
                SelectInteractable(Interactable);
            }

            State = InteractorState.Select;
        }

        public virtual void Unselect()
        {
            if (State != InteractorState.Select)
            {
                return;
            }
            if (_clearComputeShouldUnselectOverrideOnUnselect)
            {
                ClearComputeShouldUnselectOverride();
            }
            while (QueuedUnselect)
            {
                _unSelectorQueue.Dequeue();
            }
            UnselectInteractable();

            State = InteractorState.Hover;
        }

        // Returns the best interactable for selection or null
        protected abstract TInteractable ComputeCandidate();

        public virtual bool CanSelect(TInteractable interactable)
        {
            if (InteractableFilters == null)
            {
                return true;
            }

            foreach (IGameObjectFilter interactableFilter in InteractableFilters)
            {
                if (!interactableFilter.Filter(interactable.gameObject))
                {
                    return false;
                }
            }

            return true;
        }

        private void SetInteractable(TInteractable interactable)
        {
            if (_interactable == interactable)
            {
                return;
            }
            UnsetInteractable();
            _interactable = interactable;
            interactable.AddInteractor(this as TInteractor);
            InteractableSet(interactable);
        }
        private void UnsetInteractable()
        {
            TInteractable interactable = _interactable;
            if (interactable == null)
            {
                return;
            }
            _interactable = null;
            interactable.RemoveInteractor(this as TInteractor);
            InteractableUnset(interactable);
        }

        private void SelectInteractable(TInteractable interactable)
        {
            Unselect();
            _selectedInteractable = interactable;
            interactable.AddSelectingInteractor(this as TInteractor);
            InteractableSelected(interactable);
        }

        private void UnselectInteractable()
        {
            TInteractable interactable = _selectedInteractable;

            if (interactable == null)
            {
                return;
            }

            _selectedInteractable = null;
            interactable.RemoveSelectingInteractor(this as TInteractor);
            InteractableUnselected(interactable);
        }

        public void Enable()
        {
            if (!UpdateActiveState())
            {
                return;
            }

            if (State == InteractorState.Disabled)
            {
                State = InteractorState.Normal;
                HandleEnabled();
            }
        }

        public void Disable()
        {
            if (State == InteractorState.Disabled)
            {
                return;
            }

            HandleDisabled();

            if (State == InteractorState.Select)
            {
                UnselectInteractable();
                State = InteractorState.Hover;
            }

            if (State == InteractorState.Hover)
            {
                UnsetInteractable();
                State = InteractorState.Normal;
            }

            if (State == InteractorState.Normal)
            {
                State = InteractorState.Disabled;
            }
        }

        protected virtual void HandleEnabled() { }
        protected virtual void HandleDisabled() { }

        protected virtual void HandleSelected()
        {
            _selectorQueue.Enqueue(true);
        }

        protected virtual void HandleUnselected()
        {
            _unSelectorQueue.Enqueue(true);
        }

        private bool UpdateActiveState()
        {
            if (ActiveState == null || ActiveState.Active)
            {
                return true;
            }
            return false;
        }

        public bool IsRootDriver { get; set; } = true;

        protected virtual void Update()
        {
            if (!IsRootDriver)
            {
                return;
            }

            Drive();
        }

        public virtual void Drive()
        {
            Preprocess();

            if (!UpdateActiveState())
            {
                Disable();
                Postprocess();
                return;
            }

            Enable();

            InteractorState previousState = State;
            for (int i = 0; i < MaxIterationsPerFrame; i++)
            {
                if (State == InteractorState.Normal ||
                    (State == InteractorState.Hover && previousState != InteractorState.Normal))
                {
                    ProcessCandidate();
                }
                previousState = State;

                Process();

                if (State == InteractorState.Disabled)
                {
                    break;
                }

                if (State == InteractorState.Normal)
                {
                    // RKLog.Debug($"Interactor Process Hover: {ShouldHover}");
                    if (ShouldHover)
                    {
                        Hover();
                        continue;
                    }
                    break;
                }

                if (State == InteractorState.Hover)
                {
                    if (ShouldSelect)
                    {
                        Select();
                        continue;
                    }
                    if (ShouldUnhover)
                    {
                        Unhover();
                        continue;
                    }
                    break;
                }

                if (State == InteractorState.Select)
                {
                    if (ShouldUnselect)
                    {
                        Unselect();
                        continue;
                    }
                    break;
                }
            }

            Postprocess();

            // RKLog.Debug($"====Interaction==== Interactor State: {gameObject.name} {_state}");
        }
    }

}
