using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class stackedDeck : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bomb;
    public KMAudio audio;
	
	public KMSelectable b_card;
	public KMSelectable b_submit;
	public Material[] m_cards;
	public TextMesh t_target;
	
	private bool isSolved;
	private static int moduleCount;
    private int moduleId;
	
	private int target_card;
	private int start_card;
	private int selected_deck = 6;
	
	private int[][] decks = new int[][] {
		new int[] { 11, 21, 31, 41, 51, 61, 71, 81, 91, 101, 111, 121, 131, 13, 23, 33, 43, 53, 63, 73, 83, 93, 103, 113, 123, 133, 14, 24, 34, 44, 54, 64, 74, 84, 94, 104, 114, 124, 134, 12, 22, 32, 42, 52, 62, 72, 82, 92, 102, 112, 122, 132},
		new int[] { 54, 83, 111, 12, 44, 73, 101, 132, 34, 63, 91, 122, 24, 53, 81, 112, 14, 43, 71, 102, 134, 33, 61, 92, 124, 23, 51, 82, 114, 13, 41, 72, 104, 133, 31, 62, 94, 123, 21, 52, 84, 113, 11, 42, 74, 103, 131, 32, 64, 93, 121, 22},
		new int[] { 63, 94, 122, 21, 53, 84, 112, 11, 43, 74, 102, 131, 33, 64, 92, 121, 23, 54, 82, 111, 13, 44, 72, 101, 133, 34, 62, 91, 123, 24, 52, 81, 113, 14, 42, 71, 103, 134, 32, 61, 93, 124, 22, 51, 83, 114, 12, 41, 73, 104, 132, 31},
		new int[] { 101, 83, 44, 12, 112, 61, 71, 94, 62, 11, 111, 83, 54, 22, 122, 33, 133, 104, 72, 21, 121, 93, 64, 32, 132, 43, 14, 114, 82, 31, 131, 103, 74, 42, 81, 53, 24, 124, 92, 41, 13, 113, 84, 52, 91, 63, 34, 134, 102, 51, 23, 123},
		new int[] { 81, 133, 34, 102, 21, 73, 94, 52, 121, 43, 14, 62, 111, 83, 134, 32, 101, 23, 74, 92, 51, 123, 44, 12, 61, 113, 84, 132, 31, 103, 24, 72, 91, 53, 124, 42, 11, 63, 114, 82, 131, 33, 104, 22, 71, 93, 54, 122, 41, 13, 64, 112},
		new int[] { 41, 23, 72, 31, 43, 62, 14, 53, 94, 24, 123, 32, 121, 83, 64, 54, 93, 131, 22, 113, 34, 84, 63, 101, 52, 132, 21, 33, 82, 51, 134, 112, 81, 104, 133, 111, 74, 103, 12, 44, 73, 42, 11, 91, 114, 122, 71, 124, 102, 61, 13, 92},
		new int[] { 11, 12, 13, 14, 21, 22, 23, 24, 31, 32, 33, 34, 41, 42, 43, 44, 51, 52, 53, 54, 61, 62, 63, 64, 71, 72, 73, 74, 81, 82, 83, 84, 91, 92, 93, 94, 101, 102, 103, 104, 111, 112, 113, 114, 121, 122, 123, 124, 131, 132, 133, 134}
	};

	void Start () {
		Debug.LogFormat("[Stacked Deck #{0}] Module started.", moduleId);
		Reset();
	}
	
	void Awake () {
		moduleId = moduleCount++;
		
		b_card.OnInteract += delegate () { nextCard(); return false; };
		b_submit.OnInteract += delegate () { Submit(); return false; };
	}
	
	private void Reset () {
		target_card = (UnityEngine.Random.Range(1,14) * 10) + UnityEngine.Random.Range(1,5);
		t_target.text = getCardString(target_card);
		
		do {
			start_card = (UnityEngine.Random.Range(1,14) * 10) + UnityEngine.Random.Range(1,5);
		} while (target_card == start_card);
		
		int m_start = System.Array.IndexOf(decks[6], start_card);
		b_card.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(175f,185f), 0);
		b_card.GetComponent<MeshRenderer>().sharedMaterial = m_cards[m_start];
		
		Debug.LogFormat("[Stacked Deck #{0}] Starting card is {1}.", moduleId, getCardString(start_card));
		
		getDeck();
	}
	
	private void getDeck () {
		int lastn = bomb.GetSerialNumberNumbers().Last();
		string[] deck_names = new string[] {"New Deck", "Charles Gauci", "Si Stebbins", "Bart Harding", "Eight Threatning King", "Mnemonica", "N/A"};
		selected_deck = 6;
		if (bomb.IsIndicatorOn("BOB") && bomb.GetBatteryCount() > 2) target_card = start_card;
		else if (bomb.IsIndicatorOn("FRK") && bomb.IsPortPresent(Port.Parallel)) selected_deck = 0;
		else if (bomb.GetPortPlateCount() > 2 && target_card % 10 == 4) selected_deck = 1;
		else if (bomb.IsPortPresent(Port.Serial) && bomb.GetSerialNumberLetters().Any(x => x == 'S' || x == 'R' || x == 'L' || x == 'P' || x == 'T')) selected_deck = 5;
		else if (lastn == 2 || lastn == 3 || lastn == 5 || lastn == 7) selected_deck = 2;
		else if (start_card % 10 == target_card % 10) selected_deck = 3;
		else if (bomb.GetSerialNumberLetters().Any(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U')) selected_deck = 4;
		else if (bomb.GetSerialNumberLetters().Count() < bomb.GetSerialNumberNumbers().Count()) {
			int to_add = bomb.GetSerialNumberNumbers().Sum();
			Debug.LogFormat("[Stacked Deck #{0}] Need to press it {1} times.", moduleId, to_add);
			target_card = decks[6][(System.Array.IndexOf(decks[6], start_card) + to_add) % 52];
		}
		else {
			Debug.LogFormat("[Stacked Deck #{0}] Need to press it 11 times.", moduleId);
			target_card = decks[6][(System.Array.IndexOf(decks[6], start_card) + 11) % 52];
		}
		Debug.LogFormat("[Stacked Deck #{0}] Using deck: {1}.", moduleId, deck_names[selected_deck]);
	}
	
	private void nextCard () {
		b_card.AddInteractionPunch(0.25f);
		audio.PlaySoundAtTransform("next", transform);
		int card_i = System.Array.IndexOf(decks[selected_deck], start_card) + 1;
		start_card = decks[selected_deck][card_i % 52];
		
		b_card.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(175f,185f), 0);
		b_card.GetComponent<MeshRenderer>().sharedMaterial = m_cards[52];
		
		if (isSolved) {
			int m_start = System.Array.IndexOf(decks[6], start_card);
			b_card.GetComponent<MeshRenderer>().sharedMaterial = m_cards[m_start];
		}
	}
	
	private void Submit () {
		if (isSolved) return;
		
		Debug.LogFormat("[Stacked Deck #{0}] Target card is {1}, pressed on card {2}.", moduleId, getCardString(target_card), getCardString(start_card));
		
		b_submit.AddInteractionPunch();
		if (start_card != target_card) {
			Debug.LogFormat("[Stacked Deck #{0}] Wrong card!", moduleId);
			audio.PlaySoundAtTransform("shuffle", transform);
			module.HandleStrike();
			Reset();
			return;
		}
		
		Debug.LogFormat("[Stacked Deck #{0}] Module solved!", moduleId);
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		int card_i = System.Array.IndexOf(decks[6], start_card);
		b_card.GetComponent<MeshRenderer>().sharedMaterial = m_cards[card_i];
		audio.PlaySoundAtTransform("shuffle", transform);
		isSolved = true;
		module.HandlePass();
		
	}
	
	private string getCardString (int card) {
		string val;
		string suit;
		
		switch (card / 10) {
		case 11:
			val = "J";
			break;
		case 12:
			val = "Q";
			break;
		case 13:
			val = "K";
			break;
		case 1:
			val = "A";
			break;
		default:
			val = (card/10).ToString();
			break;
		}
		switch (card % 10) {
		case 1:
			suit = "♣";
			break;
		case 2:
			suit = "♦";
			break;
		case 3:
			suit = "♥";
			break;
		default:
			suit = "♠";
			break;
		}
		
		return (val + suit);
	}


#pragma warning disable IDE0051 // Remove unused private members
    private readonly string TwitchHelpMessage = "!{0} draw/deal ## [Draws that many cards by selecting the big screen.] | !{0} submit [Submits the face down card.]";
#pragma warning restore IDE0051 // Remove unused private members

    IEnumerator ProcessTwitchCommand(string command)
    {
		var rgxMatch = Regex.Match(command, @"^(d(eal|raw))\s[0-9]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (command.EqualsIgnoreCase("submit"))
        {
			yield return null;
			b_submit.OnInteract();
        }
		else if (command.EqualsIgnoreCase("draw") || command.EqualsIgnoreCase("deal"))
        {
			yield return null;
			b_card.OnInteract();
        }
		else if (rgxMatch.Success)
        {
			var numLast = rgxMatch.Value.Split().Last();
			int drawCnt;
			if (!int.TryParse(numLast, out drawCnt) || drawCnt < 1 || drawCnt >= 52)
            {
				yield return string.Format("sendtochaterror I cannot understand why you want me to draw {0} card(s)!", numLast);
				yield break;
            }
			yield return null;
            for (var x = 0; x < drawCnt; x++)
            {
				b_card.OnInteract();
				yield return new WaitForSeconds(0.1f);
            }
        }
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		while (start_card != target_card)
        {
			b_card.OnInteract();
			yield return new WaitForSeconds(0.1f);
        }
		b_submit.OnInteract();
    }

}
