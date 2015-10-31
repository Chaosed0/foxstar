using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WinHandler : MonoBehaviour {
    public int pointsToWin = 5;
    public WinnerPanel winPanel;

    public void Initialize(List<PlayerInput> inputs, string[] names) {
        List<ShipMotor> motors = new List<ShipMotor>();
        for (int i = 0; i < inputs.Count; i++) {
            motors.Add(inputs[i].GetComponent<ShipMotor>());
        }

        for (int i = 0; i < inputs.Count; i++) {
            Ship ship = inputs[i].GetComponent<Ship>();
            WinHandlerFunctor functor = new WinHandlerFunctor(names[i], motors, winPanel, pointsToWin);
            ship.OnScoreChange += functor.OnScoreChange;
        }
    }
}

public class WinHandlerFunctor {
    private List<ShipMotor> motors;
    private string name;
    private WinnerPanel winPanel;
    private int pointsToWin;

    public WinHandlerFunctor(string name, List<ShipMotor> motors, WinnerPanel winPanel, int pointsToWin) {
        this.name = name;
        this.motors = motors;
        this.winPanel = winPanel;
        this.pointsToWin = pointsToWin;
    }

    public void OnScoreChange(int score, int change) {
        if (score >= pointsToWin) {
            winPanel.SetWinner(name);
            for (int i = 0; i < motors.Count; i++) {
                motors[i].enabled = false;
            }
        }
    }
}
