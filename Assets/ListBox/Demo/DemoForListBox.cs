using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DemoForListBox : MonoBehaviour {

	public Text displayText;
	public Image displayMood;
	public ListBox listBoxMood;
	public ListBox listBoxBonsai;
	public ListBox listBoxTool;

	private string[] textParts = new string[3] {"I am in a        mood. So I will work on my\n", " bonsai tree with a ", "."};

	void Start () {
		listBoxMood.onValueChanged = this.MoodChanged;
		listBoxBonsai.onValueChanged = this.BonsaiChanged;
		listBoxTool.onValueChanged = this.ToolChanged;
	}
	
	public void MoodChanged (ListBox aListBox) {
		displayMood.sprite = aListBox.valueSprite;
	}
	public void BonsaiChanged (ListBox aListBox) {
		BuildString();
	}
	public void ToolChanged (ListBox aListBox) {
		BuildString();
	}

	private void BuildString() {
		string bonsai = listBoxBonsai.valueString;
		string tool = listBoxTool.valueString;
		if (bonsai == null) bonsai = "(select a tree type)";
		if (tool == null) tool = "(select a tool)";
		displayText.text = textParts[0] + bonsai.ToLower() + textParts[1] + tool.ToLower() + textParts[2];
	}
}
