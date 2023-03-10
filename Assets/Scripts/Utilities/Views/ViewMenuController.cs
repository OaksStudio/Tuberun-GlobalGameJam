using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace OAKS.Utilities.Views
{
    [DisallowMultipleComponent]
    public class ViewMenuController : MonoBehaviour
    {
        [SerializeField] private ViewBase InitialView;
        [SerializeField] private Canvas RootCanvas;
        [InlineButton(nameof(GetAllViews), "Get")]
        [SerializeField] private List<ViewBase> Views = new List<ViewBase>();
        [ShowInInspector]
        private Stack<ViewBase> _viewsStack = new Stack<ViewBase>();
        private ViewBase _lastPage;
        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            if (InitialView == null) return;
            PushView(InitialView);
        }

        public void ForceInitialView(ViewBase viewBase)
        {
            InitialView = viewBase;
        }

        private void Init()
        {
            GetAllViews();

        }

        [Button]
        public void PushView(ViewBase view)
        {
            if (IsViewOnTop(view)) return;
            view.OnEnter();

            if (_viewsStack.Count > 0)
            {
                _lastPage = _viewsStack.Peek();
                _lastPage.OnStack();
                if (_lastPage.RemoveOnNewViewPush)
                {
                    PopView();
                }
                else if (_lastPage.ExitOnNewViewPush)
                {
                    _lastPage.OnExit();
                }
            }

            _viewsStack.Push(view);
        }

        [Button]
        public void PushView(string viewId)
        {
            if (!IsViewOnStack(viewId)) return;
            PushView(GetViewById(viewId));
        }

        public void PopView()
        {
            if (_viewsStack.Count == 0) return;

            ViewBase popPage = _viewsStack.Pop();
            popPage.OnExit();

            if (_viewsStack.Count == 0) return;

            ViewBase currentPage = _viewsStack.Peek();

            if (currentPage.ExitOnNewViewPush)
            {
                currentPage.OnEnter();
            }
            else
            {
                currentPage.SetSelected();

                currentPage.OnUnstack();
            }
        }

        private void OnReturn()
        {
            if (!RootCanvas.enabled || !RootCanvas.gameObject.activeInHierarchy
            || _viewsStack.Count == 0) return;

            PopView();
        }

        public void PopAllViews()
        {
            for (int i = 1; i < _viewsStack.Count; i++)
            {
                PopAllViews();
            }
        }

        public bool IsViewOnStack(ViewBase view)
        {
            return _viewsStack.Contains(view);
        }

        public bool IsViewOnTop(ViewBase view)
        {
            return _viewsStack.Count > 0 && _viewsStack.Peek().Equals(view);
        }

        public bool IsViewOnStack(string viewId)
        {
            return Views.Count > 0 && Views.Exists(v => v.ViewId == viewId);
        }

        public bool IsViewOnTop(string viewId)
        {
            return IsViewOnStack(viewId) && _viewsStack.Peek().Equals(GetViewById(viewId));
        }

        public ViewBase GetViewById(string viewId)
        {
            return Views.Find(v => v.ViewId == viewId);
        }

        private void Reset()
        {
            TryGetComponent(out RootCanvas);
            GetAllViews();
        }

        private void GetAllViews()
        {
            Views = GetComponentsInChildren<ViewBase>().ToList();
        }
    }

}
