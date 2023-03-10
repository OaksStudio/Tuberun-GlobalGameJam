using System;
using System.Collections;
using System.Collections.Generic;
using Jozi.Utilities.Patterns;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [Header("Default Info")]
    public List<SOPuller> DefaultCompetitors = new List<SOPuller>();

    [Header("Tubers")]
    public List<SOTuber> MatchTubers = new List<SOTuber>();

    [Header("Hidden")]
    [SerializeField, ReadOnly] private GameMode _gameMode;
    [SerializeField, ReadOnly] private List<Puller> _pullers = new List<Puller>();


    public List<SOPuller> Competitors
    {
        get
        {
            if (CharacterSelect.SelectedCompetitors.Count <= 0) return DefaultCompetitors;
            return CharacterSelect.SelectedCompetitors;
        }
    }

    public List<int> CompetitorsPoints => _competitorsPoints;
    [ReadOnly, SerializeField] private List<int> _competitorsPoints = new List<int>();

    public List<float> CompetitorsTime => _competitorsTime;
    [ReadOnly, SerializeField] private List<float> _competitorsTime = new List<float>();

    public List<Puller> Pullers => _pullers;
    public GameMode GameMode => _gameMode;

    public UnityEvent OnWonEvent, OnLoseEvent;

    public Action<SOPuller> OnWon;
    public Action OnLost;
    public Action<int> OnSetTime, OnAddPoint;

    private void Start()
    {
        _gameMode = FindObjectOfType<GameMode>();
        _competitorsPoints = new List<int>();
        _competitorsTime = new List<float>();
        for (int i = 0; i < Competitors.Count; i++)
        {
            GameObject pullerHolder = new GameObject();
            pullerHolder.transform.parent = transform;
            if (Competitors[i] is SOPlayerPuller playerPuller)
            {
                pullerHolder.name = $"P_{playerPuller.name}";
                PlayerPuller puller = pullerHolder.AddComponent<PlayerPuller>();
                puller.Setup(i, playerPuller);
                _pullers.Add(puller);
            }
            else if (Competitors[i] is SOBotPuller botPuller)
            {
                pullerHolder.name = $"B_{botPuller.name}";
                BotPuller puller = pullerHolder.AddComponent<BotPuller>();
                puller.Setup(i, botPuller);
                _pullers.Add(puller);
            }

            _competitorsPoints.Add(0);
            _competitorsTime.Add(0);
        }

        _gameMode.Setup(MatchTubers);
        _gameMode.OnOneWins += Won;
        _gameMode.OnAllLost += Losted;
    }

    [Button]
    public void AddPoint(int value, int competitorIndex)
    {
        CompetitorsPoints[competitorIndex] += value;
        OnAddPoint?.Invoke(competitorIndex);
    }

    [Button]
    public void SetTime(float time, int competitorIndex)
    {
        CompetitorsTime[competitorIndex] = time;
        OnSetTime?.Invoke(competitorIndex);
    }

    [Button]
    private void Won(int pullerIndex)
    {
        PauseManager.Instance.CanPause = false;
        DisablePullers();
        OnWon?.Invoke(Competitors[pullerIndex]);
        OnWonEvent?.Invoke();
    }

    [Button]
    private void Losted()
    {
        PauseManager.Instance.CanPause = false;
        DisablePullers();
        OnLost?.Invoke();
        OnLoseEvent?.Invoke();
    }

    private void DisablePullers()
    {
        foreach (var puller in Pullers)
        {
            puller.Disable();
        }
    }

}
