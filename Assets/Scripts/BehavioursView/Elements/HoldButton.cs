using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Image ReleasedIcon;
    [SerializeField] private Image PressedIcon;
    public UnityEvent OnPressEvent, OnReleaseEvent, OnHoldEvent, OnUnHoldEvent, OnCompleteEvent;
    [SerializeField] private bool _changeFillers = true;
    [SerializeField] private List<Image> ReleasedFillers = new List<Image>();
    [SerializeField] private List<Image> PressedFillers = new List<Image>();

    [Header("Config")]
    [SerializeField] private float _addSpeed = 1;
    [SerializeField] private float _removeSpeed = 2;

    [SerializeField] private bool _stopOncompleted;

    [Header("Hidden")]
    [SerializeField] private ControlMap.Map _currentMap;
    [SerializeField, ReadOnly] private float _currentProgress;

    public Action OnComplete;

    private bool _completed, _active;

    private void Awake()
    {
        Setup(_currentMap);
    }

    [Button]
    public void Activate(bool value)
    {
        _active = value;
        if (!value)
        {
            Reset();
        }
    }

    public void Setup(ControlMap.Map map)
    {
        _currentMap = map;

        ReleasedIcon.sprite = map.ReleaseIcon;
        PressedIcon.sprite = map.PressedIcon;

        if (_changeFillers)
        {
            ReleasedFillers.ForEach(f => f.sprite = map.ReleaseIcon);
            PressedFillers.ForEach(f => f.sprite = map.PressedIcon);
        }

        ReleasedIcon.enabled = true;
        PressedIcon.enabled = false;

        if (_changeFillers)
        {
            ReleasedFillers.ForEach(f => f.enabled = true);
            PressedFillers.ForEach(f => f.enabled = false);
        }

        Reset();
    }

    public void Reset()
    {
        _currentProgress = 0;
        Progress(_currentProgress);
        _completed = false;
    }

    private void Update()
    {
        if (_completed && _stopOncompleted) return;

        if (Input.GetKey(_currentMap.KeyCode) && _active)
        {
            _currentProgress = Mathf.Clamp01(_currentProgress + _addSpeed * Time.deltaTime);
            OnHoldEvent?.Invoke();
            if (!PressedIcon.enabled)
            {
                ReleasedIcon.enabled = false;
                PressedIcon.enabled = true;
                OnPressEvent?.Invoke();
                if (_changeFillers)
                {
                    ReleasedFillers.ForEach(f => f.enabled = false);
                    PressedFillers.ForEach(f => f.enabled = true);
                }
            }
        }
        else
        {
            _currentProgress = Mathf.Clamp01(_currentProgress - _removeSpeed * Time.deltaTime);
            OnUnHoldEvent?.Invoke();
            if (!ReleasedIcon.enabled)
            {
                ReleasedIcon.enabled = true;
                PressedIcon.enabled = false;
                OnReleaseEvent?.Invoke();
                if (_changeFillers)
                {
                    PressedFillers.ForEach(f => f.enabled = false);
                    ReleasedFillers.ForEach(f => f.enabled = true);
                }
            }
        }

        Progress(_currentProgress);

        if (_currentProgress >= 1)
        {
            _completed = true;
            OnComplete?.Invoke();
            OnCompleteEvent?.Invoke();
        }
    }

    private void Progress(float value)
    {
        ReleasedFillers.ForEach(f => f.fillAmount = value);
        PressedFillers.ForEach(f => f.fillAmount = value);
    }
}
