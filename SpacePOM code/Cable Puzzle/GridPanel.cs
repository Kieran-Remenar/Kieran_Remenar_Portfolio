using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridPanel : MonoBehaviour
{
	public enum Direction : int { UP, RIGHT, DOWN, LEFT };
	public enum SpriteList : int { WALL, PLUG, STRAIGHT, BENT, SOCKET, END_PLUG}; //Must be in the same order as in the sprite array on GridControl to generate the correct image
	[Tooltip("The panel will use this to draw pictures over itself")]
	public GameObject spriteBase;
	public GridCoord coordinates; //This panel's coordinates on the grid
	public char panelType = 'n'; //assigned in the CablePuzzlePool, used to determine the panel's default properties.

	private GridControl master;
	private bool isWall = false, isSocket = false, hasPlug = false;
	private Button button; //This panel's Button component
	private Image image; //This panel's image component
	private Sprite objSprite; //This panel's base sprite
	private Color matchColor = Color.clear;
	private Direction direction; //The direction from which this panel is selected during gameplay
	private Stack<Image> childImage = new Stack<Image>(); //This panel's children, from spriteBase

	private void Awake()
	{
		master = GetComponentInParent<GridControl>();
		button = GetComponent<Button>();
		image = GetComponent<Image>();
		objSprite = image.sprite;
	}

	public void Initialize()
	{
		switch (panelType)
		{
			case 'w': AddWall(); break;
			case 'r': AddSocket(); matchColor = Color.red; break;
			case 'g': AddSocket(); matchColor = Color.green; break;
			case 'y': AddSocket(); matchColor = Color.yellow; break;
			case 'b': AddSocket(); matchColor = Color.blue; break;
			case 'n': break;
			default: Debug.LogError("Unrecognized char passed"); break;
		}
	}

	/// <summary>
	/// When this panel has been moved onto by the player
	/// </summary>
	/// <param name="dir">Direction moved to get here. If backtracking, pass any GridPanel.Direction</param>
	/// <param name="isBacktracking">Is the player backtracking?</param>
	public void Select(Direction dir, bool isBacktracking)
	{
		GameObject newImage = Instantiate(spriteBase, transform);
		childImage.Push(newImage.GetComponent<Image>());
		childImage.Peek().sprite = master.sprites[(int)SpriteList.END_PLUG];
		childImage.Peek().color = master.GetColor();
		AudioManager.instance.Play(SoundFX.CableMovement, transform.position);
		if (dir == Direction.LEFT)
		{
			childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 90f));
		}
		else if (dir == Direction.DOWN)
		{
			childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 180f));
		}
		else if (dir == Direction.RIGHT)
		{
			childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 270f));
		}

		if (isSocket)
		{
			master.Interaction(false, coordinates);
			hasPlug = true;
			button.interactable = false;
			if (matchColor == master.GetColor() && master.GetCurrentCable().matched)
			{
				master.CompleteCable(master.GetCurrentCable(), true);
				master.FindFirstSocket();
			}
			else
			{
				master.GetCurrentCable().matched = false;
				master.EndMismatch();
			}
		}

		if (!isBacktracking)
			direction = dir;
	}

	/// <summary>
	/// When this panel has been moved off of by the player
	/// </summary>
	/// <param name="dir">The Direction in which the player has just moved</param>
	/// <returns>Is the player moving forwards?</returns>
	public bool Deselect(Direction dir)
	{
		if (!hasPlug) //DO NOT run logic if this is deselecting the starting plug
		{
			Destroy(childImage.Pop().gameObject);
			if (dir == direction)//If the player exits the space from the same side s/he entered
			{
				master.Backtrack(dir);
				return false;
			}
			else if (Mathf.Abs((int)dir - (int)direction) == 2) //If the player continues in the same direction
			{
				GameObject newImage = Instantiate(spriteBase, transform);
				childImage.Push(newImage.GetComponent<Image>());
				childImage.Peek().sprite = master.sprites[(int)SpriteList.STRAIGHT];
				childImage.Peek().color = master.GetColor();
				if (dir == Direction.RIGHT || dir == Direction.LEFT)
				{
					childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 90f));
				}
				return true;
			}
			else
			{
				GameObject newImage = Instantiate(spriteBase, transform);
				childImage.Push(newImage.GetComponent<Image>());
				childImage.Peek().sprite = master.sprites[(int)SpriteList.BENT];
				childImage.Peek().color = master.GetColor();
				if ((dir == Direction.RIGHT && direction == Direction.UP) || (dir == Direction.UP && direction == Direction.RIGHT)) //Wire looks like └
				{
					childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 90f));
				}
				else if ((dir == Direction.UP && direction == Direction.LEFT) || (dir == Direction.LEFT && direction == Direction.UP)) //Wire looks like ┘
				{
					childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 180f));
				}
				else if ((dir == Direction.DOWN && direction == Direction.LEFT) || (dir == Direction.LEFT && direction == Direction.DOWN)) //Wire looks like ┐
				{
					childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 270f));
				} //Else case: wire looks like┌ (default rotation)
				return true;
			}
		}
		else //If this is a plug, change to the plug w/ wire and rotate accordingly
		{
			Destroy(childImage.Pop().gameObject);
			GameObject newImage = Instantiate(spriteBase, transform);
			childImage.Push(newImage.GetComponent<Image>());
			childImage.Peek().sprite = master.sprites[(int)SpriteList.END_PLUG];
			childImage.Peek().color = master.GetColor();
			if (dir == Direction.LEFT)
			{
				childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 90f));
			}
			else if (dir == Direction.DOWN)
			{
				childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 180f));
			}
			else if (dir == Direction.RIGHT)
			{
				childImage.Peek().rectTransform.Rotate(new Vector3(0f, 0f, 270f));
			}
			return true;
		}
	}

	/// <summary>
	/// Erases the top wire on a tile
	/// </summary>
	public void EraseWire()
	{
		if (childImage.Count > 0)
		{
			Destroy(childImage.Pop().gameObject);
		}
		else
		{
			Debug.LogError("Tried to Deselect Empty Wire at " + coordinates.y.ToString() + ", " + coordinates.x.ToString());
		}
	}

	/// <summary>
	/// Clears the board of the color selected in the Grid Control
	/// </summary>
	public void ClearColor()
	{
		Stack<Image> tempStack = new Stack<Image>();
		if (hasPlug)
		{
			if (childImage.Peek().color == master.GetColor())
			{
				Destroy(childImage.Pop().gameObject);
				hasPlug = false;
				button.interactable = true;
			}
		}
		else
		{
			while (childImage.Count > 0)
			{
				if (childImage.Peek().color == master.GetColor())
					Destroy(childImage.Pop().gameObject);
				else
					tempStack.Push(childImage.Pop());
			}
			while (tempStack.Count > 0)
			{
				childImage.Push(tempStack.Pop());
			}
		}
	}

	/// <summary>
	/// Clears the board of all plugs and wires
	/// </summary>
	public void Clear()
	{
		if (!isWall)
		{
			while (childImage.Count > 0)
			{
				Destroy(childImage.Pop().gameObject);
				hasPlug = false;
				if (isSocket)
					button.interactable = true;
			}
		}
	}

	/// <summary>
	/// Puts down a plug on this socket
	/// </summary>
	public void PlacePlug()
	{
		if (isSocket && !hasPlug && !master.IsInteracting() && !master.IsCableComplete())
		{
			AudioManager.instance.Play(SoundFX.CableCompletion, transform.position);
			GameObject newImage = Instantiate(spriteBase, transform);
			childImage.Push(newImage.GetComponent<Image>());
			childImage.Peek().sprite = master.sprites[(int)SpriteList.PLUG];
			childImage.Peek().color = master.GetColor();
			hasPlug = true;
			master.Interaction(true, coordinates);
			button.interactable = false;
			if (matchColor == master.GetColor())
			{
				master.GetCurrentCable().matched = true;
			}
			else
			{
				master.GetCurrentCable().matched = false;
			}
		}
	}

	/// <summary>
	/// Does this have a plug on it?
	/// </summary>
	/// <returns>Whether this panel has a plug on it</returns>
	public bool HasPlug()
	{
		return hasPlug;
	}

	/// <summary>
	/// Changes the panel to be an obstacle in the grid
	/// </summary>
	public void AddWall()
	{
		isWall = true;
		image.sprite = master.sprites[(int)SpriteList.WALL];
		Destroy(button);
	}

	/// <summary>
	/// Is this panel a wall?
	/// </summary>
	/// <returns>Returns true if it is a wall; false if not</returns>
	public bool IsWall()
	{
		return isWall;
	}

	/// <summary>
	/// Makes this GridPanel a socket
	/// </summary>
	public void AddSocket()
	{
		button.interactable = true;
		image.sprite = master.sprites[(int)SpriteList.SOCKET];
		isSocket = true;
	}

	/// <summary>
	/// Is this panel a socket?
	/// </summary>
	/// <returns>Returns true if it is a wall; false if not</returns>
	public bool IsSocket()
	{
		if (isSocket)
			return true;
		else
			return false;
	}
}

/// <summary>
/// Basically a Vec2 but with integers
/// </summary>
public class GridCoord
{
	public readonly int x, y;
	//Keep in mind that the coordinates will usually be used in the format (y, x) because of how the grid layout group works
	
	/// <summary>
	/// Constructs a coordinate with the given x and y
	/// </summary>
	/// <param name="setX">Column number of this coordinate</param>
	/// <param name="setY">Row number of this coordinate</param>
	public GridCoord(int setX, int setY)
	{
		x = setX;
		y = setY;
	}

	public override string ToString()
	{
		return y + ", " + x;
	}
}
