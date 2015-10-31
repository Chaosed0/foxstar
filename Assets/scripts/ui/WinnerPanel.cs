using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinnerPanel : MonoBehaviour {
    public Text text;

    private CanvasGroup group;

	void Start () {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0.0f;
	}

    public void SetWinner(string winnerName) {
        text.text = winnerName + " wins!";
        group.alpha = 1.0f;
    }
}
