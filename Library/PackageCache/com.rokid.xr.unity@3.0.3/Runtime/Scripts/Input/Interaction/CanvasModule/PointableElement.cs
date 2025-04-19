using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rokid.UXR.Interaction
{
    public class PointableElement : MonoBehaviour, IPointableElement
    {
        [HideInInspector, SerializeField]
        private bool _transferOnSecondSelection;

        [HideInInspector, SerializeField]
        private bool _addNewPointsToFront = false;

        [HideInInspector, SerializeField, Interface(typeof(IPointableElement)), Optional]
        private MonoBehaviour _forwardElement;

        public IPointableElement ForwardElement { get; private set; }

        #region Properties
        public bool TransferOnSecondSelection
        {
            get
            {
                return _transferOnSecondSelection;
            }
            set
            {
                _transferOnSecondSelection = value;
            }
        }

        public bool AddNewPointsToFront
        {
            get
            {
                return _addNewPointsToFront;
            }
            set
            {
                _addNewPointsToFront = value;
            }
        }
        #endregion

        public event Action<PointerEvent> WhenPointerEventRaised = delegate { };

        public List<Pose> Points => _points;
        public int PointsCount => _points.Count;

        public List<Pose> SelectingPoints => _selectingPoints;
        public int SelectingPointsCount => _selectingPoints.Count;

        protected List<Pose> _points;
        protected List<int> _pointIds;

        protected List<Pose> _selectingPoints;
        protected List<int> _selectingPointIds;


        protected virtual void Awake()
        {
            ForwardElement = _forwardElement as IPointableElement;
            _points = new List<Pose>();
            _pointIds = new List<int>();

            _selectingPoints = new List<Pose>();
            _selectingPointIds = new List<int>();
        }

        protected virtual void Start()
        {
            if (_forwardElement)
            {
                Assert.IsNotNull(ForwardElement);
            }
        }

        protected virtual void OnEnable()
        {
            if (ForwardElement != null)
            {
                ForwardElement.WhenPointerEventRaised += HandlePointerEventRaised;
            }
        }

        protected virtual void OnDisable()
        {
            while (_selectingPoints.Count > 0)
            {
                Cancel(new PointerEvent(_selectingPointIds[0], PointerEventType.Cancel, _selectingPoints[0]));
            }

            if (ForwardElement != null)
            {
                ForwardElement.WhenPointerEventRaised -= HandlePointerEventRaised;
            }
        }

        private void HandlePointerEventRaised(PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Cancel)
            {
                ProcessPointerEvent(evt);
            }
        }

        public virtual void ProcessPointerEvent(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Hover:
                    Hover(evt);
                    break;
                case PointerEventType.Unhover:
                    Unhover(evt);
                    break;
                case PointerEventType.Move:
                    Move(evt);
                    break;
                case PointerEventType.Select:
                    Select(evt);
                    break;
                case PointerEventType.Unselect:
                    Unselect(evt);
                    break;
                case PointerEventType.Cancel:
                    Cancel(evt);
                    break;
            }
        }

        private void Hover(PointerEvent evt)
        {
            if (_addNewPointsToFront)
            {
                _pointIds.Insert(0, evt.Identifier);
                _points.Insert(0, evt.Pose);
            }
            else
            {
                _pointIds.Add(evt.Identifier);
                _points.Add(evt.Pose);
            }

            PointableElementUpdated(evt);
        }

        private void Move(PointerEvent evt)
        {
            int index = _pointIds.IndexOf(evt.Identifier);
            if (index == -1)
            {
                return;
            }
            _points[index] = evt.Pose;

            index = _selectingPointIds.IndexOf(evt.Identifier);
            if (index != -1)
            {
                _selectingPoints[index] = evt.Pose;
            }

            PointableElementUpdated(evt);
        }

        private void Unhover(PointerEvent evt)
        {
            int index = _pointIds.IndexOf(evt.Identifier);
            if (index == -1)
            {
                return;
            }

            _pointIds.RemoveAt(index);
            _points.RemoveAt(index);

            PointableElementUpdated(evt);
        }

        private void Select(PointerEvent evt)
        {
            if (_selectingPoints.Count == 1 && _transferOnSecondSelection)
            {
                Cancel(new PointerEvent(_selectingPointIds[0], PointerEventType.Cancel, _selectingPoints[0]));
            }

            if (_addNewPointsToFront)
            {
                _selectingPointIds.Insert(0, evt.Identifier);
                _selectingPoints.Insert(0, evt.Pose);
            }
            else
            {
                _selectingPointIds.Add(evt.Identifier);
                _selectingPoints.Add(evt.Pose);
            }

            PointableElementUpdated(evt);
        }

        private void Unselect(PointerEvent evt)
        {
            int index = _selectingPointIds.IndexOf(evt.Identifier);
            if (index == -1)
            {
                return;
            }

            _selectingPointIds.RemoveAt(index);
            _selectingPoints.RemoveAt(index);

            PointableElementUpdated(evt);
        }

        private void Cancel(PointerEvent evt)
        {
            int index = _selectingPointIds.IndexOf(evt.Identifier);
            if (index != -1)
            {
                _selectingPointIds.RemoveAt(index);
                _selectingPoints.RemoveAt(index);
            }

            index = _pointIds.IndexOf(evt.Identifier);
            if (index != -1)
            {
                _pointIds.RemoveAt(index);
                _points.RemoveAt(index);
            }
            else
            {
                return;
            }

            PointableElementUpdated(evt);
        }

        protected virtual void PointableElementUpdated(PointerEvent evt)
        {
            if (ForwardElement != null)
            {
                ForwardElement.ProcessPointerEvent(evt);
            }
            // RKLog.Debug($"====PointableElement====: PointableElementUpdated {ForwardElement == null} {gameObject.name} {evt.Pose.position}");
            WhenPointerEventRaised.Invoke(evt);
        }
    }
}
