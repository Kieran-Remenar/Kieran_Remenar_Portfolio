using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridControl : MonoBehaviour
{
	public Action onWin;
	public ButtonUpdater buttonUpdater;

	public enum Difficulty : int { EASY = 4, MEDIUM = 6, HARD = 8 };
	[Tooltip("Difficulty of the puzzle")]
	public Difficulty difficulty;
	[Space]
	[Tooltip("GridPanel prefab")]
	public GameObject panel;
	[Tooltip("Width and Height of individual panels")]
	public int panelSize;
	[Tooltip("Space between panels")]
	public int gapSize;
	[Space]
	[Tooltip("Array of sprites used by the panels")]
	public Sprite[] sprites;
	[Tooltip("Wire color indicators")]
	public GameObject[] indicators;
	[Tooltip("Text attached to each color indicator")]
	public Text[] indicatorText;
	[Space]
	[Tooltip("The background image when a color is not selected")]
	public Sprite indicatorBase;
	[Tooltip("The background image used to show which color is currently selected")]
	public Sprite indicatorSelected;
	[Tooltip("The object displaying the win text")]
	public GameObject winText;
	[Tooltip("Exit button for the puzzle panel")]
	public Button closeButton;

	private EventSystem eventSystem;
	private CablePuzzlePool puzzlePool = new CablePuzzlePool();

	private bool interacting = false;
	private RectTransform gridBackground; //RectTransform attached to this panel
	private GridLayoutGroup gridLayout; //GridLayoutGroup attached to the object; sets up the matrix for the grid panels
	private int matrixSize; //Number of rows and columns in the grid
	private List<List<GridPanel>> gridMatrix; //Functional matrix of the puzzle in [row#][column#]

	private List<Cable> cables = new List<Cable>(); //List of Cables that keep track of colors, lengths, and completion of each Cable color
	private Cable currentCable; //The currently active Cable
	private GridCoord currentCoord;
	private readonly Color[] cableColors = new Color[4] {Color.red, Color.green, Color.yellow, Color.blue }; //Colors of the wires

	//Steam Achievement tracking variable
	private bool resetFree = true;
	
    public void StartGame(Difficulty difficulty)
    {
		this.difficulty = difficulty;
		gridLayout = GetComponent<GridLayoutGroup>(); //Set up the grid for the cells to be aligned in
		gridLayout.cellSize = new Vector2(panelSize, panelSize);
		gridLayout.padding = new RectOffset(gapSize, gapSize, gapSize, gapSize);
		gridLayout.spacing = new Vector2(gapSize, gapSize);

		eventSystem = FindObjectOfType<EventSystem>();

		matrixSize = (int) difficulty;  //set matrix size to the size indicated by puzzle difficulty
		gridBackground = GetComponent<RectTransform>();
		gridBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (panelSize * matrixSize + gapSize * (matrixSize + 1))); //Set size of the grid to fit the difficulty
		gridBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (panelSize * matrixSize + gapSize * (matrixSize + 1)));
		puzzlePool.Initialize(matrixSize);
		gridMatrix = new List<List<GridPanel>>();
		cables = puzzlePool.Cables;

		if (difficulty == Difficulty.EASY) //Scale the puzzle components to match the background size better
		{
			transform.localScale = new Vector3(1.5f, 1.5f, 0f);
		}
		else if (difficulty == Difficulty.HARD)
		{
			transform.localScale = new Vector3(0.8f, 0.8f, 0f);
		}

		buttonUpdater.UpdateButtons(PomInput.IsJoystickConnected());
		GenerateGrid();
		FindFirstSocket();
		currentCable = cables[0];
		for (int i = 0; i < cables.Count; i++)
		{
			indicators[i].SetActive(true);
			indicatorText[i].text = "Length: " + cables[i].GetLength().ToString();
		}
		indicators[0].GetComponent<Image>().sprite = indicatorSelected;

		StartCoroutine(TimePuzzle());
    }

    /// <summary>
    /// Generates the grid for the puzzle and everything on it
    /// </summary>
    private void GenerateGrid()
	{
		for (int i = 0; i < matrixSize; i++) //Instantiate the panels to set up the grid
		{
			gridMatrix.Add(new List<GridPanel>());
			for (int v = 0; v < matrixSize; v++)
			{
				GameObject newObj = Instantiate(panel, transform); //Assign the new panel's script to a unique position in the functional matrix
				gridMatrix[i].Add(newObj.GetComponent<GridPanel>());
				gridMatrix[i][v].coordinates = new GridCoord(v, i);
				gridMatrix[i][v].panelType = puzzlePool.ActiveMatrix[i][v]; //Assign initial properties to each panel from the puzzle pool
				gridMatrix[i][v].Initialize();
			}
		}
	}

	/// <summary>
	/// Times the puzzle for the Steam Achievement "Spend More Than 2 Minutes on a Single Cable Puzzle"
	/// </summary>
	/// <returns>WaitForSeconds(120)</returns>
	IEnumerator TimePuzzle()
	{
		yield return new WaitForSeconds(120);

		Achievements.AchievementGet(Achievement.Fail_Cable);
	}

	/// <summary>
	/// Checks the panel in the specified direction to see if it is clear to move onto
	/// </summary>
	/// <param name="direction">The direction to be checked</param>
	/// <returns>Whether the space in the direction is available to move onto</returns>
	private bool CheckSpace(GridPanel.Direction direction)
	{
		if (direction == GridPanel.Direction.UP)
		{
			if (currentCoord.y - 1 >= 0)
			{
				if (!gridMatrix[currentCoord.y - 1][currentCoord.x].IsWall() && !gridMatrix[currentCoord.y - 1][currentCoord.x].HasPlug())
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}
		else if (direction == GridPanel.Direction.DOWN)
		{
			if (currentCoord.y + 1 < matrixSize)
			{
				if (!gridMatrix[currentCoord.y + 1][currentCoord.x].IsWall() && !gridMatrix[currentCoord.y + 1][currentCoord.x].HasPlug())
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}
		else if (direction == GridPanel.Direction.RIGHT)
		{
			if (currentCoord.x + 1 < matrixSize)
			{
				if (!gridMatrix[currentCoord.y][currentCoord.x + 1].IsWall() && !gridMatrix[currentCoord.y][currentCoord.x + 1].HasPlug())
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}
		else if (direction == GridPanel.Direction.LEFT)
		{
			if (currentCoord.x - 1 >= 0)
			{
				if (!gridMatrix[currentCoord.y][currentCoord.x - 1].IsWall() && !gridMatrix[currentCoord.y][currentCoord.x - 1].HasPlug())
				{
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}
		else
			return false;
	}

	/// <summary>
	/// Finds the first socket in the puzzle and chooses it to be selected
	/// </summary>
	public void FindFirstSocket()
	{
		bool found = false;
		for (int i = 0; i < matrixSize; i++)
		{
			foreach (GridPanel panel in gridMatrix[i])
			{
				if (panel.IsSocket() && !panel.HasPlug())
				{
					eventSystem.SetSelectedGameObject(panel.gameObject);
					found = true;
					break;
				}
			}
			if (found)
			{
				break;
			}
		}
	}

	/// <summary>
	/// Is the puzzle currently in 'Interaction' mode?
	/// </summary>
	/// <returns>Whether this is true</returns>
	public bool IsInteracting()
	{
		return interacting;
	}

	/// <summary>
	/// The puzzle is now in 'Interaction' mode, in which the player can lay cable on the grid
	/// </summary>
	/// <param name="state">Whether the puzzle is now in 'Interaction' mode</param>
	/// <param name="coord">The coordinates of the GridPanel that calls this function</param>
	public void Interaction(bool state, GridCoord coord)
	{
		interacting = state;
		currentCoord = coord;
	}

	/// <summary>
	/// Returns the color of the current Cable
	/// </summary>
	/// <returns>The current Cable color</returns>
	public Color GetColor()
	{
		return currentCable.color;
	}

	/// <summary>
	/// Switches the active cable color
	/// </summary>
	/// <param name="index">The index of the cables list to switch to</param>
	public void UpdateCableState(int index)
	{
		if (!interacting)
		{
			AudioManager.instance.Play(SoundFX.CableSwitch, transform.position);
			currentCable = cables[index];
			for (int i = 0; i < indicators.Length; i++)
			{
				if (i == index)
				{
					indicators[i].GetComponent<Image>().sprite = indicatorSelected;
				}
				else
				{
					indicators[i].GetComponent<Image>().sprite = indicatorBase;
				}
			}
		}
	}

	/// <summary>
	/// Is this Cable completed?
	/// </summary>
	/// <returns>Whether this Cable is completed</returns>
	public bool IsCableComplete()
	{
		return currentCable.complete;
	}

	public Cable GetCurrentCable()
	{
		return currentCable;
	}

	/// <summary>
	/// Call when the Cable is completed or restarted
	/// </summary>
	/// <param name="wire">The wire to be affected</param>
	/// <param name="state">true if being completed, false if restarted</param>
	public void CompleteCable(Cable wire, bool state)
	{
		bool solved = true;
		wire.complete = state;
		if (state)
		{
			indicatorText[wire.id].text = "Connected";
			foreach (Cable cable in cables) //Check to see if all cables have been placed
			{
				if (solved && cable.complete)
					solved = cable.matched;
				else
					solved = false;
			}
			if (solved)
				PuzzleComplete();
			else
			{
				while (currentCable.complete)
				{
					indicators[currentCable.id].GetComponent<Image>().sprite = indicatorBase;
					if (currentCable.id + 1 == cables.Count)
						currentCable = cables[0];
					else
						currentCable = cables[currentCable.id + 1];
					indicators[currentCable.id].GetComponent<Image>().sprite = indicatorSelected;
				}
			}
		}
		else
			indicatorText[wire.id].text = "Length: " + currentCable.GetLength().ToString();
		FindFirstSocket();
	}

	/// <summary>
	/// For when the cable ends on a mismatched socket pair
	/// </summary>
	public void EndMismatch()
	{
		currentCable.complete = true;
		if (currentCable.GetLength() == 0)
		{
			indicatorText[currentCable.id].text = "Mismatch";
		}
		else
		{
			indicatorText[currentCable.id].text = "Oversized";
		}
	}

	/// <summary>
	/// Call when the puzzle has been completed
	/// </summary>
	public void PuzzleComplete()
	{
		AudioManager.instance.Play(SoundFX.CablePuzzleWin, transform.position);
		eventSystem.SetSelectedGameObject(null);
		winText.SetActive(true);

		StopCoroutine(TimePuzzle()); //Steam Achievement Tracking
		if (difficulty == Difficulty.HARD && resetFree) 
		{
			Achievements.AchievementGet(Achievement.Success_Cable);
		}

		if (onWin != null)
		{
			onWin();
		}
	}

	/// <summary>
	/// Move back onto the previous space to override the previous move
	/// </summary>
	/// <param name="dir">The 'dir' variable, NOT the oldDirection variable</param>
	public void Backtrack(GridPanel.Direction dir)
	{
		if (dir == GridPanel.Direction.UP)
		{
			currentCoord = new GridCoord(currentCoord.x, currentCoord.y - 1);
		}
		else if (dir == GridPanel.Direction.DOWN)
		{
			currentCoord = new GridCoord(currentCoord.x, currentCoord.y + 1);
		}
		else if (dir == GridPanel.Direction.RIGHT)
		{
			currentCoord = new GridCoord(currentCoord.x + 1, currentCoord.y);
		}
		else
		{
			currentCoord = new GridCoord(currentCoord.x - 1, currentCoord.y);
		}
		gridMatrix[currentCoord.y][currentCoord.x].EraseWire();
		gridMatrix[currentCoord.y][currentCoord.x].Select(dir, true);
		currentCable.ChangeLength(1, indicatorText[currentCable.id]);
	}

	/// <summary>
	/// Clears all cables of the currently selected color
	/// </summary>
	public void ResetCable()
	{
		for (int i = 0; i < matrixSize; i++)
		{
			for (int v = 0; v < matrixSize; v++)
			{
				if (!gridMatrix[i][v].IsWall())
				{
					gridMatrix[i][v].ClearColor();
				}
			}
		}
		CompleteCable(currentCable, false);
		currentCable.Reset(indicatorText[currentCable.id]);
		interacting = false;
		resetFree = false;
		AudioManager.instance.Play(SoundFX.CableReset, transform.position);
		FindFirstSocket();
	}

	/// <summary>
	/// Clears all wires and plugs on the board
	/// </summary>
	public void ResetPuzzle()
	{
		for (int i = 0; i < matrixSize; i++)
		{
			for (int v = 0; v < matrixSize; v++)
			{
				if (!gridMatrix[i][v].IsWall())
				{
					gridMatrix[i][v].Clear();
				}
			}
		}
		foreach (Cable cable in cables)
		{
			CompleteCable(cable, false);
			cable.Reset(indicatorText[cable.id]);
		}
		interacting = false;
		resetFree = false;
		AudioManager.instance.Play(SoundFX.CableReset, transform.position);
		FindFirstSocket();
	}
	
    void Update()
    {
		if (!interacting && PomInput.CableSelect)
		{
			eventSystem.currentSelectedGameObject?.GetComponent<GridPanel>()?.PlacePlug();
		}
		if (interacting && currentCable.GetLength() > 0) //Determines where and how to place wires as the player moves around the grid
		{
			if (PomInput.CableUp)
			{
				MoveUp();
			}
			if (PomInput.CableLeft)
			{
				MoveLeft();
			}
			if (PomInput.CableDown)
			{
				MoveDown();
			}
			if (PomInput.CableRight)
			{
				MoveRight();
			}
		}
		else //Allows the player to switch between cable colors when not currently placing a cable
		{
			if (PomInput.CablePrev)
			{
				int cableIndex = currentCable.id - 1;
				if (cableIndex < 0)
					cableIndex = cables.Count - 1;
				UpdateCableState(cableIndex);
			}
			else if (PomInput.CableNext)
			{
				int cableIndex = (currentCable.id + 1) % cables.Count;
				UpdateCableState(cableIndex);
			}
		}

		if (PomInput.CableResetSingle) //Resets the wire color currently selected
		{
			ResetCable();
		}
		if (PomInput.CableResetAll) //Resets the puzzle
		{
			ResetPuzzle();
		}
		if (PomInput.ToggleInterface)
		{
			StopCoroutine(TimePuzzle());
		}
	}

	public void MoveUp()
	{
		if (interacting && currentCable.GetLength() > 0 && CheckSpace(GridPanel.Direction.UP))
		{
			if (gridMatrix[currentCoord.y][currentCoord.x].Deselect(GridPanel.Direction.UP))
			{
				currentCoord = new GridCoord(currentCoord.x, currentCoord.y - 1);
				currentCable.ChangeLength(-1, indicatorText[currentCable.id]);
				gridMatrix[currentCoord.y][currentCoord.x].Select(GridPanel.Direction.DOWN, false);
			}
		}
	}

	public void MoveLeft()
	{
		if (interacting && currentCable.GetLength() > 0 && CheckSpace(GridPanel.Direction.LEFT))
		{
			if (gridMatrix[currentCoord.y][currentCoord.x].Deselect(GridPanel.Direction.LEFT))
			{
				currentCoord = new GridCoord(currentCoord.x - 1, currentCoord.y);
				currentCable.ChangeLength(-1, indicatorText[currentCable.id]);
				gridMatrix[currentCoord.y][currentCoord.x].Select(GridPanel.Direction.RIGHT, false);
			}
		}
	}

	public void MoveDown()
	{
		if (interacting && currentCable.GetLength() > 0 && CheckSpace(GridPanel.Direction.DOWN))
		{
			if (gridMatrix[currentCoord.y][currentCoord.x].Deselect(GridPanel.Direction.DOWN))
			{
				currentCoord = new GridCoord(currentCoord.x, currentCoord.y + 1);
				currentCable.ChangeLength(-1, indicatorText[currentCable.id]);
				gridMatrix[currentCoord.y][currentCoord.x].Select(GridPanel.Direction.UP, false);
			}
		}
	}

	public void MoveRight()
	{
		if (interacting && currentCable.GetLength() > 0 && CheckSpace(GridPanel.Direction.RIGHT))
		{
			if (gridMatrix[currentCoord.y][currentCoord.x].Deselect(GridPanel.Direction.RIGHT))
			{
				currentCoord = new GridCoord(currentCoord.x + 1, currentCoord.y);
				currentCable.ChangeLength(-1, indicatorText[currentCable.id]);
				gridMatrix[currentCoord.y][currentCoord.x].Select(GridPanel.Direction.LEFT, false);
			}
		}
	}
}

public class Cable
{
	public readonly Color color;
	private int length, initLength;
	public readonly int id;
	public bool complete, matched;

	public Cable(int ID, Color cableColor)
	{
		color = cableColor;
		initLength = 0;
		length = initLength;
		id = ID;
		complete = false;
		matched = false;
	}

	/// <summary>
	/// Changes the length of the cable.
	/// </summary>
	/// <param name="change">The amount to change the length by (should be 1 or -1)</param>
	/// <param name="text">The text to update</param>
	public void ChangeLength(int change, Text text)
	{
		length += change;
		text.text = "Length: " + length.ToString();
	}

	/// <summary>
	/// Returns the current cable length
	/// </summary>
	/// <returns>Current cable length</returns>
	public int GetLength()
	{
		return length;
	}

	/// <summary>
	/// Sets the starting cable length
	/// </summary>
	/// <param name="cableLength">CableLength</param>
	public void SetLength(int cableLength)
	{
		initLength = cableLength;
		length = initLength;
	}

	/// <summary>
	/// Resets the cable length to the value it was initialized to
	/// </summary>
	/// <param name="text">The text to update</param>
	public void Reset(Text text)
	{
		length = initLength;
		text.text = "Length: " + length.ToString();
	}
}