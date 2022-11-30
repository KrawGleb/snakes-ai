using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Snake : Agent
{
    [SerializeField] private Transform goal;
    [SerializeField] private Transform parent;
    [SerializeField] private Transform segmentPrefab;
    [SerializeField] private int initialSize = 2;

    private static int _currentMaxScore = 0;

    private Vector2 _direction = Vector2.right;
    private readonly List<Transform> _segments = new List<Transform>();
    private float _distanceToGoal = 0f;
    
    private void Start()
    {
        ResetState();
    }

    public void Move(int direction)
    {
        switch (direction)
        {
            case 0: _direction = Vector2.up; break;
            case 1: _direction = Vector2.right; break;
            case 2: _direction = Vector2.down; break;
            case 3: _direction = Vector2.left; break;
            default: break;
        }
    }

    private void FixedUpdate()
    {
        for (int i = _segments.Count - 1; i > 0; i--)
            _segments[i].localPosition = _segments[i - 1].localPosition;
        
        transform.localPosition = new Vector3(
            Mathf.Round(transform.localPosition.x) + _direction.x,
            Mathf.Round(transform.localPosition.y) + _direction.y,
            0.0f
        );

        var newDistanceToGoal = Vector3.Distance(transform.localPosition, goal.localPosition);

        if (newDistanceToGoal < _distanceToGoal)
            AddReward(0.001f);
        else
            AddReward(-0.01f);

        _distanceToGoal = newDistanceToGoal;
    }

    public void ResetState()
    {
        for (int i = 1; i < _segments.Count; i++)
            Destroy(_segments[i].gameObject);

        _segments.Clear();
        _segments.Add(transform);
        transform.localPosition = Vector3.zero;

        for (int i = 0; i < initialSize; i++)
            Grow();
    }

    private void Grow()
    {
        var segment = Instantiate(segmentPrefab, parent);
        segment.localPosition = _segments[_segments.Count - 1].localPosition;

        _segments.Add(segment);
    }

    public override void OnEpisodeBegin()
    {
        ResetState();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var dirToGoal = (goal.localPosition - transform.localPosition).normalized;

        sensor.AddObservation(dirToGoal.x);
        sensor.AddObservation(dirToGoal.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var moveDirection = actions.DiscreteActions[0];

        Move(moveDirection);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W))
            discreteActions[0] = 0;
        else if (Input.GetKey(KeyCode.S))
            discreteActions[0] = 2;
        else if (Input.GetKey(KeyCode.D))
            discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.A))
            discreteActions[0] = 3;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Grow();
            AddReward(10f);
            Debug.Log("Reward");

            if (_segments.Count > _currentMaxScore)
            {
                _currentMaxScore = _segments.Count;
                Debug.LogWarning($"New max score: {_currentMaxScore}");
            }

        }
        else if (other.CompareTag("Obstacle"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
}
