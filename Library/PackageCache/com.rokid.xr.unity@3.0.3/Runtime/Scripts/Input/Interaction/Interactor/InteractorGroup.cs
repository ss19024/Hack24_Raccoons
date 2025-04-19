
namespace Rokid.UXR.Interaction
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// InteractorGroup coordinates between a set of Interactors to
    /// determine which Interactor(s) should be enabled at a time.
    ///
    /// By default, Interactors are prioritized in list order (first = highest priority).
    /// Interactors can also be prioritized with an optional ICandidateComparer
    /// 交互器组合 在一组交互器之间进行协调，以确定一次应启用哪些交互器。
    /// 默认情况下，交互器按列表顺序排列优先级（第一个 = 最高优先级）。
    /// 还可以使用可选的 ICandidateComparer 对交互器进行优先级排序
    /// </summary>
    public class InteractorGroup : MonoBehaviour, IInteractor
    {
        [SerializeField, Interface(typeof(IInteractor))]
        private List<MonoBehaviour> _interactors;

        protected List<IInteractor> Interactors;

        public bool IsRootDriver { get; set; } = true;

        private IInteractor _candidateInteractor = null;
        private IInteractor _activeInteractor = null;

        [SerializeField, Interface(typeof(ICandidateComparer)), Optional]
        private MonoBehaviour _interactorComparer;

        [SerializeField, Optional]
        private UnityEngine.Object _data = null;
        public object Data { get; protected set; } = null;

        public int MaxIterationsPerFrame = 3;
        protected ICandidateComparer CandidateComparer = null;

        public event Action<InteractorStateChangeArgs> WhenStateChanged = delegate { };
        public event Action WhenPreprocessed = delegate { };
        public event Action WhenProcessed = delegate { };
        public event Action WhenPostprocessed = delegate { };

        protected virtual void Awake()
        {
            Interactors = _interactors.ConvertAll(mono => mono as IInteractor);
            CandidateComparer = _interactorComparer as ICandidateComparer;
        }

        protected virtual void Start()
        {
            foreach (IInteractor interactor in Interactors)
            {
                Assert.IsNotNull(interactor);
            }

            foreach (IInteractor interactor in Interactors)
            {
                interactor.IsRootDriver = false;
            }

            if (_interactorComparer != null)
            {
                Assert.IsNotNull(CandidateComparer);
            }

            if (Data == null)
            {
                _data = this;
                Data = _data;
            }
        }

        public void Preprocess()
        {
            foreach (IInteractor interactor in Interactors)
            {
                interactor.Preprocess();
            }
            WhenPreprocessed();
        }

        public void Process()
        {
            if (_activeInteractor != null)
            {
                _activeInteractor.Process();
            }
            WhenProcessed();
        }

        public void Postprocess()
        {
            foreach (IInteractor interactor in Interactors)
            {
                interactor.Postprocess();
            }

            if (_activeInteractor != null && _activeInteractor.State == InteractorState.Disabled)
            {
                _activeInteractor = null;
            }

            WhenPostprocessed();
        }

        public void ProcessCandidate()
        {
            _candidateInteractor = null;

            foreach (IInteractor interactor in Interactors)
            {
                interactor.ProcessCandidate();

                if (interactor.HasCandidate)
                {
                    if (_candidateInteractor == null ||
                        Compare(_candidateInteractor, interactor) > 0)
                    {
                        _candidateInteractor = interactor;
                    }
                }
            }

            if (_candidateInteractor == null && Interactors.Count > 0)
            {
                _candidateInteractor = Interactors[Interactors.Count - 1];
            }
        }

        public void Enable()
        {
            if (_activeInteractor == null)
            {
                return;
            }
            _activeInteractor.Enable();
        }

        public void Disable()
        {
            foreach (IInteractor interactor in Interactors)
            {
                interactor.Disable();
            }

            State = InteractorState.Disabled;
        }

        public void Hover()
        {
            if (State != InteractorState.Normal)
            {
                return;
            }

            _activeInteractor = _candidateInteractor;
            _activeInteractor.Hover();
            State = InteractorState.Hover;
        }

        public void Unhover()
        {
            if (State != InteractorState.Hover)
            {
                return;
            }

            if (_activeInteractor != null)
            {
                _activeInteractor.Unhover();
            }

            _activeInteractor = null;

            State = InteractorState.Normal;
        }

        public void Select()
        {
            if (State != InteractorState.Hover)
            {
                return;
            }

            _activeInteractor.Select();
            State = InteractorState.Select;
        }

        public void Unselect()
        {
            if (State != InteractorState.Select)
            {
                return;
            }

            if (_activeInteractor != null)
            {
                _activeInteractor.Unselect();
            }

            State = InteractorState.Hover;
        }

        public bool ShouldHover => _activeInteractor != null && _activeInteractor.ShouldHover;
        public bool ShouldUnhover => _activeInteractor == null ||
                                     _activeInteractor.ShouldUnhover ||
                                     _activeInteractor != _candidateInteractor;
        public bool ShouldSelect => _activeInteractor != null && _activeInteractor.ShouldSelect;
        public bool ShouldUnselect => _activeInteractor == null || _activeInteractor.ShouldUnselect;

        private void DisableAllInteractorsExcept(IInteractor enabledInteractor)
        {
            foreach (IInteractor interactor in Interactors)
            {
                if (interactor == enabledInteractor) continue;
                interactor.Disable();
            }
        }

        public int Identifier => _activeInteractor != null
            ? _activeInteractor.Identifier
            : Interactors[Interactors.Count - 1].Identifier;

        public bool HasCandidate => _candidateInteractor != null && _candidateInteractor.HasCandidate;

        public object CandidateProperties => HasCandidate ? _candidateInteractor.CandidateProperties : null;

        public bool HasInteractable => _activeInteractor != null &&
                                       _activeInteractor.HasInteractable;

        public bool HasSelectedInteractable => State == InteractorState.Select &&
                                               _activeInteractor.HasSelectedInteractable;

        private InteractorState _state = InteractorState.Normal;

        public InteractorState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state == value)
                {
                    return;
                }
                InteractorState previousState = _state;
                _state = value;

                WhenStateChanged(new InteractorStateChangeArgs(
                    previousState, _state
                ));
            }
        }

        public virtual void AddInteractor(IInteractor interactor)
        {
            Interactors.Add(interactor);
            interactor.IsRootDriver = false;

            MonoBehaviour interactorMono = interactor as MonoBehaviour;
            if (interactorMono != null)
            {
                _interactors.Add(interactor as MonoBehaviour);
            }
        }

        public virtual void RemoveInteractor(IInteractor interactor)
        {
            if (!Interactors.Remove(interactor))
            {
                return;
            }

            interactor.IsRootDriver = true;
            MonoBehaviour interactorMono = interactor as MonoBehaviour;
            if (interactorMono != null)
            {
                _interactors.Remove(interactor as MonoBehaviour);
            }
        }

        private int Compare(IInteractor a, IInteractor b)
        {
            if (!a.HasCandidate && !b.HasCandidate)
            {
                return -1;
            }

            if (a.HasCandidate && b.HasCandidate)
            {
                if (CandidateComparer == null)
                {
                    return -1;
                }

                int result = CandidateComparer.Compare(a.CandidateProperties, b.CandidateProperties);
                return result > 0 ? 1 : -1;
            }

            return a.HasCandidate ? -1 : 1;
        }

        protected virtual void Update()
        {
            if (!IsRootDriver)
            {
                return;
            }

            Drive();
        }

        public void Drive()
        {
            Preprocess();

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
                    if (_candidateInteractor != null && _activeInteractor != _candidateInteractor)
                    {
                        _activeInteractor = _candidateInteractor;
                        Enable();
                        DisableAllInteractorsExcept(_activeInteractor);
                    }

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
        }

    }
}
